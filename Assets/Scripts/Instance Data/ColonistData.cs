using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ColonistData : MonoBehaviour
{
    // peasant needs
    private int sleepNeedMeter = 100;
    public int SleepNeedMeter
    {
        get { return sleepNeedMeter; }
        set {
                if(value <= 0)
                    sleepNeedMeter = 0;
                else
                    sleepNeedMeter = value;
            }
    }
    private int foodNeedMeter = 0;
    public int FoodNeedMeter
    {
        get { return foodNeedMeter; }
        set {
                if(value <= 0)
                    foodNeedMeter = 0;
                else
                    foodNeedMeter = value;
            }
    }
    private int waterNeedMeter = 0;
    public int WaterNeedMeter
    {
        get { return waterNeedMeter; }
        set {
                if(value <= 0)
                    waterNeedMeter = 0;
                else
                    waterNeedMeter = value;
            }
    }
    // citizen needs
    private int clothesNeedMeter = 0;
    public int ClothesNeedMeter
    {
        get { return clothesNeedMeter; }
        set {
                if(value <= 0)
                    clothesNeedMeter = 0;
                else
                    clothesNeedMeter = value;
            }
    }
    private int religionNeedMeter = 0;
    public int ReligionNeedMeter
    {
        get { return religionNeedMeter; }
        set {
                if(value <= 0)
                    religionNeedMeter = 0;
                else
                    religionNeedMeter = value;
            }
    }
    // nobleman needs
    private int beerNeedMeter = 0;
    public int BeerNeedMeter
    {
        get { return beerNeedMeter; }
        set {
                if(value <= 0)
                    beerNeedMeter = 0;
                else
                    beerNeedMeter = value;
            }
    }
    private int saltNeedMeter = 0;
    public int SaltNeedMeter
    {
        get { return saltNeedMeter; }
        set {
                if(value <= 0)
                    saltNeedMeter = 0;
                else
                    saltNeedMeter = value;
            }
    }

    public int MoneyEarned = 0;
    public int MoneySpent = 0;

    public enum Type
    {
        Peasant,
        Citizen,
        Nobleman
    }
    public Type type = Type.Peasant;

    public enum Occupation
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
        Brewer,
        Trader,
        Bartender
    }
    public Occupation occupation = Occupation.Jobless;

    public enum Action
    {
        // movement
        GoHome,
        GoToWork,
        GoToMarket,
        GoToWell,
        GoToChurch,
        GoToInn,
        // consumer
        Sleep,
        BuyFood,
        BuyClothes,
        BuySalt,
        GetWater,
        BuyBeer,
        Pray,
        // worker
        Build,
        Work
    }
    public Queue<Action> workerActions;
    public Queue<Action> consumerActions;

    // queue flags
    public bool SleepAlreadyQueued;
    public bool FoodAlreadyQueued;
    public bool ClothesAlreadyQueued;
    public bool SaltAlreadyQueued;
    public bool WaterAlreadyQueued;
    public bool BeerAlreadyQueued;
    public bool ReligionAlreadyQueued;

    public BuildingData livesAt;
    public BuildingData worksAt;
    public BuildingData consumerCurrentlyInside;

    /*
        logic for same colonist doing work, walking and fulfilling needs isn't viable
        solution: split each colonist into two
        husband and wife (worker and consumer)
    */
    public GameObject workerModelReference;
    public GameObject consumerModelReference;

    public bool isConsumerRoutineActive;
    public bool isWorkerRoutineActive;
}
