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

    // data for placing houses
    bool[,] marketCoverArea;
    bool[,] churchCoverArea;
    bool[,] innCoverArea;
    bool[,] wellCoverArea;

    // uncovered houses (for placing service buildings)
    bool[,] uncoveredByMarket;
    bool[,] uncoveredByChurch;
    bool[,] uncoveredByInn;
    bool[,] uncoveredByWell;

    // data for placing gathering buildings
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

        // TODO add surrounding resource space as unavailable
        availableSpace = new bool[gridX, gridY];
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (terrainGrid[i,j] == TerrainType.Ground)
                    availableSpace[i,j] = true;
                else
                    availableSpace[i,j] = false;
            }
        }
    }

    public bool Build(BuildingTag buildingTag)
    {
        BuildingTemplate buildingTemplate = NameToTemplate(buildingTag);

        // pick location
        (int x, int y) location;
        switch (buildingTemplate.buildingType)
        {
            case BuildingType.Housing:
                location = PickBestHousingLocation(buildingTemplate);
                break;
            case BuildingType.Service:
                location = PickBestServiceLocation(buildingTemplate);
                break;
            case BuildingType.ResourceGathering:
                location = PickLocationNearResource(buildingTemplate);
                break;
            default:
                location = PickRandomBuildingLocation(buildingTemplate);
                break;
        }

        if (location == (-1, -1))
        {
            print("Building space run out for " + nameof(buildingTag));
            return false;
        }

        // place building
        // TODO instantiate building and place prefab onto scene

        // update building affected areas
        UpdateAvailableSpace(location, buildingTemplate);
        switch (buildingTemplate.buildingType)
        {
            case BuildingType.Housing:
                UpdateHouseCoverage(location, buildingTemplate);
                break;
            case BuildingType.Service:
                UpdateServiceCoverArea(location, buildingTemplate);
                break;
            case BuildingType.ResourceGathering:
                UpdateResourceCaptureArea(location, buildingTemplate);
                break;
            default:
                break;
        }
        
        return true;
    }

    bool[,] GetAvailableBuildingSpots(BuildingTemplate bt)
    {
        // TODO O(x^4) can be optimized
        var availableForBuilding = new bool[gridX - bt.sizeX + 1, gridY - bt.sizeY + 1];
        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                availableForBuilding[i,j] = true;
                for (int k = 0; k < bt.sizeX; k++)
                {
                    for (int l = 0; l < bt.sizeY; l++)
                    {
                        if (!availableSpace[i + k,j + l])
                        {
                            availableForBuilding[i,j] = false;
                            break;
                        }
                    }
                }
            }
        }
        return availableForBuilding;
    }

    (int, int) PickBestHousingLocation(BuildingTemplate bt)
    {
        var availableForBuilding = GetAvailableBuildingSpots(bt);

        const int MAXIMUM_VALUE = 5;
        var numberOfSpots = new int[MAXIMUM_VALUE];
        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (!availableForBuilding[i,j])
                    continue;
                
                int tileValue = 0;
                if (marketCoverArea[i,j])
                    tileValue += 1;
                if (churchCoverArea[i,j])
                    tileValue += 1;
                if (innCoverArea[i,j])
                    tileValue += 1;
                if (wellCoverArea[i,j])
                    tileValue += 1;
                
                numberOfSpots[tileValue] += 1;
            }
        }

        int bestValue = MAXIMUM_VALUE - 1; // TODO REWRITE with max
        while (bestValue >= 0 && numberOfSpots[bestValue] == 0)
            bestValue--;
        
        // out of space
        if (numberOfSpots[0] == 0)
            return (-1, -1);
        
        int chosenPos = UnityEngine.Random.Range(0, numberOfSpots[bestValue]);
        int currentPos = 0;

        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (!availableForBuilding[i,j])
                    continue;
                
                int tileValue = 0;
                if (marketCoverArea[i,j])
                    tileValue += 1;
                if (churchCoverArea[i,j])
                    tileValue += 1;
                if (innCoverArea[i,j])
                    tileValue += 1;
                if (wellCoverArea[i,j])
                    tileValue += 1;
                
                if(tileValue == bestValue)
                {
                    currentPos++;
                    if (currentPos == chosenPos)
                        return (i, j);
                }
            }
        }

        throw new Exception("Should never happen. Picked random position not found");
    }

    (int, int) PickBestServiceLocation(BuildingTemplate buildingTemplate)
    {
        ServiceBT bt = (ServiceBT)buildingTemplate;

        var availableForBuilding = GetAvailableBuildingSpots(bt);

        bool[,] uncoveredHouses;
        switch (bt.buildingTag)
        {
            case BuildingTag.Market:
                uncoveredHouses = uncoveredByMarket;
                break;
            case BuildingTag.Church:
                uncoveredHouses = uncoveredByChurch;
                break;
            case BuildingTag.Inn:
                uncoveredHouses = uncoveredByInn;
                break;
            case BuildingTag.Well:
                uncoveredHouses = uncoveredByWell;
                break;
            default:
                throw new Exception("Unknown service building.");
        }

        int bestCoverValue = 0;
        int numberOfBestSpots = 0;
        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (!availableForBuilding[i,j])
                    continue;
                
                int coverValue = GetServiceCoverAreaValue(i, j, bt);
                if (coverValue > bestCoverValue)
                {
                    bestCoverValue = coverValue;
                    numberOfBestSpots = 1;
                }
                else if (coverValue == bestCoverValue)
                {
                    numberOfBestSpots++;
                }
            }
        }

        if (numberOfBestSpots == 0)
            return (-1, -1);

        return (0,0);
    }

    int GetServiceCoverAreaValue(int x, int y, ServiceBT bt)
    {
        return 0;
    }

    (int, int) PickLocationNearResource(BuildingTemplate buildingTemplate)
    {
        ResourceGatheringBT bt = (ResourceGatheringBT)buildingTemplate;

        return (0,0);
    }

    (int, int) PickRandomBuildingLocation(BuildingTemplate buildingTemplate)
    {
        return (0,0);
    }

    void UpdateAvailableSpace((int x, int y) location, BuildingTemplate buildingTemplate)
    {
        for (int i = location.x; i < location.x + buildingTemplate.sizeX; i++)
        {
            for (int j = location.y; j < location.y + buildingTemplate.sizeY; j++)
            {
                availableSpace[i,j] = true;
            }
        }
    }

    void UpdateHouseCoverage((int x, int y) location,BuildingTemplate buildingTemplate)
    {

    }

    void UpdateServiceCoverArea((int x, int y) location,BuildingTemplate buildingTemplate)
    {
        
    }

    void UpdateResourceCaptureArea((int x, int y) location,BuildingTemplate buildingTemplate)
    {
        
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
