using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBT : WorkableBT
{
    [SerializeField] public int amountProducedPerInterval;
    [SerializeField] public ResourceType producedResource;
}
