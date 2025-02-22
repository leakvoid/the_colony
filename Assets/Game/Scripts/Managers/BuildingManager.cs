using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] BuildingData buildingDataPrefab;

    Globals globals;
    BuildingLocationModule blm;
    ColonistManager cm;
    ConstructionScheduler constructionScheduler;
    RoadPathModule rpm;

    List<BuildingData> allBuildings;
    List<BuildingData> allHouses;
    Queue<BuildingData> emptyWorkableBuildings;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        blm = FindObjectOfType<BuildingLocationModule>();
        cm = FindObjectOfType<ColonistManager>();
        constructionScheduler = FindObjectOfType<ConstructionScheduler>();// TODO only for computer player
        rpm = FindObjectOfType<RoadPathModule>();

        allBuildings = new List<BuildingData>();
        allHouses = new List<BuildingData>();
        emptyWorkableBuildings = new Queue<BuildingData>();
    }

    public List<BuildingData> GetAllHouses()
    {
        return allHouses;
    }

    public void PlaceStartingHouse()
    {
        BuildingTemplate bt = globals.HouseTemplate;

        (int x, int y) location = blm.PickNewBuildingLocation(bt);

        BuildingData buildingData = Instantiate(buildingDataPrefab);
        buildingData.template = bt;
        buildingData.gridLocation = location;
        buildingData.isConstructed = true;
        buildingData.modelReference = Instantiate(
            bt.FinishedModel,
            Globals.GridToGlobalCoordinates(location, bt.FinishedModel),
            Quaternion.identity
        );
        blm.UpdateAfterBuildingCreation(buildingData, bt);

        buildingData.modelReference.transform.parent = buildingData.transform;

        allBuildings.Add(buildingData);
        allHouses.Add(buildingData);

        buildingData.roadLocation = rpm.SetFirstRoad(location, bt);
        Camera.main.transform.position = new Vector3(location.x, 20f, location.y);

        FinishBuildingConstruction(buildingData);
    }

    public BuildingData StartBuildingConstruction(BuildingTemplate bt)
    {
        if (bt.GoldCost > globals.goldAmount ||
            bt.WoodCost > globals.woodAmount ||
            bt.StoneCost > globals.stoneAmount ||
            bt.ToolsCost > globals.toolsAmount)
            return null;

        if (cm.GetJoblessColonistCount() < 1)
            return null;

        (int x, int y) location = blm.PickNewBuildingLocation(bt);
        if (location == (-1, -1))
            return null;

        BuildingData buildingData = Instantiate(buildingDataPrefab);
        buildingData.template = bt;
        buildingData.gridLocation = location;
        buildingData.modelReference = Instantiate(
            bt.UnfinishedModel,
            Globals.GridToGlobalCoordinates(location, bt.UnfinishedModel),
            Quaternion.identity
        );
        blm.UpdateAfterBuildingCreation(buildingData, bt);

        allBuildings.Add(buildingData);

        buildingData.modelReference.transform.parent = buildingData.transform;

        globals.goldAmount -= bt.GoldCost;
        globals.woodAmount -= bt.WoodCost;
        globals.stoneAmount -= bt.StoneCost;
        globals.toolsAmount -= bt.ToolsCost;

        buildingData.roadLocation = rpm.BuildRoad(location, bt);

        cm.SendColonistToBuild(buildingData);

        return buildingData;
    }

    public void FinishBuildingConstruction(BuildingData buildingData)
    {
        buildingData.isConstructed = true;
        Destroy(buildingData.modelReference);
        buildingData.modelReference = Instantiate(
            buildingData.template.FinishedModel,
            Globals.GridToGlobalCoordinates(buildingData.gridLocation, 
                buildingData.template.FinishedModel),
            Quaternion.identity
        );

        buildingData.modelReference.transform.parent = buildingData.transform;

        if (buildingData.template.BuildingType == BuildingType.Housing)
        {
            HousingBT bt = (HousingBT)buildingData.template;
            for (int i = 0; i < bt.Tier0ColonistCapacity; i++)
                buildingData.colonists.Add(cm.CreateColonist(buildingData));
            
            allHouses.Add(buildingData);
        }
        else
        {
            emptyWorkableBuildings.Enqueue(buildingData);
        }

        AssignJobsToColonists();
    }

    void AssignJobsToColonists()
    {
        while(emptyWorkableBuildings.Count > 0)
        {
            BuildingData buildingData = emptyWorkableBuildings.Peek();
            WorkableBT bt = (WorkableBT)buildingData.template;
            if (bt.MaxNumberOfWorkers > cm.GetJoblessColonistCount())
                return;
            for (int i = 0; i < bt.MaxNumberOfWorkers; i++)
                cm.SendColonistToWork(buildingData);
            emptyWorkableBuildings.Dequeue();
        }
    }

    public void OnWorkerArrival(ColonistData colonistData)
    {
        BuildingData building = colonistData.worksAt;
        building.colonists.Add(colonistData);

        if (building.colonists.Count == 1)
            StartCoroutine(BuildingProductionRoutine(building));
    }

    IEnumerator BuildingProductionRoutine(BuildingData buildingData)
    {
        void HandleProduction(BuildingTemplate buildingTemplate, ref int producedResource)
        {
            ProductionBT bt = (ProductionBT)buildingTemplate;
            producedResource += bt.AmountProducedPerInterval;
        }

        void HandleProcessing(BuildingTemplate buildingTemplate, ref int producedResource, ref int consumedResource)
        {
            ProcessingBT bt = (ProcessingBT)buildingTemplate;
            if (consumedResource < bt.AmountConsumedPerInterval)
            {
                if (constructionScheduler)
                    constructionScheduler.IncreaseResourcePressure(bt.ConsumedResource, bt.AmountConsumedPerInterval);
                return;
            }
            consumedResource -= bt.AmountConsumedPerInterval;
            producedResource += bt.AmountProducedPerInterval;
        }

        WorkableBT bt = (WorkableBT)buildingData.template;
        var waitTimeInterval = new WaitForSeconds(bt.TimeInterval);
        while (true)
        {
            yield return waitTimeInterval;
            foreach (var colonist in buildingData.colonists)
            {
                globals.goldAmount -= bt.Salary;
                colonist.MoneyEarned += bt.Salary;
            }

            switch (bt.BuildingTag)
            {
                case BuildingTag.CottonPlantation:
                    HandleProduction(bt, ref globals.cottonAmount);
                    break;
                case BuildingTag.HopsFarm:
                    HandleProduction(bt, ref globals.hopsAmount);
                    break;
                case BuildingTag.WheatFarm:
                    HandleProduction(bt, ref globals.wheatAmount);
                    break;
                case BuildingTag.Bakery:
                    HandleProcessing(bt, ref globals.breadAmount, ref globals.flourAmount);
                    break;
                case BuildingTag.Brewery:
                    HandleProcessing(bt, ref globals.beerAmount, ref globals.hopsAmount);
                    break;
                case BuildingTag.Clothier:
                    HandleProcessing(bt, ref globals.clothAmount, ref globals.cottonAmount);
                    break;
                case BuildingTag.Forge:
                    HandleProcessing(bt, ref globals.toolsAmount, ref globals.ironAmount);
                    break;
                case BuildingTag.Windmill:
                    HandleProcessing(bt, ref globals.flourAmount, ref globals.wheatAmount);
                    break;
                case BuildingTag.FishingHut:
                    HandleProduction(bt, ref globals.fishAmount);
                    break;
                case BuildingTag.HuntersCabin:
                    HandleProduction(bt, ref globals.meatAmount);
                    break;
                case BuildingTag.IronMine:
                    HandleProduction(bt, ref globals.ironAmount);
                    break;
                case BuildingTag.SaltMine:
                    HandleProduction(bt, ref globals.saltAmount);
                    break;
                case BuildingTag.Sawmill:
                    HandleProduction(bt, ref globals.woodAmount);
                    break;
                case BuildingTag.StoneMine:
                    HandleProduction(bt, ref globals.stoneAmount);
                    break;
            }
        }
    }

    public void UpgradeHouse(BuildingData house)
    {
        HousingBT bt = (HousingBT)house.template;
        if (house.upgradeTier == 0)
        {
            if (globals.goldAmount < bt.Tier1UpgradeGoldCost || globals.woodAmount < bt.Tier1UpgradeWoodCost)
                return;

            for (int i = 0; i < (bt.Tier1ColonistCapacity - bt.Tier0ColonistCapacity); i++)
                house.colonists.Add(cm.CreateColonist(house));
            foreach(var colonist in house.colonists)
                colonist.type = ColonistData.Type.Citizen;
            house.upgradeTier = 1;

            globals.goldAmount -= bt.Tier1UpgradeGoldCost;
            globals.woodAmount -= bt.Tier1UpgradeWoodCost;

            Destroy(house.modelReference);
            house.modelReference = Instantiate(
                bt.Tier1ModelPrefab,
                Globals.GridToGlobalCoordinates(house.gridLocation, bt.Tier1ModelPrefab),
                Quaternion.identity
            );
            house.modelReference.transform.parent = house.transform;
        }
        else if (house.upgradeTier == 1)
        {
            if (globals.goldAmount < bt.Tier2UpgradeGoldCost || globals.stoneAmount < bt.Tier2UpgradeStoneCost)
                return;

            for (int i = 0; i < (bt.Tier2ColonistCapacity - bt.Tier1ColonistCapacity); i++)
                house.colonists.Add(cm.CreateColonist(house));
            foreach(var colonist in house.colonists)
                colonist.type = ColonistData.Type.Nobleman;
            house.upgradeTier = 2;
            
            globals.goldAmount -= bt.Tier2UpgradeGoldCost;
            globals.stoneAmount -= bt.Tier2UpgradeStoneCost;

            Destroy(house.modelReference);
            house.modelReference = Instantiate(
                bt.Tier2ModelPrefab,
                Globals.GridToGlobalCoordinates(house.gridLocation, bt.Tier2ModelPrefab),
                Quaternion.identity
            );
            house.modelReference.transform.parent = house.transform;
        }
        else
            throw new Exception("House already fully upgraded");
    }
}
