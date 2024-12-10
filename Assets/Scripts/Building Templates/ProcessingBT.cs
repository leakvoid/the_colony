using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Processing BT", fileName = "New Processing Building Template")]
public class ProcessingBT : ProductionBT
{
    [SerializeField] public int amountConsumedPerInterval;
    [SerializeField] public ResourceType consumedResource;
}
