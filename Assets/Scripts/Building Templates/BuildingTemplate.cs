using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTemplate : ScriptableObject
{
    [SerializeField] public string buildingName;
    [SerializeField] public int gridLength;
    [SerializeField] public int gridWidth;
}
