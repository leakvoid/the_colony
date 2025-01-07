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
    TerrainMeshRenderer terrainMeshRenderer;
    MainCameraController mainCameraControls;
    TerrainDepositsGenerator terrainDepositsGenerator;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        abstractMapGenerator = FindObjectOfType<AbstractMapGenerator>();
        buildingLocationModule = FindObjectOfType<BuildingLocationModule>();
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
        constructionScheduler.Initialize();
        computerPlayerEngine.InitializeComputerPlayer();
        terrainMeshRenderer.Initialize();
        terrainDepositsGenerator.Initialize();
        mainCameraControls.Initialize();
    }
}
