using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Processing BT", fileName = "New Processing Building Template")]
public class ProcessingBT : ProductionBT
{
    [field: SerializeField] public int AmountConsumedPerInterval { get; private set; }
    [field: SerializeField] public ResourceType ConsumedResource { get; private set; }
}
