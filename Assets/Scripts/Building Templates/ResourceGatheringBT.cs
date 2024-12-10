using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Resource Gathering BT", fileName = "New Resource Gathering Building Template")]
public class ResourceGatheringBT : ProductionBT
{
    [SerializeField] public TerrainType groundResource;
    [SerializeField] public int minDistanceToResource;
    [SerializeField] public int captureGatheringArea;
}