using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [Header("General colonist data")]
    [SerializeField] public float colonistMovementSpeed = 3f;
    [SerializeField] public float sleepDuration = 5f;

    [Header("Colony resources")]
    [SerializeField] public int goldAmount = 2000;
    [SerializeField] public int woodAmount = 20;
    [SerializeField] public int stoneAmount = 20;
    [SerializeField] public int toolsAmount = 0;

    [SerializeField] public int ironAmount = 0;
    [SerializeField] public int cottonAmount = 0;
    [SerializeField] public int wheatAmount = 0;
    [SerializeField] public int hopsAmount = 0;
    [SerializeField] public int flourAmount = 0;
    [SerializeField] public int saltAmount = 0;
    [SerializeField] public int clothAmount = 0;
    [SerializeField] public int meatAmount = 0;
    [SerializeField] public int fishAmount = 0;
    [SerializeField] public int breadAmount = 0;
    [SerializeField] public int beerAmount = 0;

    [Header("Resource prices")]
    [SerializeField] public int foodPrice = 20;
    [SerializeField] public int saltPrice = 30;
    [SerializeField] public int clothPrice = 40;
    [SerializeField] public int beerPrice = 50;
    [SerializeField] public int churchDonation = 10;

    [Header("Temporal")]
    [SerializeField] public float gameSpeedMultiplier = 1f;
    [SerializeField] public float needsConsumptionInterval = 5f;
    [SerializeField] public float needsAmountDecreased = 10f;
    [SerializeField] public float engineNeedCheckInterval = 2f;
    [SerializeField] public float engineConstructionInterval = 5f;

    [Header("Building templates")]
    // housing
    [SerializeField] public HousingBT houseTemplate;
    // service
    [SerializeField] public ServiceBT marketTemplate;
    [SerializeField] public ServiceBT churchTemplate;
    [SerializeField] public ServiceBT innTemplate;
    [SerializeField] public ServiceBT wellTemplate;
    // farming
    [SerializeField] public FarmingBT cottonPlantationTemplate;
    [SerializeField] public FarmingBT hopsFarmTemplate;
    [SerializeField] public FarmingBT wheatFarmTemplate;
    // processing
    [SerializeField] public ProcessingBT bakeryTemplate;
    [SerializeField] public ProcessingBT breweryTemplate;
    [SerializeField] public ProcessingBT clothierTemplate;
    [SerializeField] public ProcessingBT forgeTemplate;
    [SerializeField] public ProcessingBT windmillTemplate;
    // gathering
    [SerializeField] public ResourceGatheringBT fishingHutTemplate;
    [SerializeField] public ResourceGatheringBT huntersCabinTemplate;
    [SerializeField] public ResourceGatheringBT ironMineTemplate;
    [SerializeField] public ResourceGatheringBT saltMineTemplate;
    [SerializeField] public ResourceGatheringBT sawmillTemplate;
    [SerializeField] public ResourceGatheringBT stoneMineTemplate;

    public BuildingTemplate NameToTemplate(BuildingTag buildingTag)
    {
        return buildingTag switch
        {
            BuildingTag.House => houseTemplate,
            BuildingTag.Market => marketTemplate,
            BuildingTag.Church => churchTemplate,
            BuildingTag.Inn => innTemplate,
            BuildingTag.Well => wellTemplate,
            BuildingTag.CottonPlantation => cottonPlantationTemplate,
            BuildingTag.HopsFarm => hopsFarmTemplate,
            BuildingTag.WheatFarm => wheatFarmTemplate,
            BuildingTag.Bakery => bakeryTemplate,
            BuildingTag.Brewery => breweryTemplate,
            BuildingTag.Clothier => clothierTemplate,
            BuildingTag.Forge => forgeTemplate,
            BuildingTag.Windmill => windmillTemplate,
            BuildingTag.FishingHut => fishingHutTemplate,
            BuildingTag.HuntersCabin => huntersCabinTemplate,
            BuildingTag.IronMine => ironMineTemplate,
            BuildingTag.SaltMine => saltMineTemplate,
            BuildingTag.Sawmill => sawmillTemplate,
            BuildingTag.StoneMine => stoneMineTemplate,
            _ => throw new ArgumentException("building " + nameof(buildingTag) + " is missing"),
        };
    }

    public Vector3 GridToGlobalCoordinates((int x, int y) location)
    {
        return new Vector3(0, 0, 0);// TODO implement coordinate conversion
    }

    // Singleton
    static Globals instance = null;

    void Start()
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
