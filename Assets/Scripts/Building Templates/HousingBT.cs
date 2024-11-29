using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Housing BT", fileName = "New Housing Building Template")]
public class HousingBT : BuildingTemplate
{
    [SerializeField] public int colonistCapacity;
}
