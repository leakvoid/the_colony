using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonistData : MonoBehaviour
{
    // 0-100 
    int sleepNeedMeter = 100;
    int foodNeedMeter = 0;
    int waterNeedMeter = 0;
    int clothesNeedMeter = 0;
    int saltNeedMeter = 0;
    int beerNeedMeter = 0;
    int religionNeedMeter = 0;

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

    GameObject livesAt;
    GameObject worksAt;

    GameObject modelReference;
}
