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

    /*
    What is the right approach for building placement?
    
    1. flagMap of every service building placement
    overlap of those maps for best house building placement

    2. flagMap of all houses unaffected by a particular service

     */
}
