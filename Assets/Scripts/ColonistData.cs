using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ColonistData : MonoBehaviour
{
    // peasant needs
    int sleepNeedMeter = 100;
    int foodNeedMeter = 0;
    int waterNeedMeter = 0;
    // citizen needs
    int clothesNeedMeter = 0;
    int religionNeedMeter = 0;
    // nobleman needs
    int beerNeedMeter = 0;
    int saltNeedMeter = 0;

    // TODO money earned, money spent

    enum ColonistType
    {
        Peasant,
        Citizen,
        Nobleman
    }

    ColonistType colonistType = ColonistType.Peasant;

    enum Occupation
    {
        Jobless,
        Builder,
        Priest,
        StoneMiner,
        IronMiner,
        SaltMiner,
        Lumberjack,
        PlantationWorker,
        WheatFarmer,
        HopsFarmer,
        Hunter,
        Fisherman,
        Blacksmith,
        Tailor,
        Miller,
        Baker,
        Brewer
    }

    Occupation occupation = Occupation.Jobless;

    enum Status
    {
        Idle,
        Walking,
        Working,
        Consuming
    }

    Status status = Status.Idle;

    BuildingData livesAt;
    BuildingData worksAt;

    GameObject modelReference;

    void QueueActions()
    {
        if (sleepNeedMeter <= 0)
        {
            // enqueue goToLocation home
            // enqueue sleep
        }
        if (foodNeedMeter <= 0)
        {
            // check market availability
            // if constructed
            // check if available food
            // reserve purchase
            // enqueue goToLocation market
            // enqueue purchase food
        }
        if (clothesNeedMeter <= 0)
        {
            // same as food
        }
        if (saltNeedMeter <= 0)
        {
            // same as food
        }
        if (waterNeedMeter <= 0)
        {
            // check well availability
            // if constructed
            // enqueue goToLocation well
            // enqueue get water
        }
        if (beerNeedMeter <= 0)
        {
            // same as food
        }
        if (religionNeedMeter <= 0)
        {
            // same as well
        }
        
        // if has work
        // enqueue goToLocation work
        // enqueue startWorking
    }
}
