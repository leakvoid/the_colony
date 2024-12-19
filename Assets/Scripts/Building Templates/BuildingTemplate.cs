using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTemplate : ScriptableObject
{
    [field: SerializeField] public string buildingName { get; private set; }
    [field: SerializeField] public BuildingTag buildingTag { get; private set; }
    [field: SerializeField] public BuildingType buildingType { get; private set; }
    [field: SerializeField] public int sizeX { get; private set; }
    [field: SerializeField] public int sizeY { get; private set; }
    [field: SerializeField] public int woodCost { get; private set; }
    [field: SerializeField] public int stoneCost { get; private set; }
    [field: SerializeField] public int toolsCost { get; private set; }
    [field: SerializeField] public int goldCost { get; private set; }
    [field: SerializeField] public int constructionTime { get; private set; }
    [field: SerializeField] public GameObject finishedPrefab { get; private set; }
    [field: SerializeField] public GameObject unfinishedPrefab { get; private set; }
}
