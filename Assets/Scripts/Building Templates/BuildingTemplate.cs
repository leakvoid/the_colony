using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTemplate : ScriptableObject
{
    [SerializeField] public string buildingName;
    [SerializeField] public BuildingTag buildingTag;
    [SerializeField] public BuildingType buildingType;
    [SerializeField] public int sizeX;
    [SerializeField] public int sizeY;
    [SerializeField] public int woodCost;
    [SerializeField] public int stoneCost;
    [SerializeField] public int toolsCost;
    [SerializeField] public int goldCost;
    [SerializeField] public int constructionTime;
    [SerializeField] public GameObject finishedPrefab;
    [SerializeField] public GameObject unfinishedPrefab;
}
