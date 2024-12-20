using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTemplate : ScriptableObject
{
    [field: SerializeField] public BuildingTag BuildingTag { get; private set; }
    [field: SerializeField] public BuildingType BuildingType { get; private set; }
    [field: SerializeField] public int SizeX { get; private set; }
    [field: SerializeField] public int SizeY { get; private set; }
    [field: SerializeField] public int WoodCost { get; private set; }
    [field: SerializeField] public int StoneCost { get; private set; }
    [field: SerializeField] public int ToolsCost { get; private set; }
    [field: SerializeField] public int GoldCost { get; private set; }
    [field: SerializeField] public int ConstructionTime { get; private set; }
    [field: SerializeField] public GameObject FinishedModel { get; private set; }
    [field: SerializeField] public GameObject UnfinishedModel { get; private set; }
}
