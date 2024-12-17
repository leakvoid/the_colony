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

    // TODO money earned, money spent

    public enum Type
    {
        Peasant,
        Citizen,
        Nobleman
    }

    public Type type = Type.Peasant;

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

    public enum Action
    {
        GoHome,
        GoToWork,
        GoToMarket,
        GoToWell,
        GoToChurch,
        GoToInn,
        Sleep,
        BuyFood,
        BuyClothes,
        BuySalt,
        GetWater,
        BuyBeer,
        AttendChurch
    }

    public BuildingData livesAt;
    public BuildingData worksAt;
    public BuildingData currentlyInside;

    GameObject workerModelReference;
    GameObject consumerModelReference;

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

    /*
        logic for same colonist doing work, walking and fulfilling needs isn't viable
        solution: split each colonist into two
        husband and wife (worker and consumer)
    */
}
