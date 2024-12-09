using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [Header("General colonist data")]
    [SerializeField] public float colonistMovementSpeed = 3f;
    [SerializeField] public float sleepDuration = 10f;

    [Header("Colony resources")]
    [SerializeField] int goldAmount = 2000;
    [SerializeField] int woodAmount = 20;
    [SerializeField] int stoneAmount = 20;
    [SerializeField] int toolsAmount = 0;

    [SerializeField] int IronAmount = 0;
    [SerializeField] int CottonAmount = 0;
    [SerializeField] int WheatAmount = 0;
    [SerializeField] int HopsAmount = 0;
    [SerializeField] int FlourAmount = 0;
    [SerializeField] int SaltAmount = 0;
    [SerializeField] int ClothAmount = 0;
    [SerializeField] int MeatAmount = 0;
    [SerializeField] int FishAmount = 0;
    [SerializeField] int BreadAmount = 0;
    [SerializeField] int BeerAmount = 0;

    [Header("Resource prices")]
    [SerializeField] int foodPrice = 20;
    [SerializeField] int saltPrice = 30;
    [SerializeField] int clothPrice = 40;
    [SerializeField] int beerPrice = 50;
    [SerializeField] int churchDonation = 10;

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
