using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Resource Gathering BT", fileName = "New Resource Gathering Building Template")]
public class ResourceGatheringBT : ProductionBT
{
    [field: SerializeField] public TerrainType GroundResource { get; private set; }
    [field: SerializeField] public int MinDistanceToResource { get; private set; }
    [field: SerializeField] public int CaptureGatheringArea { get; private set; }
}