using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ComputerPlayerEngine : MonoBehaviour
{
    Globals globals;
    ConstructionScheduler cs;
    ColonistManager cm;
    BuildingLocationModule blm;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        cs = FindObjectOfType<ConstructionScheduler>();
        cm = FindObjectOfType<ColonistManager>();
        blm = FindObjectOfType<BuildingLocationModule>();
    }
    
    public void InitializeComputerPlayer()
    {
        StartCoroutine(ColonistPressureRoutine());
        StartCoroutine(BuildingConstructionRoutine());
    }

    IEnumerator ColonistPressureRoutine()
    {
        var colonists = cm.GetColonists();
        var waitForInterval = new WaitForSeconds(globals.EngineNeedCheckInterval);
        
        while (true)
        {
            yield return waitForInterval;

            foreach (var colonist in colonists)
            {
                BuildingData market = blm.GetClosestService(colonist.livesAt, BuildingTag.Market);
                if (market != null && colonist.FoodNeedMeter <= globals.NeedReplenishThreshold)
                    cs.IncreaseResourcePressure(ResourceType.Food);
                if (market != null && colonist.type != ColonistData.Type.Peasant && colonist.ClothesNeedMeter <= globals.NeedReplenishThreshold)
                    cs.IncreaseResourcePressure(ResourceType.Cloth);
                if (colonist.type == ColonistData.Type.Nobleman)
                {
                    if (market != null && colonist.SaltNeedMeter <= globals.NeedReplenishThreshold)
                        cs.IncreaseResourcePressure(ResourceType.Salt);
                    BuildingData inn = blm.GetClosestService(colonist.livesAt, BuildingTag.Inn);
                    if (inn != null && colonist.BeerNeedMeter <= globals.NeedReplenishThreshold)
                        cs.IncreaseResourcePressure(ResourceType.Beer);
                }
            }
        }
    }

    IEnumerator BuildingConstructionRoutine()
    {
        var waitForInterval = new WaitForSeconds(globals.EngineConstructionInterval);

        while (true)
        {
            int workerDeficit = 20 - cm.GetJoblessColonistCount() - cm.GetFutureColonistCount() -
                cs.GetBuildingPressure(BuildingTag.House) * globals.HouseTemplate.Tier0ColonistCapacity;
            
            if (workerDeficit > 0)
            {
                cs.IncreaseBuildingPressure(BuildingTag.House,
                    (workerDeficit + globals.HouseTemplate.Tier0ColonistCapacity - 1) / globals.HouseTemplate.Tier0ColonistCapacity);
            }

            cs.MakeBuildings();

            yield return waitForInterval;
        }
    }
}
