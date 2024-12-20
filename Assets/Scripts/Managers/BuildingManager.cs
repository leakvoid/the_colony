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

    List<BuildingData> allBuildings;
    Queue<BuildingData> emptyWorkableBuildings;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        blm = FindObjectOfType<BuildingLocationModule>();
        cm = FindObjectOfType<ColonistManager>();

        allBuildings = new List<BuildingData>();
        emptyWorkableBuildings = new Queue<BuildingData>();
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

        (int, int) location = blm.PickNewBuildingLocation(bt);
        if (location == (-1, -1))
            return null;

        BuildingData buildingData = Instantiate(buildingDataPrefab);
        buildingData.template = bt;
        buildingData.gridLocation = location;
        buildingData.modelReference = Instantiate(
            buildingData.template.UnfinishedModel,
            globals.GridToGlobalCoordinates(location),
            Quaternion.identity
        );
        blm.UpdateAfterBuildingCreation(buildingData, bt);

        cm.SendColonistToBuild(buildingData);
        
        allBuildings.Add(buildingData);

        return buildingData;
    }

    public void FinishBuildingConstruction(BuildingData buildingData)
    {
        buildingData.isConstructed = true;
        Destroy(buildingData.modelReference);
        buildingData.modelReference = Instantiate(
            buildingData.template.FinishedModel,
            globals.GridToGlobalCoordinates(buildingData.gridLocation),
            Quaternion.identity
        );

        if (buildingData.template.BuildingType == BuildingType.Housing)
        {
            HousingBT bt = (HousingBT)buildingData.template;
            for (int i = 0; i < bt.Tier0ColonistCapacity; i++)
                buildingData.colonists.Add(cm.CreateColonist(buildingData));
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
                return;
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
}
