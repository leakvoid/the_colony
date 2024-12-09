using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionScheduler : MonoBehaviour
{
    // housing
    [SerializeField] HousingBT houseTemplate;

    // service
    [SerializeField] ServiceBT marketTemplate;
    [SerializeField] ServiceBT churchTemplate;
    [SerializeField] ServiceBT innTemplate;
    [SerializeField] ServiceBT wellTemplate;

    // farming
    [SerializeField] FarmingBT cottonPlantationTemplate;
    [SerializeField] FarmingBT hopsFarmTemplate;
    [SerializeField] FarmingBT wheatFarmTemplate;

    // processing
    [SerializeField] ProcessingBT bakeryTemplate;
    [SerializeField] ProcessingBT breweryTemplate;
    [SerializeField] ProcessingBT clothierTemplate;
    [SerializeField] ProcessingBT forgeTemplate;
    [SerializeField] ProcessingBT windmillTemplate;

    // gathering
    [SerializeField] ResourceGatheringBT fishingHutTemplate;
    [SerializeField] ResourceGatheringBT huntersCabinTemplate;
    [SerializeField] ResourceGatheringBT ironMineTemplate;
    [SerializeField] ResourceGatheringBT saltMineTemplate;
    [SerializeField] ResourceGatheringBT sawmillTemplate;
    [SerializeField] ResourceGatheringBT stoneMineTemplate;

    int[] pressure;

    void Start()
    {
        int buildingTagNumber = Enum.GetValues(typeof(BuildingTag)).Length;

        pressure = new int[buildingTagNumber];
    }

    public void AddBuildingPressure(BuildingTag buildingTag, int amount = 1)
    {
        pressure[(int)buildingTag] += amount;
    }

    public void AddResourcePressure(ResourceType resourceType, int amount = 1)
    {
        switch (resourceType)
        {
            // construction
            case ResourceType.Wood:
                AddBuildingPressure(BuildingTag.Sawmill, amount);
                break;
            case ResourceType.Stone:
                AddBuildingPressure(BuildingTag.StoneMine, amount);
                break;
            case ResourceType.Tools:
                AddBuildingPressure(BuildingTag.Forge, amount);
                break;
            // raw
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
            // consumption
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

    public void MakeBuildings()
    {
        // if 1 house
        // build market
        // if 5 house
        // build well
        // if 15 houses
        // build church
        // if 20 houses
        // build inn

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
