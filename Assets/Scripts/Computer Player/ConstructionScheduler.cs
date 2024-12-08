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

    void Start()
    {
        int buildingTagNumber = Enum.GetValues(typeof(BuildingTag)).Length;
    }

    public void AddBuildingPressure(BuildingTag buildingTag, int amount = 1)
    {
        switch (buildingTag)
        {
            // service
            case BuildingTag.Market:
                break;
            case BuildingTag.Church:
                break;
            case BuildingTag.Inn:
                break;
            case BuildingTag.Well:
                break;
        }
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
            case ResourceType.Meat:
                AddBuildingPressure(BuildingTag.HuntersCabin, amount);
                break;
            case ResourceType.Fish:
                AddBuildingPressure(BuildingTag.FishingHut, amount);
                break;
            case ResourceType.Bread:
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
        // always try to build houses

        // low income -> push services, needs production
        // high income -> push construction resource production
        // low population -> increase population
    }
}
