using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    BuildingTag buildingTag;
    (int x, int y) gridLocation;
    List<ColonistData> colonists;
    int upgradeTier = 0;

    GameObject modelReference;
}