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
    [SerializeField] public int WoodCost;
    [SerializeField] public int StoneCost;
    [SerializeField] public int ToolsCost;
    [SerializeField] public int GoldCost;
    [SerializeField] public int constructionTime;
}
