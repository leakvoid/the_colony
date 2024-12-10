using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionScheduler : MonoBehaviour
{
    Globals globals;
    BuildingPlacementManager bpm;

    int[] pressure;
    bool[] canBuild;
    int[] buildingCount;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        bpm = FindObjectOfType<BuildingPlacementManager>();
    }

    void Start()
    {
        int count = Enum.GetValues(typeof(BuildingTag)).Length;

        pressure = new int[count];
        canBuild = new bool[count];
        buildingCount = new int[count];

        for (int i = 0; i < count; i++)
            canBuild[i] = true;
    }

    void AddBuildingPressure(BuildingTag buildingTag, int amount = 1)
    {
        pressure[(int)buildingTag] += amount;
    }

    void ReduceBuildingPressure(BuildingTag buildingTag)
    {
        ProductionBT bt = (ProcessingBT)globals.NameToTemplate(buildingTag);
        switch (buildingTag)
        {
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
                pressure[(int)buildingTag] -= (int)grace; // TODO timeInterval from requesting building ?
                break;
            // Consumption
            case BuildingTag.SaltMine:
            case BuildingTag.Clothier:
            case BuildingTag.HuntersCabin:
            case BuildingTag.FishingHut:
            case BuildingTag.Bakery:
            case BuildingTag.Brewery:
                float consumptionPeriod = 100 * globals.needsConsumptionInterval / globals.needsAmountDecreased;
                grace = (bt.constructionTime + consumptionPeriod) / bt.timeInterval * bt.amountProducedPerInterval;
                pressure[(int)buildingTag] -= (int)(grace * consumptionPeriod / globals.engineNeedCheckInterval);
                break;
        }
    }

    public void AddResourcePressure(ResourceType resourceType, int amount = 1)
    {
        switch (resourceType)
        {
            // Construction
            case ResourceType.Wood:
                AddBuildingPressure(BuildingTag.Sawmill, amount);
                break;
            case ResourceType.Stone:
                AddBuildingPressure(BuildingTag.StoneMine, amount);
                break;
            case ResourceType.Tools:
                AddBuildingPressure(BuildingTag.Forge, amount);
                break;
            // Raw
            case ResourceType.Iron:
                AddBuildingPressure(BuildingTag.IronMine, amount);
                break;
            case ResourceType.Cotton:
                AddBuildingPressure(BuildingTag.CottonPlantation, amount);
                break;
            case ResourceType.Wheat:
                AddBuildingPressure(BuildingTag.WheatFarm, amount);
                break;
            case ResourceType.Hops:
                AddBuildingPressure(BuildingTag.HopsFarm, amount);
                break;
            case ResourceType.Flour:
                AddBuildingPressure(BuildingTag.Windmill, amount);
                break;
            // Consumption
            case ResourceType.Salt:
                AddBuildingPressure(BuildingTag.SaltMine, amount);
                break;
            case ResourceType.Cloth:
                AddBuildingPressure(BuildingTag.Clothier, amount);
                break;
            case ResourceType.Food:
                AddBuildingPressure(BuildingTag.HuntersCabin, amount);
                AddBuildingPressure(BuildingTag.FishingHut, amount);
                AddBuildingPressure(BuildingTag.Bakery, amount);
                break;
            case ResourceType.Beer:
                AddBuildingPressure(BuildingTag.Brewery, amount);
                break;
            default:
                throw new Exception("Resource " + nameof(ResourceType) + " not handled");
        }
    }

    enum ConstructionState
    {
        NotNeeded,
        InsufficientGold,
        InsufficientMaterials,
        Success
    }

    int GetThreshold(BuildingTag buildingTag)
    {
        if (buildingTag == BuildingTag.Well)
            return 5;
        if (buildingTag == BuildingTag.Church)
            return 15;
        if (buildingTag == BuildingTag.Inn)
            return 20;
        return 1;
    }

    ConstructionState TryBuilding(BuildingTag buildingTag)
    {
        if (!canBuild[(int)buildingTag] || pressure[(int)buildingTag] < GetThreshold(buildingTag))
            return ConstructionState.NotNeeded;

        BuildingTemplate bt = globals.NameToTemplate(buildingTag);
        if (bt.goldCost > globals.goldAmount)
            return ConstructionState.InsufficientGold;
        if (bt.woodCost > globals.woodAmount)
        {
            AddResourcePressure(ResourceType.Wood, bt.woodCost);
            return ConstructionState.InsufficientMaterials;
        }
        if (bt.stoneCost > globals.stoneAmount)
        {
            AddResourcePressure(ResourceType.Stone, bt.stoneCost);
            return ConstructionState.InsufficientMaterials;
        }
        if (bt.toolsCost > globals.toolsAmount)
        {
            AddResourcePressure(ResourceType.Tools, bt.toolsCost);
            return ConstructionState.InsufficientMaterials;
        }
        
        canBuild[(int)buildingTag] = bpm.Build(buildingTag);
        if (!canBuild[(int)buildingTag])
            return ConstructionState.NotNeeded;

        switch (bt.buildingType)
        {
            case BuildingType.Housing:
                break;
            case BuildingType.Service:
                break;
            case BuildingType.Processing:
            case BuildingType.Farming:
            case BuildingType.ResourceGathering:
                ReduceBuildingPressure(buildingTag);
                break;
            default:
                throw new Exception("Unknown building type");
        }

        return ConstructionState.Success;
    }

    public void MakeBuildings()
    {
        // Services
        TryBuilding(BuildingTag.Market);
        TryBuilding(BuildingTag.Well);
        TryBuilding(BuildingTag.Church);
        TryBuilding(BuildingTag.Inn);

        // Houses when worker shortage
        // if idle citizens < 20
        // build house

        // if ? (gold is also needed -> don't increase; increase construction resource pressure if enough gold)
        // build wood, stone, tools

        // if raw needed
        // build raw

        // if consumption need
        // build consumption
        // else
        // build or upgrade house (up to limit ?)
    }
}
