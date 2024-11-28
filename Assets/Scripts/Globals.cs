using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [Header("General colonist data")]
    [SerializeField] public float colonistMovementSpeed = 3f;
    [SerializeField] public float sleepDuration = 10f;

    // Singleton
    static Globals instance = null;

    void Start() {
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

    // Common types
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

    public enum GroundResource
    {
        None,
        Forest,
        Water,
        IronDeposit,
        SaltDeposit,
        StoneDeposit
    }
}

enum ResourceType
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

enum BuildingType
{
    Housing,
    Service,
    ResourceGathering,
    Processing
}

class House
{
    string buildingName = "House";
    BuildingType buildingType = BuildingType.Housing;
    int gridLength = 2;
    int gridWidth = 2;

    int colonistCapacity = 5;

    // instance
    Vector2 gridLocation;
    List<GameObject> colonists;
}

class Market
{
    string buildingName = "Market";
    BuildingType buildingType = BuildingType.Service;
    int gridLength = 5;
    int gridWidth = 5;

    int coverRadius = 100;

    int maxNumberOfWorkers = 1;
    int salary = 5;
    float timeInterval = 10f;

    // instance
    Vector2 gridLocation;
    List<GameObject> workers;
}

class Well
{
    string buildingName = "Well";
    BuildingType buildingType = BuildingType.Service;
    int gridLength = 1;
    int gridWidth = 1;

    int coverRadius = 50;

    // instance
    Vector2 gridLocation;
}

class Church
{
    string buildingName = "Church";
    BuildingType buildingType = BuildingType.Service;
    int gridLength = 7;
    int gridWidth = 7;

    int coverRadius = 150;

    int maxNumberOfWorkers = 3;
    int salary = 7;
    float timeInterval = 10f;

    // instance
    Vector2 gridLocation;
    List<GameObject> workers;
}

class Inn
{
    string buildingName = "Inn";
    BuildingType buildingType = BuildingType.Service;
    int gridLength = 5;
    int gridWidth = 5;

    int coverRadius = 150;

    int maxNumberOfWorkers = 3;
    int salary = 6;
    float timeInterval = 10f;

    // instance
    Vector2 gridLocation;
    List<GameObject> workers;
}

class StoneMine
{
    string buildingName = "StoneMine";
    BuildingType buildingType = BuildingType.ResourceGathering;
    int gridLength = 3;
    int gridWidth = 3;

    int maxNumberOfWorkers = 3;
    int salary = 2;
    float timeInterval = 5f;
    
    int amountProducedPerInterval = 1;
    ResourceType producedResource = ResourceType.Stone;
    int minDistanceToResource = 3;
    int exclusiveGatheringArea = 10;

    // instance
    int currentNumberOfWorkers = 0;
    int currentlyWorking = 0;
    List<GameObject> workers;
}

class Sawmill
{
    string buildingName = "Sawmill";
    BuildingType buildingType = BuildingType.ResourceGathering;
    int gridLength = 2;
    int gridWidth = 4;

    int maxNumberOfWorkers = 3;
    int salary = 2;
    float timeInterval = 5f;
    
    int amountProducedPerInterval = 1;
    ResourceType producedResource = ResourceType.Wood;
    int minDistanceToResource = 5;
    int exclusiveGatheringArea = 30;

    // instance
    int currentNumberOfWorkers = 0;
    int currentlyWorking = 0;
    List<GameObject> workers;
}

class Forge
{
    string buildingName = "Forge";
    BuildingType buildingType = BuildingType.Processing;
    int gridLength = 3;
    int gridWidth = 3;

    int maxNumberOfWorkers = 3;
    int salary = 2;
    float timeInterval = 5f;
    
    int amountConsumedPerInterval = 2;
    int amountProducedPerInterval = 2;
    ResourceType consumedResource = ResourceType.Iron;
    ResourceType producedResource = ResourceType.Tools;

    // instance
    int currentNumberOfWorkers = 0;
    int currentlyWorking = 0;
    List<GameObject> workers;
}