using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkableBT : BuildingTemplate
{
    [SerializeField] public int maxNumberOfWorkers;
    [SerializeField] public int salary;
    [SerializeField] public float timeInterval = 10f;
}
