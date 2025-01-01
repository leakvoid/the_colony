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
    int[] buildingCount;

    List<BuildingTag> buildingPriorityList;

    BuildingTag[] serviceTags = new BuildingTag[] {BuildingTag.Market, BuildingTag.Well, BuildingTag.Church, BuildingTag.Inn};

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        bm = FindObjectOfType<BuildingManager>();
        blm = FindObjectOfType<BuildingLocationModule>();
    }

    public void Initialize()
    {
        int count = Enum.GetValues(typeof(BuildingTag)).Length;

        pressure = new int[count];
        buildingCount = new int[count];

        buildingPriorityList = new List<BuildingTag>()
        {
            // service
            BuildingTag.Market,
            BuildingTag.Well,
            BuildingTag.Church,
            BuildingTag.Inn,
            // housing
            BuildingTag.House,
            // construction
            BuildingTag.Sawmill,
            BuildingTag.StoneMine,
            BuildingTag.Forge,
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

        bm.PlaceStartingHouse();
        foreach (var serviceTag in serviceTags)
            IncreaseBuildingPressure(serviceTag);
    }

    public void IncreaseBuildingPressure(BuildingTag buildingTag, int amount = 1)
    {
        pressure[(int)buildingTag] += amount;
    }

    void ReduceBuildingPressure(BuildingTemplate buildingTemplate)
    {
        switch (buildingTemplate.BuildingTag)
        {
            // Housing
            case BuildingTag.House:
                pressure[(int)buildingTemplate.BuildingTag] -= 1;
                break;
            // Service
            case BuildingTag.Market:
            case BuildingTag.Church:
            case BuildingTag.Well:
            case BuildingTag.Inn:
                pressure[(int)buildingTemplate.BuildingTag] -= blm.GetServiceCoverValue();
                break;
            // Construction
            case BuildingTag.Sawmill:
            case BuildingTag.StoneMine:
            case BuildingTag.Forge:
                ProductionBT bt = (ProductionBT)buildingTemplate;
                float grace = (bt.ConstructionTime + 30) * bt.AmountProducedPerInterval;// TODO (+ walk time from house to work) * 2
                pressure[(int)buildingTemplate.BuildingTag] -= (int)(grace / globals.EngineConstructionInterval);
                break;
            // Raw
            case BuildingTag.IronMine:
            case BuildingTag.CottonPlantation:
            case BuildingTag.WheatFarm:
            case BuildingTag.HopsFarm:
            case BuildingTag.Windmill:
                bt = (ProductionBT)buildingTemplate;
                grace = (bt.ConstructionTime + 30) / bt.TimeInterval * bt.AmountProducedPerInterval;
                pressure[(int)buildingTemplate.BuildingTag] -= (int)grace;
                break;
            // Consumption
            case BuildingTag.SaltMine:
            case BuildingTag.Clothier:
            case BuildingTag.Brewery:
                bt = (ProductionBT)buildingTemplate;
                float consumptionPeriod = 100 * globals.NeedConsumptionInterval / globals.NeedAmountDecrement;
                grace = (bt.ConstructionTime + consumptionPeriod) / bt.TimeInterval * bt.AmountProducedPerInterval;
                pressure[(int)buildingTemplate.BuildingTag] -= (int)(grace * consumptionPeriod / globals.EngineNeedCheckInterval);
                break;
            case BuildingTag.HuntersCabin:
            case BuildingTag.FishingHut:
            case BuildingTag.Bakery:
                bt = (ProductionBT)buildingTemplate;
                consumptionPeriod = 100 * globals.NeedConsumptionInterval / globals.NeedAmountDecrement;
                grace = (bt.ConstructionTime + consumptionPeriod) / bt.TimeInterval * bt.AmountProducedPerInterval;
                int reducedAmount = (int)(grace * consumptionPeriod / globals.EngineNeedCheckInterval);
                pressure[(int)BuildingTag.HuntersCabin] -= reducedAmount;
                pressure[(int)BuildingTag.FishingHut] -= reducedAmount;
                pressure[(int)BuildingTag.Bakery] -= reducedAmount;
                break;
        }
    }

    void IncreaseResourceBuildingPressure(BuildingTag buildingTag, int amount = 1)
    {
        if (pressure[(int)buildingTag] <= 0)
            pressure[(int)buildingTag] += amount;
    }

    public void IncreaseResourcePressure(ResourceType resourceType, int amount = 1)
    {
        switch (resourceType)
        {
            // Construction
            case ResourceType.Wood:
                IncreaseResourceBuildingPressure(BuildingTag.Sawmill, amount);
                break;
            case ResourceType.Stone:
                IncreaseResourceBuildingPressure(BuildingTag.StoneMine, amount);
                break;
            case ResourceType.Tools:
                IncreaseResourceBuildingPressure(BuildingTag.Forge, amount);
                break;
            // Raw
            case ResourceType.Iron:
                IncreaseResourceBuildingPressure(BuildingTag.IronMine, amount);
                break;
            case ResourceType.Cotton:
                IncreaseResourceBuildingPressure(BuildingTag.CottonPlantation, amount);
                break;
            case ResourceType.Wheat:
                IncreaseResourceBuildingPressure(BuildingTag.WheatFarm, amount);
                break;
            case ResourceType.Hops:
                IncreaseResourceBuildingPressure(BuildingTag.HopsFarm, amount);
                break;
            case ResourceType.Flour:
                IncreaseResourceBuildingPressure(BuildingTag.Windmill, amount);
                break;
            // Consumption
            case ResourceType.Salt:
                IncreaseResourceBuildingPressure(BuildingTag.SaltMine, amount);
                break;
            case ResourceType.Cloth:
                IncreaseResourceBuildingPressure(BuildingTag.Clothier, amount);
                break;
            case ResourceType.Food:
                IncreaseResourceBuildingPressure(BuildingTag.HuntersCabin, amount);
                IncreaseResourceBuildingPressure(BuildingTag.FishingHut, amount);
                IncreaseResourceBuildingPressure(BuildingTag.Bakery, amount);
                break;
            case ResourceType.Beer:
                IncreaseResourceBuildingPressure(BuildingTag.Brewery, amount);
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

    bool woodPressureOccurred;
    bool stonePressureOccurred;
    bool toolsPressureOccurred;

    bool TryBuilding(BuildingTag buildingTag)// TODO needs refactoring
    {
        if (pressure[(int)buildingTag] < GetPressureThreshold(buildingTag))
            return false;

        BuildingTemplate bt = globals.NameToTemplate(buildingTag);

        if (bt.GoldCost > globals.goldAmount)
            return false;

        bool insufficientMaterials = false;
        if (bt.WoodCost > globals.woodAmount)
        {
            if (!woodPressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Wood, globals.SawmillTemplate.AmountProducedPerInterval);
                woodPressureOccurred = true;
            }
            insufficientMaterials = true;
        }
        if (bt.StoneCost > globals.stoneAmount)
        {
            if (!stonePressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Stone, globals.StoneMineTemplate.AmountProducedPerInterval);
                stonePressureOccurred = true;
            }

            insufficientMaterials = true;
        }
        if (bt.ToolsCost > globals.toolsAmount)
        {
            if (!toolsPressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Tools, globals.ForgeTemplate.AmountProducedPerInterval);
                toolsPressureOccurred = true;
            }
            insufficientMaterials = true;
        }
        if (insufficientMaterials)
            return false;
        
        BuildingData buildingData = bm.StartBuildingConstruction(bt);
        if (buildingData == null)
            return false;

        if (buildingTag == BuildingTag.House)
        {
            foreach (var serviceTag in serviceTags)
                if (!blm.CheckServiceOverlap(buildingData.gridLocation, serviceTag))
                    IncreaseBuildingPressure(serviceTag);
        }

        ReduceBuildingPressure(bt);

        return true;
    }

    public void MakeBuildings()
    {
        woodPressureOccurred = false;
        stonePressureOccurred = false;
        toolsPressureOccurred = false;

        int constructedCount;
        do
        {
            constructedCount = 0;
            foreach (var buildingTag in buildingPriorityList)
            {
                if (TryBuilding(buildingTag))
                    constructedCount++;
            }
            print("Constructed count: " + constructedCount);
        }
        while (constructedCount > 0);

        print("flags: " + woodPressureOccurred + " " + stonePressureOccurred + " " + toolsPressureOccurred);
        if (!woodPressureOccurred && !stonePressureOccurred && !toolsPressureOccurred)
        {
            print("Try building a house");
            // TODO try upgrading && while loop here like above
            IncreaseBuildingPressure(BuildingTag.House, 5);
            for (int i = 0; i < 5; i++)
                TryBuilding(BuildingTag.House);
        }
    }
}
