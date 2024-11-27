using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerAI : MonoBehaviour
{
    GameParameters gameParameters;

    void Awake()
    {
        gameParameters = FindObjectOfType<GameParameters>();
        print(gameParameters.colonistMovementSpeed);
    }
}
