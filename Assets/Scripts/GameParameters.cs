using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameParameters : MonoBehaviour
{
    [Header("General colonist data")]
    [SerializeField] float colonistMovementSpeed = 1f;

    [Header("Sawmill")]
    int gridLength = 2;
    int gridWidth = 4;
    int maxNumberOfWorkers = 3;
    int currentNumberOfWorkers = 0;
    float timeToProduceResource = 5f;
    
}
