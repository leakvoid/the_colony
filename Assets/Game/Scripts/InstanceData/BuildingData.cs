using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    public BuildingTemplate template;
    public (int x, int y) gridLocation;
    public List<ColonistData> colonists = new List<ColonistData>();
    public int upgradeTier = 0;
    public bool isConstructed = false;
    public float buildStartTime = 0;
    public GameObject modelReference;
    public (int x, int y) roadLocation;
}