using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class ConstructionScheduler : MonoBehaviour
{
    Globals globals;
    BuildingManager bm;
    BuildingLocationModule blm;// TODO refactor

    int[] pressure;
    bool[] canBuild;
    int[] buildingCount;

    List<BuildingTag> buildingPriorityList;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        bm = FindObjectOfType<BuildingManager>();
        blm = FindObjectOfType<BuildingLocationModule>();
    }

    void Start()
    {
        int count = Enum.GetValues(typeof(BuildingTag)).Length;

        pressure = new int[count];
        canBuild = new bool[count];//TODO maybe should be deleted
        buildingCount = new int[count];

        for (int i = 0; i < count; i++)
            canBuild[i] = true;

        buildingPriorityList = new List<BuildingTag>()
        {
            // service
            BuildingTag.Market,
            BuildingTag.Well,
            BuildingTag.Church,
            BuildingTag.Inn,
            // housing
            BuildingTag.House,
            // raw
            BuildingTag.IronMine,
            BuildingTag.CottonPlantation,
            BuildingTag.HopsFarm,
            BuildingTag.WheatFarm,
            BuildingTag.Windmill,
            // consumption
            BuildingTag.FishingHut,
            BuildingTag.HuntersCabin,
            BuildingTag.Brewery,
            BuildingTag.Clothier,
            BuildingTag.Bakery,
            BuildingTag.SaltMine,
        };
    }

    void IncreaseBuildingPressure(BuildingTag buildingTag, int amount = 1)
    {
        pressure[(int)buildingTag] += amount;
    }

    void ReduceBuildingPressure(BuildingTag buildingTag)
    {
        ProductionBT bt = (ProcessingBT)globals.NameToTemplate(buildingTag);
        switch (buildingTag)
        {
            // Housing
            case BuildingTag.House:
                pressure[(int)buildingTag] -= 1;
                break;
            // Service
            case BuildingTag.Market:
            case BuildingTag.Church:
            case BuildingTag.Well:
            case BuildingTag.Inn:
                pressure[(int)buildingTag] -= blm.GetServiceCoverValue();
                break;
            // Construction
            case BuildingTag.Sawmill:
            case BuildingTag.StoneMine:
            case BuildingTag.Forge:
                float grace = (bt.constructionTime + 30) * bt.amountProducedPerInterval;
                pressure[(int)buildingTag] -= (int)(grace / globals.engineConstructionInterval);
                break;
            // Raw
            case BuildingTag.IronMine:
            case BuildingTag.CottonPlantation:
            case BuildingTag.WheatFarm:
            case BuildingTag.HopsFarm:
            case BuildingTag.Windmill:
                grace = (bt.constructionTime + 30) / bt.timeInterval * bt.amountProducedPerInterval;
                pressure[(int)buildingTag] -= (int)grace;
                break;
            // Consumption
            case BuildingTag.SaltMine:
            case BuildingTag.Clothier:
            case BuildingTag.Brewery:
                float consumptionPeriod = 100 * globals.needsConsumptionInterval / globals.needsAmountDecreased;
                grace = (bt.constructionTime + consumptionPeriod) / bt.timeInterval * bt.amountProducedPerInterval;
                pressure[(int)buildingTag] -= (int)(grace * consumptionPeriod / globals.engineNeedCheckInterval);
                break;
            case BuildingTag.HuntersCabin:
            case BuildingTag.FishingHut:
            case BuildingTag.Bakery:
                consumptionPeriod = 100 * globals.needsConsumptionInterval / globals.needsAmountDecreased;
                grace = (bt.constructionTime + consumptionPeriod) / bt.timeInterval * bt.amountProducedPerInterval;
                int reducedAmount = (int)(grace * consumptionPeriod / globals.engineNeedCheckInterval);
                pressure[(int)BuildingTag.HuntersCabin] -= reducedAmount;
                pressure[(int)BuildingTag.FishingHut] -= reducedAmount;
                pressure[(int)BuildingTag.Bakery] -= reducedAmount;
                break;
        }
    }

    public void IncreaseResourcePressure(ResourceType resourceType, int amount = 1)
    {
        switch (resourceType)
        {
            // Construction
            case ResourceType.Wood:
                IncreaseBuildingPressure(BuildingTag.Sawmill, amount);
                break;
            case ResourceType.Stone:
                IncreaseBuildingPressure(BuildingTag.StoneMine, amount);
                break;
            case ResourceType.Tools:
                IncreaseBuildingPressure(BuildingTag.Forge, amount);
                break;
            // Raw
            case ResourceType.Iron:
                IncreaseBuildingPressure(BuildingTag.IronMine, amount);
                break;
            case ResourceType.Cotton:
                IncreaseBuildingPressure(BuildingTag.CottonPlantation, amount);
                break;
            case ResourceType.Wheat:
                IncreaseBuildingPressure(BuildingTag.WheatFarm, amount);
                break;
            case ResourceType.Hops:
                IncreaseBuildingPressure(BuildingTag.HopsFarm, amount);
                break;
            case ResourceType.Flour:
                IncreaseBuildingPressure(BuildingTag.Windmill, amount);
                break;
            // Consumption
            case ResourceType.Salt:
                IncreaseBuildingPressure(BuildingTag.SaltMine, amount);
                break;
            case ResourceType.Cloth:
                IncreaseBuildingPressure(BuildingTag.Clothier, amount);
                break;
            case ResourceType.Food:
                IncreaseBuildingPressure(BuildingTag.HuntersCabin, amount);
                IncreaseBuildingPressure(BuildingTag.FishingHut, amount);
                IncreaseBuildingPressure(BuildingTag.Bakery, amount);
                break;
            case ResourceType.Beer:
                IncreaseBuildingPressure(BuildingTag.Brewery, amount);
                break;
            default:
                throw new Exception("Resource " + nameof(ResourceType) + " not handled");
        }
    }

    int GetPressureThreshold(BuildingTag buildingTag)
    {
        if (buildingTag == BuildingTag.Well)
            return 5;
        if (buildingTag == BuildingTag.Church)
            return 15;
        if (buildingTag == BuildingTag.Inn)
            return 20;
        return 1;
    }

    enum ConstructionState
    {
        NotNeeded,
        InsufficientGold,
        InsufficientMaterials,
        Success
    }

    bool woodPressureOccurred;
    bool stonePressureOccurred;
    bool toolsPressureOccurred;

    ConstructionState TryBuilding(BuildingTag buildingTag)
    {
        // TODO make multiple buildings of the same type at once
        if (!canBuild[(int)buildingTag] || pressure[(int)buildingTag] < GetPressureThreshold(buildingTag))
            return ConstructionState.NotNeeded;

        BuildingTemplate bt = globals.NameToTemplate(buildingTag);

        if (bt.goldCost > globals.goldAmount)
            return ConstructionState.InsufficientGold;

        bool insufficientMaterials = false;// TODO maybe max ?
        if (bt.woodCost > globals.woodAmount)
        {
            if (!woodPressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Wood, bt.woodCost);
                TryBuilding(BuildingTag.Sawmill);
                woodPressureOccurred = true;
            }
            insufficientMaterials = true;
        }
        if (bt.stoneCost > globals.stoneAmount)
        {
            if (!stonePressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Stone, bt.stoneCost);
                TryBuilding(BuildingTag.StoneMine);
                stonePressureOccurred = true;
            }

            insufficientMaterials = true;
        }
        if (bt.toolsCost > globals.toolsAmount)
        {
            if (!toolsPressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Tools, bt.toolsCost);
                TryBuilding(BuildingTag.Forge);
                toolsPressureOccurred = true;
            }
            insufficientMaterials = true;
        }
        if (insufficientMaterials)
            return ConstructionState.InsufficientMaterials;
        
        BuildingData buildingData = bm.StartBuildingConstruction(bt);
        if (buildingData == null)
            return ConstructionState.NotNeeded;

        if (buildingTag == BuildingTag.House)
        {
            BuildingTag[] serviceTags = new BuildingTag[] {BuildingTag.Market, BuildingTag.Well, BuildingTag.Church, BuildingTag.Inn};
            foreach (var serviceTag in serviceTags)
                if (!blm.CheckServiceOverlap(buildingData.gridLocation, serviceTag))
                    IncreaseBuildingPressure(serviceTag);
        }

        ReduceBuildingPressure(buildingTag);

        return ConstructionState.Success;
    }

    public void MakeBuildings()
    {
        woodPressureOccurred = false;
        stonePressureOccurred = false;
        toolsPressureOccurred = false;

        foreach(var buildingTag in buildingPriorityList)
        {
            // TODO if no idle workers -> return
            if (TryBuilding(buildingTag) == ConstructionState.InsufficientGold)
                return;
        }

        if (!woodPressureOccurred && !stonePressureOccurred && !toolsPressureOccurred)
        {
            // TODO try upgrading
            TryBuilding(BuildingTag.House);
        }
    }
}
