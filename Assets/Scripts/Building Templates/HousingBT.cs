using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Housing BT", fileName = "New Housing Building Template")]
public class HousingBT : ScriptableObject
{
    [SerializeField] public string buildingName = "House";
    [SerializeField] public int gridLength = 2;
    [SerializeField] public int gridWidth = 2;
    [SerializeField] public int colonistCapacity = 5;
}
