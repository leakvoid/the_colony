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
    // TODO add material cost and time blocks to build
    // is constructed (instance)
}
