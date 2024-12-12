using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonistManager : MonoBehaviour
{
    List<ColonistData> allColonists;
    Queue<ColonistData> joblessColonists;

    void Awake()
    {
        allColonists = new List<ColonistData>();
        joblessColonists = new Queue<ColonistData>();
    }

    public ColonistData CreateColonist(BuildingData livesAt)
    {
        ColonistData colonistData = new ColonistData();// TODO make proper instantiation
        colonistData.livesAt = livesAt;

        joblessColonists.Enqueue(colonistData);
        allColonists.Add(colonistData);

        return colonistData;
    }

    public int GetJoblessColonistCount()
    {
        return joblessColonists.Count;
    }

    public void SendColonistToBuild(BuildingData worksAt)
    {
        if (GetJoblessColonistCount() < 1)
            throw new System.Exception("Not enough workers to build");

        ColonistData colonistData = joblessColonists.Dequeue();
        colonistData.worksAt = worksAt;

        //colonistData.enqueueAction
        // TODO send colonist to work
    }

    public void SendColonistToWork(BuildingData worksAt)
    {
        if (GetJoblessColonistCount() < 1)
            throw new System.Exception("Not enough workers for a job");
        
        ColonistData colonistData = joblessColonists.Dequeue();
        colonistData.worksAt = worksAt;

        // TODO send colonist to work
    }
}