using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerAI : MonoBehaviour
{
    Globals globals;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
    }
}
