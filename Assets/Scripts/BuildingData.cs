using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    public BuildingTag buildingTag;
    public (int x, int y) gridLocation;
    public List<ColonistData> colonists;
    public int upgradeTier = 0;

    GameObject modelReference;
}