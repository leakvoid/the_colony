using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModuleLoader : MonoBehaviour
{
    Globals globals;
    AbstractMapGenerator abstractMapGenerator;
    BuildingLocationModule buildingLocationModule;
    RoadPathModule roadPathModule;
    ConstructionScheduler constructionScheduler;
    ComputerPlayerEngine computerPlayerEngine;
    TerrainMeshRenderer terrainMeshRenderer;
    TerrainDepositsGenerator terrainDepositsGenerator;
    MainCameraController mainCameraControls;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();

        abstractMapGenerator = FindObjectOfType<AbstractMapGenerator>();
        buildingLocationModule = FindObjectOfType<BuildingLocationModule>();
        roadPathModule = FindObjectOfType<RoadPathModule>();
        constructionScheduler = FindObjectOfType<ConstructionScheduler>();
        computerPlayerEngine = FindObjectOfType<ComputerPlayerEngine>();

        terrainMeshRenderer = FindObjectOfType<TerrainMeshRenderer>();
        terrainDepositsGenerator = FindObjectOfType<TerrainDepositsGenerator>();
        mainCameraControls = FindObjectOfType<MainCameraController>();
    }

    void Start()
    {
        globals.Initialize();

        abstractMapGenerator.GenerateNewMap();
        buildingLocationModule.Initialize();
        roadPathModule.Initialize();
        constructionScheduler.Initialize();
        computerPlayerEngine.InitializeComputerPlayer();

        terrainMeshRenderer.Initialize();
        terrainDepositsGenerator.Initialize();
        mainCameraControls.Initialize();
    }
}
