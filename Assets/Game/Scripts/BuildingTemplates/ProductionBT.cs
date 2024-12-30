using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBT : WorkableBT
{
    [field: SerializeField] public int AmountProducedPerInterval { get; private set; }
    [field: SerializeField] public ResourceType ProducedResource { get; private set; }
}
