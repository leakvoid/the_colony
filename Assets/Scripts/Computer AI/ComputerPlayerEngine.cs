using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ComputerPlayerEngine : MonoBehaviour
{
    Globals globals;
    ConstructionScheduler cs;
    ColonistManager cm;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        cs = FindObjectOfType<ConstructionScheduler>();
        cm = FindObjectOfType<ColonistManager>();
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
                if (colonist.FoodNeedMeter <= globals.NeedReplenishThreshold)
                    cs.IncreaseResourcePressure(ResourceType.Food);
                if (colonist.type != ColonistData.Type.Peasant && colonist.ClothesNeedMeter <= globals.NeedReplenishThreshold)
                    cs.IncreaseResourcePressure(ResourceType.Cloth);
                if (colonist.type == ColonistData.Type.Nobleman)
                {
                    if (colonist.SaltNeedMeter <= globals.NeedReplenishThreshold)
                        cs.IncreaseResourcePressure(ResourceType.Salt);
                    if (colonist.BeerNeedMeter <= globals.NeedReplenishThreshold)
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
            yield return waitForInterval;

            cs.MakeBuildings();
        }
    }

    /*
    1. write this module +
    2. check module load order +
    3. todos +
    4. refactor building templates +
    5. fill all game models with cubes
    */
}
