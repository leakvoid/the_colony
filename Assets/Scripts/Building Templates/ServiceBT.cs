using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Template/Service BT", fileName = "New Service Building Template")]
public class ServiceBT : WorkableBT
{
    [SerializeField] public int coverArea;
}
