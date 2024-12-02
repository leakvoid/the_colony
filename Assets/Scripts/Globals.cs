using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [Header("General colonist data")]
    [SerializeField] public float colonistMovementSpeed = 3f;
    [SerializeField] public float sleepDuration = 10f;

    // TODO starting strategic resources

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
    None,
    Stone,
    Iron,
    Salt,
    Wood,
    Cotton,
    Wheat,
    Hops,
    Meat,
    Fish,
    Tools,
    Cloth,
    Flour,
    Bread,
    Beer
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

// instances
class House
{
    // instance
    Vector2 gridLocation;
    List<GameObject> workers;
}

class Market
{
    // instance
    Vector2 gridLocation;
    List<GameObject> workers;
}
