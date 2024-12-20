using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkableBT : BuildingTemplate
{
    [field: SerializeField] public int MaxNumberOfWorkers { get; private set; }
    [field: SerializeField] public int Salary { get; private set; }
    [field: SerializeField] public float TimeInterval { get; private set; } = 10f;
}
