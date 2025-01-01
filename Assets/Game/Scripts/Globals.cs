using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [Header("Colony resources")]
    public int goldAmount = 3000;
    public int woodAmount = 20;
    public int stoneAmount = 20;
    public int toolsAmount = 0;

    public int ironAmount = 0;
    public int cottonAmount = 0;
    public int wheatAmount = 0;
    public int hopsAmount = 0;
    public int flourAmount = 0;
    public int saltAmount = 0;
    public int clothAmount = 0;
    public int meatAmount = 0;
    public int fishAmount = 0;
    public int breadAmount = 0;
    public int beerAmount = 0;

    public int FoodAmount
    {
        get { return meatAmount + fishAmount + breadAmount; }
        set {
                // WARNING, only works for decrements
                if (meatAmount > 0)
                    meatAmount--;
                else if (fishAmount > 0)
                    fishAmount--;
                else
                    breadAmount--;
            }
    }
    public int FoodReservedAmount { get; set; } = 0;
    public int ClothReservedAmount { get; set; } = 0;
    public int SaltReservedAmount { get; set; } = 0;
    public int BeerReservedAmount { get; set; } = 0;

    [field: Header("General colonist data")]
    [field: SerializeField] public float ColonistMovementSpeed { get; private set; } = 6f;
    [field: SerializeField] public float SleepDuration { get; private set; } = 2f;
    [field: SerializeField] public float PrayerDuration { get; private set; } = 2f;
    [field: SerializeField] public int NeedReplenishThreshold { get; private set; } = 20;

    [field: Header("Resource prices")]
    [field: SerializeField] public int FoodPrice { get; private set; } = 40;
    [field: SerializeField] public int SaltPrice { get; private set; } = 30;
    [field: SerializeField] public int ClothPrice { get; private set; } = 50;
    [field: SerializeField] public int BeerPrice { get; private set; } = 60;
    [field: SerializeField] public int ChurchDonation { get; private set; } = 10;

    [field: Header("Temporal")]
    [field: SerializeField] public float GameSpeedMultiplier { get; private set; } = 1f;
    [field: SerializeField] public float NeedConsumptionInterval { get; private set; } = 5f;
    [field: SerializeField] public int NeedAmountDecrement { get; private set; } = 10;
    [field: SerializeField] public int NeedAmountReplenished { get; private set; } = 100;
    [field: SerializeField] public float EngineNeedCheckInterval { get; private set; } = 2f;
    [field: SerializeField] public float EngineConstructionInterval { get; private set; } = 5f;

    [field: Header("Building templates")]
    // housing
    [field: SerializeField] public HousingBT HouseTemplate { get; private set; }
    // service
    [field: SerializeField] public ServiceBT MarketTemplate { get; private set; }
    [field: SerializeField] public ServiceBT ChurchTemplate { get; private set; }
    [field: SerializeField] public ServiceBT InnTemplate { get; private set; }
    [field: SerializeField] public ServiceBT WellTemplate { get; private set; }
    // farming
    [field: SerializeField] public FarmingBT CottonPlantationTemplate { get; private set; }
    [field: SerializeField] public FarmingBT HopsFarmTemplate { get; private set; }
    [field: SerializeField] public FarmingBT WheatFarmTemplate { get; private set; }
    // processing
    [field: SerializeField] public ProcessingBT BakeryTemplate { get; private set; }
    [field: SerializeField] public ProcessingBT BreweryTemplate { get; private set; }
    [field: SerializeField] public ProcessingBT ClothierTemplate { get; private set; }
    [field: SerializeField] public ProcessingBT ForgeTemplate { get; private set; }
    [field: SerializeField] public ProcessingBT WindmillTemplate { get; private set; }
    // gathering
    [field: SerializeField] public ResourceGatheringBT FishingHutTemplate { get; private set; }
    [field: SerializeField] public ResourceGatheringBT HuntersCabinTemplate { get; private set; }
    [field: SerializeField] public ResourceGatheringBT IronMineTemplate { get; private set; }
    [field: SerializeField] public ResourceGatheringBT SaltMineTemplate { get; private set; }
    [field: SerializeField] public ResourceGatheringBT SawmillTemplate { get; private set; }
    [field: SerializeField] public ResourceGatheringBT StoneMineTemplate { get; private set; }

    public BuildingTemplate NameToTemplate(BuildingTag buildingTag)
    {
        return buildingTag switch
        {
            BuildingTag.House => HouseTemplate,
            BuildingTag.Market => MarketTemplate,
            BuildingTag.Church => ChurchTemplate,
            BuildingTag.Inn => InnTemplate,
            BuildingTag.Well => WellTemplate,
            BuildingTag.CottonPlantation => CottonPlantationTemplate,
            BuildingTag.HopsFarm => HopsFarmTemplate,
            BuildingTag.WheatFarm => WheatFarmTemplate,
            BuildingTag.Bakery => BakeryTemplate,
            BuildingTag.Brewery => BreweryTemplate,
            BuildingTag.Clothier => ClothierTemplate,
            BuildingTag.Forge => ForgeTemplate,
            BuildingTag.Windmill => WindmillTemplate,
            BuildingTag.FishingHut => FishingHutTemplate,
            BuildingTag.HuntersCabin => HuntersCabinTemplate,
            BuildingTag.IronMine => IronMineTemplate,
            BuildingTag.SaltMine => SaltMineTemplate,
            BuildingTag.Sawmill => SawmillTemplate,
            BuildingTag.StoneMine => StoneMineTemplate,
            _ => throw new ArgumentException("building " + nameof(buildingTag) + " is missing"),
        };
    }

    public Vector3 GridToGlobalCoordinates((int x, int y) location)
    {
        return new Vector3(location.x, location.y, 0);
    }

    // Singleton
    static Globals instance = null;

    public void Initialize()
    {
        if(instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}

// Common types
public enum BuildingTag
{
    House,
    Market,
    Church,
    Inn,
    Well,
    CottonPlantation,
    HopsFarm,
    WheatFarm,
    Bakery,
    Brewery,
    Clothier,
    Forge,
    Windmill,
    FishingHut,
    HuntersCabin,
    IronMine,
    SaltMine,
    Sawmill,
    StoneMine
}

public enum BuildingType
{
    Housing,
    Service,
    Processing,
    Farming,
    ResourceGathering
}

public enum ResourceType
{
    // construction
    Wood,
    Stone,
    Tools,
    // raw
    Iron,
    Cotton,
    Wheat,
    Hops,
    Flour,
    // consumption
    Salt,
    Cloth,
    Meat,
    Fish,
    Bread,
    Beer,
    //
    Food
}

public enum TerrainType
{
    Ground,
    Forest,
    Water,
    IronDeposit,
    SaltDeposit,
    StoneDeposit
}
