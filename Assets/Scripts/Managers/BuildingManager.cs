using System.Collections;
using System.Collections.Generic;
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
        if (bt.goldCost > globals.goldAmount ||
            bt.woodCost > globals.woodAmount ||
            bt.stoneCost > globals.stoneAmount ||
            bt.toolsCost > globals.toolsAmount)
            return null;

        if (cm.GetJoblessColonistCount() < 1)
            return null;

        (int, int) location = blm.PickNewBuildingLocation(bt.buildingTag);
        if (location == (-1, -1))
            return null;

        BuildingData buildingData = Instantiate(buildingDataPrefab);
        buildingData.template = bt;
        buildingData.gridLocation = location;
        buildingData.modelReference = Instantiate(
            buildingData.template.unfinishedPrefab,
            globals.GridToGlobalCoordinates(location),
            Quaternion.identity
        );

        cm.SendColonistToBuild(buildingData);
        
        allBuildings.Add(buildingData);

        return buildingData;
    }

    void FinishBuildingConstruction(BuildingData buildingData)
    {
        buildingData.isConstructed = true;
        Destroy(buildingData.modelReference);
        buildingData.modelReference = Instantiate(
            buildingData.template.finishedPrefab,
            globals.GridToGlobalCoordinates(buildingData.gridLocation),
            Quaternion.identity
        );

        if (buildingData.template.buildingType == BuildingType.Housing)
        {
            HousingBT bt = (HousingBT)buildingData.template;
            for (int i = 0; i < bt.tier0ColonistCapacity; i++)
                buildingData.colonists.Add(cm.CreateColonist(buildingData));
            
            AssignJobsToColonists();
        }
        else
        {
            emptyWorkableBuildings.Enqueue(buildingData);
        }
    }

    void AssignJobsToColonists()
    {
        while(emptyWorkableBuildings.Count > 0)
        {
            BuildingData buildingData = emptyWorkableBuildings.Peek();
            WorkableBT bt = (WorkableBT)buildingData.template;
            if (bt.maxNumberOfWorkers > cm.GetJoblessColonistCount())
                return;
            for (int i = 0; i < bt.maxNumberOfWorkers; i++)
                cm.SendColonistToWork(buildingData);// TODO has to get to work first buildingData.colonists.Add()
            emptyWorkableBuildings.Dequeue();
        }
    }


}
