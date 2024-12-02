using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    // housing
    [SerializeField] HousingBT houseTemplate;

    // service
    [SerializeField] ServiceBT marketTemplate;
    [SerializeField] ServiceBT churchTemplate;
    [SerializeField] ServiceBT innTemplate;
    [SerializeField] ServiceBT wellTemplate;

    // farming
    [SerializeField] FarmingBT cottonPlantationTemplate;
    [SerializeField] FarmingBT hopsFarmTemplate;
    [SerializeField] FarmingBT wheatFarmTemplate;

    // processing
    [SerializeField] ProcessingBT bakeryTemplate;
    [SerializeField] ProcessingBT breweryTemplate;
    [SerializeField] ProcessingBT clothierTemplate;
    [SerializeField] ProcessingBT forgeTemplate;
    [SerializeField] ProcessingBT windmillTemplate;

    // gathering
    [SerializeField] ResourceGatheringBT fishingHutTemplate;
    [SerializeField] ResourceGatheringBT huntersCabinTemplate;
    [SerializeField] ResourceGatheringBT ironMineTemplate;
    [SerializeField] ResourceGatheringBT saltMineTemplate;
    [SerializeField] ResourceGatheringBT sawmillTemplate;
    [SerializeField] ResourceGatheringBT stoneMineTemplate;

    AbstractMapGenerator mapGenerator;

    // arrays for placing houses
    bool[,] marketCoverArea;
    bool[,] churchCoverArea;
    bool[,] innCoverArea;
    bool[,] wellCoverArea;

    // uncovered houses (for placing service buildings)
    bool[,] uncoveredByMarket;
    bool[,] uncoveredByChurch;
    bool[,] uncoveredByInn;
    bool[,] uncoveredByWell;

    // arrays for placing gathering buildings
    bool[,] fishingHutCaptureArea;
    bool[,] huntersCabinCaptureArea;
    bool[,] sawmillCaptureArea;
    bool[,] ironMineCaptureArea;
    bool[,] saltMineCaptureArea;
    bool[,] stoneMineCaptureArea;

    // already taken spaces
    bool[,] availableSpace;

    TerrainType[,] terrainGrid;
    int gridX;
    int gridY;

    // initialization
    void Awake()
    {
        mapGenerator = FindObjectOfType<AbstractMapGenerator>();
    }

    void Start()
    {
        terrainGrid = mapGenerator.GetTerrainGrid();
        gridX = terrainGrid.GetLength(0);
        gridY = terrainGrid.GetLength(1);

        marketCoverArea = new bool[gridX, gridY];
        churchCoverArea = new bool[gridX, gridY];
        innCoverArea = new bool[gridX, gridY];
        wellCoverArea = new bool[gridX, gridY];

        uncoveredByMarket = new bool[gridX, gridY];
        uncoveredByChurch = new bool[gridX, gridY];
        uncoveredByInn = new bool[gridX, gridY];
        uncoveredByWell = new bool[gridX, gridY];

        fishingHutCaptureArea = new bool[gridX, gridY];
        huntersCabinCaptureArea = new bool[gridX, gridY];
        sawmillCaptureArea = new bool[gridX, gridY];
        ironMineCaptureArea = new bool[gridX, gridY];
        saltMineCaptureArea = new bool[gridX, gridY];
        stoneMineCaptureArea = new bool[gridX, gridY];

        availableSpace = new bool[gridX, gridY];
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (terrainGrid[i,j] != TerrainType.Ground)
                {
                    availableSpace[i,j] = true;
                }
            }
        }
    }

    public bool Build(BuildingTag buildingTag)
    {
        BuildingTemplate buildingTemplate = NameToTemplate(buildingTag);

        (int x, int y) location;
        switch (buildingTemplate.buildingType)
        {
            case BuildingType.Housing:
                location = PickBestHousingLocation(buildingTag);
                break;
            case BuildingType.Service:
                location = PickBestServiceLocation(buildingTag);
                break;
            case BuildingType.ResourceGathering:
                location = PickLocationNearResource(buildingTag);
                break;
            default:
                location = PickRandomBuildingLocation(buildingTag);
                break;
        }

        if (location == (-1, -1))
        {
            print("Building space run out for " + nameof(buildingTag));
            return false;
        }

        PlaceBuildingAt(location, buildingTemplate);
        // TODO instantiate building

        // TODO update arrays
        return true;
    }

    (int, int) PickBestHousingLocation(BuildingTag buildingTag)
    {
        return (0,0);
    }

    (int, int) PickBestServiceLocation(BuildingTag buildingTag)
    {
        return (0,0);
    }

    (int, int) PickRandomBuildingLocation(BuildingTag buildingTag)
    {
        return (0,0);
    }

    (int, int) PickLocationNearResource(BuildingTag buildingTag)
    {
        return (0,0);
    }

    BuildingTemplate NameToTemplate(BuildingTag buildingTag)
    {
        switch (buildingTag)
        {
            case BuildingTag.House:
                return houseTemplate;
            case BuildingTag.Market:
                return marketTemplate;
            case BuildingTag.Church:
                return churchTemplate;
            case BuildingTag.Inn:
                return innTemplate;
            case BuildingTag.Well:
                return wellTemplate;
            case BuildingTag.CottonPlantation:
                return cottonPlantationTemplate;
            case BuildingTag.HopsFarm:
                return hopsFarmTemplate;
            case BuildingTag.WheatFarm:
                return wheatFarmTemplate;
            case BuildingTag.Bakery:
                return bakeryTemplate;
            case BuildingTag.Brewery:
                return breweryTemplate;
            case BuildingTag.Clothier:
                return clothierTemplate;
            case BuildingTag.Forge:
                return forgeTemplate;
            case BuildingTag.Windmill:
                return windmillTemplate;
            case BuildingTag.FishingHut:
                return fishingHutTemplate;
            case BuildingTag.HuntersCabin:
                return huntersCabinTemplate;
            case BuildingTag.IronMine:
                return ironMineTemplate;
            case BuildingTag.SaltMine:
                return saltMineTemplate;
            case BuildingTag.Sawmill:
                return sawmillTemplate;
            case BuildingTag.StoneMine:
                return stoneMineTemplate;
            default:
                throw new ArgumentException("building " + nameof(buildingTag) + " is missing");
        }
    }

    void PlaceBuildingAt((int x, int y) location, BuildingTemplate buildingTemplate)
    {
        for (int i = location.x; i < location.x + buildingTemplate.gridLength; i++)
        {
            for (int j = location.y; j < location.y + buildingTemplate.gridWidth; j++)
            {
                availableSpace[i,j] = true;
            }
        }
    }

    /*
    What is the right approach for building placement?
    
    1. flagMap of every service building placement
        overlap of those flagMaps for best house building placement

    2. flagMap of all houses unaffected by a particular service

    3. flagMap of all spaces with buildings / natural barriers (free or taken)
    ? OR list of all free spaces

    1 + 3 cascading rule for placing a house (from highest overlap to no service buildings)

    2 + 3 find empty space where service building area (square) covers highest number of unaffected houses

    farming and processing can be placed wherever (MAYBE introduce farmland / warehouse logic)

    gathering nearby resources 4. get flagMap for each already mined resource
     */
}
