using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Resource Gathering BT", fileName = "New Resource Gathering Building Template")]
public class ResourceGatheringBT : WorkableBT
{
    [SerializeField] public int amountProducedPerInterval;
    [SerializeField] public Globals.ResourceType producedResource;
    [SerializeField] public Globals.GroundResource groundResource;
    [SerializeField] public int minDistanceToResource;
    [SerializeField] public int captureGatheringArea;
}