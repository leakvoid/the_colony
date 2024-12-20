using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModuleLoader : MonoBehaviour
{
    Globals globals;
    AbstractMapGenerator abstractMapGenerator;
    BuildingLocationModule buildingLocationModule;
    ConstructionScheduler constructionScheduler;
    ComputerPlayerEngine computerPlayerEngine;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        abstractMapGenerator = FindObjectOfType<AbstractMapGenerator>();
        buildingLocationModule = FindObjectOfType<BuildingLocationModule>();
        constructionScheduler = FindObjectOfType<ConstructionScheduler>();
        computerPlayerEngine = FindObjectOfType<ComputerPlayerEngine>();
    }

    void Start()
    {
        globals.Initialize();
        abstractMapGenerator.GenerateNewMap();
        buildingLocationModule.Initialize();
        constructionScheduler.Initialize();
        computerPlayerEngine.InitializeComputerPlayer();
    }
}
