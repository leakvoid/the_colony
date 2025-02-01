using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public int GetBuildingPressure(BuildingTag buildingTag)
    {
        return pressure[(int)buildingTag];
    }

    public void ReduceBuildingPressure(BuildingTemplate buildingTemplate, float walkingTime)
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
                float grace = (bt.ConstructionTime + 2 * walkingTime + 30) * bt.AmountProducedPerInterval;
                pressure[(int)buildingTemplate.BuildingTag] -= (int)(grace / globals.EngineConstructionInterval);
                break;
            // Raw
            case BuildingTag.IronMine:
            case BuildingTag.CottonPlantation:
            case BuildingTag.WheatFarm:
            case BuildingTag.HopsFarm:
            case BuildingTag.Windmill:
                bt = (ProductionBT)buildingTemplate;
                grace = (bt.ConstructionTime + 2 * walkingTime + 30) * bt.AmountProducedPerInterval / bt.TimeInterval;
                pressure[(int)buildingTemplate.BuildingTag] -= (int)grace;
                break;
            // Consumption
            case BuildingTag.SaltMine:
            case BuildingTag.Clothier:
            case BuildingTag.Brewery:
                bt = (ProductionBT)buildingTemplate;
                float consumptionPeriod = globals.NeedAmountReplenished * globals.NeedConsumptionInterval / globals.NeedAmountDecrement;
                grace = (bt.ConstructionTime + 2 * walkingTime + consumptionPeriod) / bt.TimeInterval * bt.AmountProducedPerInterval;
                pressure[(int)buildingTemplate.BuildingTag] -= (int)(grace * consumptionPeriod / globals.EngineNeedCheckInterval);
                break;
            case BuildingTag.HuntersCabin:
            case BuildingTag.FishingHut:
            case BuildingTag.Bakery:
                bt = (ProductionBT)buildingTemplate;
                consumptionPeriod = globals.NeedAmountReplenished * globals.NeedConsumptionInterval / globals.NeedAmountDecrement;
                grace = (bt.ConstructionTime + 2 * walkingTime + consumptionPeriod) / bt.TimeInterval * bt.AmountProducedPerInterval;
                int reducedAmount = (int)(grace * consumptionPeriod / globals.EngineNeedCheckInterval);
                pressure[(int)BuildingTag.HuntersCabin] -= reducedAmount;
                pressure[(int)BuildingTag.FishingHut] -= reducedAmount;
                pressure[(int)BuildingTag.Bakery] -= reducedAmount;
                if (buildingTemplate.BuildingTag == BuildingTag.Bakery)
                {
                    IncreaseResourceBuildingPressure(BuildingTag.Windmill, 1);
                    IncreaseResourceBuildingPressure(BuildingTag.WheatFarm, 1);
                }
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
                IncreaseResourceBuildingPressure(BuildingTag.IronMine, amount);
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
                IncreaseResourceBuildingPressure(BuildingTag.WheatFarm, amount);
                break;
            // Consumption
            case ResourceType.Salt:
                IncreaseResourceBuildingPressure(BuildingTag.SaltMine, amount);
                break;
            case ResourceType.Cloth:
                IncreaseResourceBuildingPressure(BuildingTag.Clothier, amount);
                IncreaseResourceBuildingPressure(BuildingTag.CottonPlantation, amount);
                break;
            case ResourceType.Food:
                IncreaseResourceBuildingPressure(BuildingTag.HuntersCabin, amount);
                IncreaseResourceBuildingPressure(BuildingTag.FishingHut, amount);
                IncreaseResourceBuildingPressure(BuildingTag.Bakery, amount);
                break;
            case ResourceType.Beer:
                IncreaseResourceBuildingPressure(BuildingTag.Brewery, amount);
                IncreaseResourceBuildingPressure(BuildingTag.HopsFarm, amount);
                break;
            default:
                throw new Exception("Resource " + resourceType.ToString() + " not handled");
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

    bool goldPressureOccurred;
    bool woodPressureOccurred;
    bool stonePressureOccurred;
    bool toolsPressureOccurred;

    bool TryBuilding(BuildingTag buildingTag)
    {
        if (pressure[(int)buildingTag] < GetPressureThreshold(buildingTag))
            return false;

        BuildingTemplate bt = globals.NameToTemplate(buildingTag);

        if (bt.GoldCost > globals.goldAmount)
        {
            goldPressureOccurred = true;
            return false;
        }

        bool insufficientMaterials = false;
        if (bt.WoodCost > globals.woodAmount)
        {
            if (!woodPressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Wood, bt.WoodCost);
                woodPressureOccurred = true;
            }
            insufficientMaterials = true;
        }
        if (bt.StoneCost > globals.stoneAmount)
        {
            if (!stonePressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Stone, bt.StoneCost);
                stonePressureOccurred = true;
            }

            insufficientMaterials = true;
        }
        if (bt.ToolsCost > globals.toolsAmount)
        {
            if (!toolsPressureOccurred)
            {
                IncreaseResourcePressure(ResourceType.Tools, bt.ToolsCost);
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

        return true;
    }

    public void MakeBuildings()
    {
        goldPressureOccurred = false;
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
        }
        while (constructedCount > 0);

        if (!goldPressureOccurred && !woodPressureOccurred && !stonePressureOccurred && !toolsPressureOccurred)
        {
            UpgradeRandomHouses();

            IncreaseBuildingPressure(BuildingTag.House, 5);
            for (int i = 0; i < 5; i++)
                TryBuilding(BuildingTag.House);
        }
    }

    void UpgradeRandomHouses()
    {
        int maxT1Upgrades = 0, maxT2Upgrades = 0;
        var houses = bm.GetAllHouses();
        foreach(var house in houses)
        {
            if (house.upgradeTier == 0)
                maxT1Upgrades++;
            else if (house.upgradeTier == 1)
                maxT2Upgrades++;
        }

        int tier1Wood = globals.woodAmount / globals.HouseTemplate.Tier1UpgradeWoodCost;
        int tier1Gold = globals.goldAmount / globals.HouseTemplate.Tier1UpgradeGoldCost;
        int tier1Upgraded = Mathf.Min(tier1Wood, tier1Gold, maxT1Upgrades);
        int tier2Stone = globals.stoneAmount / globals.HouseTemplate.Tier2UpgradeStoneCost;
        int tier2Gold = (globals.goldAmount - tier1Upgraded * globals.HouseTemplate.Tier1UpgradeGoldCost) / globals.HouseTemplate.Tier2UpgradeGoldCost;
        int tier2Upgraded = Mathf.Min(tier2Stone, tier2Gold, maxT2Upgrades);

        foreach(var house in houses)
        {
            if (tier1Upgraded > 0 && house.upgradeTier == 0)
            {
                bm.UpgradeHouse(house);
                tier1Upgraded--;
            }
            if (tier2Upgraded > 0 && house.upgradeTier == 1)
            {
                bm.UpgradeHouse(house);
                tier2Upgraded--;
            }
        }
    }
}
