using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [Header("General colonist data")]
    [SerializeField] public float colonistMovementSpeed = 3f;
    [SerializeField] public float sleepDuration = 10f;

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

    public enum TileType
    {
        Ground,
        Forest,
        Water,
        IronDeposit,
        SaltDeposit,
        StoneDeposit
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
