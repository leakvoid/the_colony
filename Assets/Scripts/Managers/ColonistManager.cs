using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public class ColonistManager : MonoBehaviour
{
    Globals globals;
    BuildingManager bm;
    BuildingLocationModule blm;

    List<ColonistData> allColonists;
    Queue<ColonistData> joblessColonists;

    [SerializeField] ColonistData colonistDataPrefab;
    [SerializeField] GameObject colonistModelPrefab;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        bm = FindObjectOfType<BuildingManager>();
        blm = FindObjectOfType<BuildingLocationModule>();

        allColonists = new List<ColonistData>();
        joblessColonists = new Queue<ColonistData>();

        waitForNeedConsumption = new WaitForSeconds(globals.needConsumptionInterval);
    }

    public ColonistData CreateColonist(BuildingData livesAt)
    {
        ColonistData colonistData = Instantiate(colonistDataPrefab);
        colonistData.livesAt = livesAt;
        colonistData.currentlyInside = livesAt;

        InitiateConsumerRoutine(colonistData);

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

    // consumer coroutines
    WaitForSeconds waitForNeedConsumption;

    void InitiateConsumerRoutine(ColonistData colonistData)
    {
        IEnumerator NeedsDecrementRoutine(ColonistData colonistData)
        {
            while(true)
            {
                colonistData.SleepNeedMeter -= globals.needAmountDecrement;
                if (colonistData.SleepNeedMeter <= 20 && !colonistData.SleepAlreadyQueued)
                {
                    colonistData.SleepAlreadyQueued = true;
                    QueueConsumerAction(ColonistData.Action.GoHome, colonistData);
                    QueueConsumerAction(ColonistData.Action.Sleep, colonistData);
                }
                
                colonistData.FoodNeedMeter -= globals.needAmountDecrement;
                if (colonistData.FoodNeedMeter <= 20 && !colonistData.FoodAlreadyQueued)
                {
                    BuildingData market = blm.GetClosestService(colonistData.livesAt, BuildingTag.Market);
                    if (market != null && globals.FoodAmount > 0)
                    {
                        colonistData.FoodAlreadyQueued = true;
                        globals.FoodAmount--;
                        globals.FoodReservedAmount++;
                        QueueConsumerAction(ColonistData.Action.GoToMarket, colonistData);
                        QueueConsumerAction(ColonistData.Action.BuyFood, colonistData);
                    }

                }
                
                colonistData.ClothesNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type != ColonistData.Type.Peasant && colonistData.ClothesNeedMeter <= 20 && !colonistData.ClothesAlreadyQueued)
                {
                    BuildingData market = blm.GetClosestService(colonistData.livesAt, BuildingTag.Market);
                    if (market != null && globals.clothAmount > 0)
                    {
                        colonistData.ClothesAlreadyQueued = true;
                        globals.clothAmount--;
                        globals.ClothReservedAmount++;
                        QueueConsumerAction(ColonistData.Action.GoToMarket, colonistData);
                        QueueConsumerAction(ColonistData.Action.BuyClothes, colonistData);
                    }
                }
                
                colonistData.SaltNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type == ColonistData.Type.Nobleman && colonistData.SaltNeedMeter <= 20 && !colonistData.SaltAlreadyQueued)
                {
                    BuildingData market = blm.GetClosestService(colonistData.livesAt, BuildingTag.Market);
                    if (market != null && globals.saltAmount > 0)
                    {
                        colonistData.SaltAlreadyQueued = true;
                        globals.saltAmount--;
                        globals.SaltReservedAmount++;
                        QueueConsumerAction(ColonistData.Action.GoToMarket, colonistData);
                        QueueConsumerAction(ColonistData.Action.BuySalt, colonistData);
                    }
                }

                colonistData.WaterNeedMeter -= globals.needAmountDecrement;
                if (colonistData.WaterNeedMeter <= 20 && !colonistData.WaterAlreadyQueued)
                {
                    BuildingData well = blm.GetClosestService(colonistData.livesAt, BuildingTag.Well);
                    if (well != null)
                    {
                        colonistData.WaterAlreadyQueued = true;
                        QueueConsumerAction(ColonistData.Action.GoToWell, colonistData);
                        QueueConsumerAction(ColonistData.Action.GetWater, colonistData);
                    }
                }

                colonistData.BeerNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type == ColonistData.Type.Nobleman && colonistData.BeerNeedMeter <= 20 && !colonistData.BeerAlreadyQueued)
                {
                    BuildingData inn = blm.GetClosestService(colonistData.livesAt, BuildingTag.Inn);
                    if (inn != null && globals.beerAmount > 0)
                    {
                        colonistData.BeerAlreadyQueued = true;
                        globals.beerAmount--;
                        globals.BeerReservedAmount++;
                        QueueConsumerAction(ColonistData.Action.GoToInn, colonistData);
                        QueueConsumerAction(ColonistData.Action.BuyBeer, colonistData);
                    }
                }

                colonistData.ReligionNeedMeter -= globals.needAmountDecrement;
                if (colonistData.type != ColonistData.Type.Peasant && colonistData.ReligionNeedMeter <= 20 && !colonistData.ReligionAlreadyQueued)
                {
                    BuildingData church = blm.GetClosestService(colonistData.livesAt, BuildingTag.Church);
                    if (church != null)
                    {
                        colonistData.ReligionAlreadyQueued = true;
                        QueueConsumerAction(ColonistData.Action.GoToChurch, colonistData);
                        QueueConsumerAction(ColonistData.Action.Pray, colonistData);
                    }
                }

                if (!colonistData.isConsumerRoutineActive && colonistData.consumerActions.Count > 0)
                {
                    StartCoroutine(ProcessConsumerActionQueue(colonistData));
                }

                yield return waitForNeedConsumption;
            }
        }

        StartCoroutine(NeedsDecrementRoutine(colonistData));
    }

    void QueueConsumerAction(ColonistData.Action action, ColonistData colonistData)
    {
        colonistData.consumerActions.Enqueue(action);
    }

    IEnumerator ProcessConsumerActionQueue(ColonistData colonistData)
    {
        colonistData.isConsumerRoutineActive = true;

        while (colonistData.consumerActions.Count > 0)
        {
            var action = colonistData.consumerActions.Dequeue();
            switch (action)
            {
                case ColonistData.Action.GoHome:
                    yield return ConsumerMoveTo(colonistData, colonistData.livesAt);
                    break;
                case ColonistData.Action.GoToWork:
                    yield return ConsumerMoveTo(colonistData, colonistData.worksAt);
                    break;
                case ColonistData.Action.GoToMarket:
                    BuildingData market = blm.GetClosestService(colonistData.livesAt, BuildingTag.Market);
                    yield return ConsumerMoveTo(colonistData, market);
                    break;
                case ColonistData.Action.GoToWell:
                    BuildingData well = blm.GetClosestService(colonistData.livesAt, BuildingTag.Well);
                    yield return ConsumerMoveTo(colonistData, well);
                    break;
                case ColonistData.Action.GoToChurch:
                    BuildingData church = blm.GetClosestService(colonistData.livesAt, BuildingTag.Church);
                    yield return ConsumerMoveTo(colonistData, church);
                    break;
                case ColonistData.Action.GoToInn:
                    BuildingData inn = blm.GetClosestService(colonistData.livesAt, BuildingTag.Inn);
                    yield return ConsumerMoveTo(colonistData, inn);
                    break;
                case ColonistData.Action.Sleep:
                    yield return new WaitForSeconds(globals.sleepDuration);
                    colonistData.SleepNeedMeter += globals.needAmountReplenished;
                    break;
                case ColonistData.Action.BuyFood:
                    globals.FoodReservedAmount--;
                    globals.goldAmount += globals.foodPrice;
                    colonistData.FoodNeedMeter += globals.needAmountReplenished;
                    break;
                case ColonistData.Action.BuyClothes:
                    globals.ClothReservedAmount--;
                    globals.goldAmount += globals.clothPrice;
                    colonistData.ClothesNeedMeter += globals.needAmountReplenished;
                    break;
                case ColonistData.Action.BuySalt:
                    globals.SaltReservedAmount--;
                    globals.goldAmount += globals.saltPrice;
                    colonistData.SaltNeedMeter += globals.needAmountReplenished;
                    break;
                case ColonistData.Action.GetWater:
                    colonistData.WaterNeedMeter += globals.needAmountReplenished;
                    break;
                case ColonistData.Action.BuyBeer:
                    globals.BeerReservedAmount--;
                    globals.goldAmount += globals.beerPrice;
                    colonistData.BeerNeedMeter += globals.needAmountReplenished;
                    break;
                case ColonistData.Action.Pray:
                    yield return new WaitForSeconds(globals.prayerDuration);
                    globals.goldAmount += globals.churchDonation;
                    colonistData.ReligionNeedMeter += globals.needAmountReplenished;
                    break;
            }
        }

        // TODO go home/work

        colonistData.isConsumerRoutineActive = false;
    }

    IEnumerator ConsumerMoveTo(ColonistData colonistData, BuildingData destination)// TODO road traversal + spawn location, animations
    {
        colonistData.consumerModelReference = Instantiate(colonistModelPrefab,
            globals.GridToGlobalCoordinates(colonistData.currentlyInside.gridLocation),
            Quaternion.identity);
        
        var model = colonistData.consumerModelReference;
        var start = model.transform.position;
        var end = destination.transform.position;
        while (start != end)
        {
            model.transform.position = Vector3.MoveTowards(model.transform.position, end, globals.colonistMovementSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();// TODO "new" optimization
        }
    }

    // worker coroutines

}