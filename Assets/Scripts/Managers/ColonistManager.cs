using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class ColonistManager : MonoBehaviour
{
    Globals globals;
    BuildingManager bm;

    List<ColonistData> allColonists;
    Queue<ColonistData> joblessColonists;

    [SerializeField] ColonistData colonistDataPrefab;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        bm = FindObjectOfType<BuildingManager>();

        allColonists = new List<ColonistData>();
        joblessColonists = new Queue<ColonistData>();
    }

    public ColonistData CreateColonist(BuildingData livesAt)
    {
        ColonistData colonistData = Instantiate(colonistDataPrefab);
        colonistData.livesAt = livesAt;
        colonistData.currentlyInside = livesAt;

        /* queue going for need consumption -> otherwise go to work */

        joblessColonists.Enqueue(colonistData);
        allColonists.Add(colonistData);

        return colonistData;
    }

    public int GetJoblessColonistCount()
    {
        return joblessColonists.Count;
    }

    public void SendColonistToBuild(BuildingData worksAt)
    {
        if (GetJoblessColonistCount() < 1)
            throw new System.Exception("Not enough workers to build");

        ColonistData colonistData = joblessColonists.Dequeue();
        colonistData.worksAt = worksAt;

        //colonistData.enqueueAction
        // TODO send colonist to build
        /*
        queue send worker to build -> start building -> send back to house
        */
    }

    public void SendColonistToWork(BuildingData worksAt)
    {
        if (GetJoblessColonistCount() < 1)
            throw new System.Exception("Not enough workers for a job");
        
        ColonistData colonistData = joblessColonists.Dequeue();
        colonistData.worksAt = worksAt;

        // TODO send colonist to work
        /* queue send worker to work -> occupy work building */
    }

    // colonist coroutines
    void InitiateColonistRoutine(ColonistData colonistData)
    {
        IEnumerator NeedsReductionRoutine(ColonistData colonistData)
        {
            while(true)
            {
                colonistData.SleepNeedMeter -= globals.needAmountDecrement;
                if (colonistData.SleepNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoHome, colonistData);
                    QueueConsumerAction(ColonistData.Action.Sleep, colonistData);
                
                colonistData.FoodNeedMeter -= globals.needAmountDecrement;
                if (colonistData.FoodNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoToMarket, colonistData);
                    QueueConsumerAction(ColonistData.Action.BuyFood, colonistData);
                
                colonistData.ClothesNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type != ColonistData.Type.Peasant && colonistData.ClothesNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoToMarket, colonistData);
                    QueueConsumerAction(ColonistData.Action.BuyClothes, colonistData);
                
                colonistData.SaltNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type == ColonistData.Type.Nobleman && colonistData.SaltNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoToMarket, colonistData);
                    QueueConsumerAction(ColonistData.Action.BuySalt, colonistData);

                colonistData.WaterNeedMeter -= globals.needAmountDecrement;
                if (colonistData.WaterNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoToWell, colonistData);
                    QueueConsumerAction(ColonistData.Action.GetWater, colonistData);

                colonistData.BeerNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type == ColonistData.Type.Nobleman && colonistData.BeerNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoToInn, colonistData);
                    QueueConsumerAction(ColonistData.Action.BuyBeer, colonistData);
                
                colonistData.ReligionNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type != ColonistData.Type.Peasant && colonistData.ReligionNeedMeter == 20)
                    QueueConsumerAction(ColonistData.Action.GoToChurch, colonistData);
                    QueueConsumerAction(ColonistData.Action.AttendChurch, colonistData);

                yield return new WaitForSeconds(globals.needConsumptionInterval);
            }
        }

        StartCoroutine(NeedsReductionRoutine(colonistData));
    }

    // worker coroutines

    // consumer coroutines
     void QueueConsumerAction(ColonistData.Action action, ColonistData colonistData)
    {
        
    }
}