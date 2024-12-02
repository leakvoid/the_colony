using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTemplate : ScriptableObject
{
    [SerializeField] public string buildingName;
    [SerializeField] public BuildingType buildingType;
    [SerializeField] public int gridLength;
    [SerializeField] public int gridWidth;
    // TODO add material cost and time to build
}
