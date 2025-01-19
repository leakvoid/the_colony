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
    ConstructionScheduler constructionScheduler;
    RoadPathModule rpm;

    List<ColonistData> allColonists;
    Queue<ColonistData> joblessColonists;
    int futureColonistCount = 0;

    [SerializeField] ColonistData colonistDataPrefab;
    [SerializeField] GameObject colonistModelPrefab;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        bm = FindObjectOfType<BuildingManager>();
        blm = FindObjectOfType<BuildingLocationModule>();
        constructionScheduler = FindObjectOfType<ConstructionScheduler>();// TODO only for computer player
        rpm = FindObjectOfType<RoadPathModule>();

        allColonists = new List<ColonistData>();
        joblessColonists = new Queue<ColonistData>();

        waitForNeedConsumption = new WaitForSeconds(globals.NeedConsumptionInterval);
    }

    public List<ColonistData> GetColonists()
    {
        return allColonists;
    }

    public ColonistData CreateColonist(BuildingData livesAt)
    {
        ColonistData colonistData = Instantiate(colonistDataPrefab);
        colonistData.livesAt = livesAt;
        colonistData.consumerCurrentlyInside = livesAt;

        InitiateConsumerRoutine(colonistData);

        joblessColonists.Enqueue(colonistData);
        allColonists.Add(colonistData);

        return colonistData;
    }

    public int GetJoblessColonistCount()
    {
        return joblessColonists.Count;
    }

    public int GetFutureColonistCount()
    {
        return futureColonistCount;
    }

    public void SendColonistToBuild(BuildingData worksAt)
    {
        if (GetJoblessColonistCount() < 1)
            throw new System.Exception("Not enough workers to build");

        ColonistData colonistData = joblessColonists.Dequeue();
        colonistData.worksAt = worksAt;
        colonistData.occupation = ColonistData.Occupation.Builder;

        if (constructionScheduler)
            constructionScheduler.ReduceBuildingPressure(worksAt.template, CalculateWalkingTime(colonistData));

        QueueWorkerAction(ColonistData.Action.GoToWork, colonistData);
        QueueWorkerAction(ColonistData.Action.Build, colonistData);
        QueueWorkerAction(ColonistData.Action.GoHome, colonistData);
        if (!colonistData.isWorkerRoutineActive)
            StartCoroutine(ProcessWorkerActionQueue(colonistData));
    }

    float CalculateWalkingTime(ColonistData colonistData)// TODO for roads
    {
        float distance = Vector3.Distance(colonistData.livesAt.modelReference.transform.position,
            colonistData.worksAt.modelReference.transform.position);
        return distance / globals.ColonistMovementSpeed;
    }

    public void SendColonistToWork(BuildingData worksAt)
    {
        if (GetJoblessColonistCount() < 1)
            throw new System.Exception("Not enough workers for a job");
        
        ColonistData colonistData = joblessColonists.Dequeue();
        colonistData.worksAt = worksAt;
        colonistData.occupation = worksAt.template.BuildingTag switch
        {
            BuildingTag.Market => ColonistData.Occupation.Trader,
            BuildingTag.Church => ColonistData.Occupation.Priest,
            BuildingTag.Inn => ColonistData.Occupation.Bartender,
            BuildingTag.CottonPlantation => ColonistData.Occupation.PlantationWorker,
            BuildingTag.HopsFarm => ColonistData.Occupation.HopsFarmer,
            BuildingTag.WheatFarm => ColonistData.Occupation.WheatFarmer,
            BuildingTag.Bakery => ColonistData.Occupation.Baker,
            BuildingTag.Brewery => ColonistData.Occupation.Brewer,
            BuildingTag.Clothier => ColonistData.Occupation.Tailor,
            BuildingTag.Forge => ColonistData.Occupation.Blacksmith,
            BuildingTag.Windmill => ColonistData.Occupation.Miller,
            BuildingTag.FishingHut => ColonistData.Occupation.Fisherman,
            BuildingTag.HuntersCabin => ColonistData.Occupation.Hunter,
            BuildingTag.IronMine => ColonistData.Occupation.IronMiner,
            BuildingTag.SaltMine => ColonistData.Occupation.SaltMiner,
            BuildingTag.Sawmill => ColonistData.Occupation.Lumberjack,
            BuildingTag.StoneMine => ColonistData.Occupation.StoneMiner,
            _ => throw new Exception("Unknown occupation for colonist"),
        };

        QueueWorkerAction(ColonistData.Action.GoToWork, colonistData);
        QueueWorkerAction(ColonistData.Action.Work, colonistData);
        if (!colonistData.isWorkerRoutineActive)
            StartCoroutine(ProcessWorkerActionQueue(colonistData));
    }

    // consumer coroutines
    WaitForSeconds waitForNeedConsumption;

    void InitiateConsumerRoutine(ColonistData colonistData)
    {
        IEnumerator NeedsDecrementRoutine(ColonistData colonistData)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 5f));

            while(true)
            {
                colonistData.SleepNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.SleepNeedMeter <= globals.NeedReplenishThreshold && !colonistData.SleepAlreadyQueued)
                {
                    colonistData.SleepAlreadyQueued = true;
                    QueueConsumerAction(ColonistData.Action.GoHome, colonistData);
                    QueueConsumerAction(ColonistData.Action.Sleep, colonistData);
                }
                
                colonistData.FoodNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.FoodNeedMeter <= globals.NeedReplenishThreshold && !colonistData.FoodAlreadyQueued)
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
                
                colonistData.ClothesNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.type != ColonistData.Type.Peasant &&
                    colonistData.ClothesNeedMeter <= globals.NeedReplenishThreshold &&
                    !colonistData.ClothesAlreadyQueued)
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
                
                colonistData.SaltNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.type == ColonistData.Type.Nobleman &&
                    colonistData.SaltNeedMeter <= globals.NeedReplenishThreshold &&
                    !colonistData.SaltAlreadyQueued)
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

                colonistData.WaterNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.WaterNeedMeter <= globals.NeedReplenishThreshold && !colonistData.WaterAlreadyQueued)
                {
                    BuildingData well = blm.GetClosestService(colonistData.livesAt, BuildingTag.Well);
                    if (well != null)
                    {
                        colonistData.WaterAlreadyQueued = true;
                        QueueConsumerAction(ColonistData.Action.GoToWell, colonistData);
                        QueueConsumerAction(ColonistData.Action.GetWater, colonistData);
                    }
                }

                colonistData.BeerNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.type == ColonistData.Type.Nobleman &&
                    colonistData.BeerNeedMeter <= globals.NeedReplenishThreshold &&
                    !colonistData.BeerAlreadyQueued)
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

                colonistData.ReligionNeedMeter -= globals.NeedAmountDecrement;
                if (colonistData.type != ColonistData.Type.Peasant &&
                    colonistData.ReligionNeedMeter <= globals.NeedReplenishThreshold &&
                    !colonistData.ReligionAlreadyQueued)
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
                    StartCoroutine(ProcessConsumerActionQueue(colonistData));

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
                    yield return new WaitForSeconds(globals.SleepDuration);
                    colonistData.SleepNeedMeter += globals.NeedAmountReplenished;
                    colonistData.SleepAlreadyQueued = false;
                    break;
                case ColonistData.Action.BuyFood:
                    globals.FoodReservedAmount--;
                    globals.goldAmount += globals.FoodPrice;
                    colonistData.MoneySpent += globals.FoodPrice;
                    colonistData.FoodNeedMeter += globals.NeedAmountReplenished;
                    colonistData.FoodAlreadyQueued = false;
                    break;
                case ColonistData.Action.BuyClothes:
                    globals.ClothReservedAmount--;
                    globals.goldAmount += globals.ClothPrice;
                    colonistData.MoneySpent += globals.ClothPrice;
                    colonistData.ClothesNeedMeter += globals.NeedAmountReplenished;
                    colonistData.ClothesAlreadyQueued = false;
                    break;
                case ColonistData.Action.BuySalt:
                    globals.SaltReservedAmount--;
                    globals.goldAmount += globals.SaltPrice;
                    colonistData.MoneySpent += globals.SaltPrice;
                    colonistData.SaltNeedMeter += globals.NeedAmountReplenished;
                    colonistData.SaltAlreadyQueued = false;
                    break;
                case ColonistData.Action.GetWater:
                    colonistData.WaterNeedMeter += globals.NeedAmountReplenished;
                    colonistData.WaterAlreadyQueued = false;
                    break;
                case ColonistData.Action.BuyBeer:
                    globals.BeerReservedAmount--;
                    globals.goldAmount += globals.BeerPrice;
                    colonistData.MoneySpent += globals.BeerPrice;
                    colonistData.BeerNeedMeter += globals.NeedAmountReplenished;
                    colonistData.BeerAlreadyQueued = false;
                    break;
                case ColonistData.Action.Pray:
                    yield return new WaitForSeconds(globals.PrayerDuration);
                    globals.goldAmount += globals.ChurchDonation;
                    colonistData.MoneySpent += globals.ChurchDonation;
                    colonistData.ReligionNeedMeter += globals.NeedAmountReplenished;
                    colonistData.ReligionAlreadyQueued = false;
                    break;
                default:
                    throw new Exception("consumer action queue exception");
            }
        }

        if (colonistData.worksAt != null)
            yield return ConsumerMoveTo(colonistData, colonistData.worksAt);
        else
            yield return ConsumerMoveTo(colonistData, colonistData.livesAt);

        colonistData.isConsumerRoutineActive = false;
    }

    IEnumerator ConsumerMoveTo(ColonistData colonistData, BuildingData destination)
    {
        yield return MoveTo(colonistData, colonistData.consumerCurrentlyInside, destination, false);
    }

    IEnumerator MoveTo(ColonistData colonistData, BuildingData source, BuildingData destination, bool isWorker)// TODO animations
    {
        var model = Instantiate(colonistModelPrefab,
            Globals.GridToGlobalCoordinates(source.roadLocation, colonistModelPrefab),
            Quaternion.identity);
        model.transform.parent = colonistData.transform;
        
        if (isWorker)
            colonistData.workerModelReference = model;
        else
            colonistData.consumerModelReference = model;

        yield return rpm.MoveColonist(source.roadLocation, destination.roadLocation, model);

        Destroy(model);
        colonistData.consumerCurrentlyInside = destination;
    }

    // worker coroutines
    void QueueWorkerAction(ColonistData.Action action, ColonistData colonistData)
    {
        colonistData.workerActions.Enqueue(action);
    }

    IEnumerator ProcessWorkerActionQueue(ColonistData colonistData)
    {
        colonistData.isWorkerRoutineActive = true;

        while (colonistData.workerActions.Count > 0)
        {
            var action = colonistData.workerActions.Dequeue();
            switch (action)
            {
                case ColonistData.Action.GoHome:
                    yield return MoveTo(colonistData, colonistData.worksAt, colonistData.livesAt, true);
                    colonistData.occupation = ColonistData.Occupation.Jobless;
                    joblessColonists.Enqueue(colonistData);
                    break;
                case ColonistData.Action.GoToWork:
                    if (colonistData.worksAt.template.BuildingTag == BuildingTag.House)
                        futureColonistCount += globals.HouseTemplate.Tier0ColonistCapacity;

                    yield return MoveTo(colonistData, colonistData.livesAt, colonistData.worksAt, true);
                    break;
                case ColonistData.Action.Build:
                    colonistData.worksAt.buildStartTime = Time.time;
                    yield return new WaitForSeconds(colonistData.worksAt.template.ConstructionTime);
                    bm.FinishBuildingConstruction(colonistData.worksAt);

                    if (colonistData.worksAt.template.BuildingTag == BuildingTag.House)
                        futureColonistCount -= globals.HouseTemplate.Tier0ColonistCapacity;
                    break;
                case ColonistData.Action.Work:
                    bm.OnWorkerArrival(colonistData);
                    break;
                default:
                    throw new Exception("worker action queue exception");
            }
        }

        colonistData.isWorkerRoutineActive = false;
    }
}