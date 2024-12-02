using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Processing BT", fileName = "New Processing Building Template")]
public class ProcessingBT : WorkableBT
{
    [SerializeField] public int amountConsumedPerInterval;
    [SerializeField] public int amountProducedPerInterval;
    [SerializeField] public ResourceType consumedResource;
    [SerializeField] public ResourceType producedResource;
}
