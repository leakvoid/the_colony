using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Farming BT", fileName = "New Farming Building Template")]
public class FarmingBT : WorkableBT
{
    [SerializeField] public int amountProducedPerInterval;
    [SerializeField] public Globals.ResourceType producedResource;
}
