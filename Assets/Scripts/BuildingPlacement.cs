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

        var maxTileValue = 0;
        var maxTileValueCount = 0;
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
                
                if (tileValue > maxTileValue)
                {
                    maxTileValue = tileValue;
                    maxTileValueCount = 1;
                }
                else if (tileValue == maxTileValue)
                {
                    maxTileValueCount++;
                }
            }
        }
        
        if (maxTileValueCount == 0)
            return (-1, -1);
        
        int chosenPos = UnityEngine.Random.Range(0, maxTileValueCount);
        int currentPos = -1;

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
                
                if(tileValue == maxTileValue)
                {
                    currentPos++;
                    if (currentPos == chosenPos)
                        return (i, j);
                }
            }
        }

        throw new Exception("Should never happen. Picked random housing position not found");
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

        var coverValueGrid = new int[gridX - bt.sizeX + 1, gridY - bt.sizeY + 1];
        int bestCoverValue = 0;
        int numberOfBestSpots = 0;
        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (!availableForBuilding[i,j])
                    continue;
                
                int coverValue = GetServiceCoverAreaValue(i, j, uncoveredHouses, bt);
                if (coverValue > bestCoverValue)
                {
                    bestCoverValue = coverValue;
                    numberOfBestSpots = 1;
                }
                else if (coverValue == bestCoverValue)
                {
                    numberOfBestSpots++;
                }
                coverValueGrid[i,j] = coverValue;
            }
        }

        if (numberOfBestSpots == 0)
            return (-1, -1);

        int chosenPos = UnityEngine.Random.Range(0, numberOfBestSpots);

        int currentPos = -1;
        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (!availableForBuilding[i,j])
                    continue;

                if(coverValueGrid[i,j] == bestCoverValue)
                {
                    currentPos++;
                    if (currentPos == chosenPos)
                        return (i, j);
                }
            }
        }

        throw new Exception("Should never happen. Picked random service position not found");
    }

    int GetServiceCoverAreaValue(int x, int y, bool[,] uncoveredHouses, ServiceBT bt)
    {
        // TODO center around entire building
        // TODO O(x^4) can be optimized
        int count = 0;
        for (int i = Math.Min(x - bt.coverArea, 0); i < Math.Max(x + bt.coverArea, gridX); i++)
        {
            for (int j = Math.Min(y - bt.coverArea, 0); j < Math.Max(y + bt.coverArea, gridY); j++)
            {
                if (uncoveredHouses[i,j])
                    count++;
            }
        }
        return count;
    }

    // TODO logic is wrong, any building space, not just top left corner; similar problem in other cases
    (int, int) PickLocationNearResource(BuildingTemplate buildingTemplate)
    {
        ResourceGatheringBT bt = (ResourceGatheringBT)buildingTemplate;

        var availableForBuilding = GetAvailableBuildingSpots(bt);
        var resourceCoverGrid = CreateResourceCoverGrid(bt.groundResource, bt.minDistanceToResource);
        bool[,] captureArea;
        switch (bt.buildingTag)
        {
            case BuildingTag.FishingHut:
                captureArea = fishingHutCaptureArea;
                break;
            case BuildingTag.HuntersCabin:
                captureArea = huntersCabinCaptureArea;
                break;
            case BuildingTag.Sawmill:
                captureArea = sawmillCaptureArea;
                break;
            case BuildingTag.IronMine:
                captureArea = ironMineCaptureArea;
                break;
            case BuildingTag.SaltMine:
                captureArea = saltMineCaptureArea;
                break;
            case BuildingTag.StoneMine:
                captureArea = stoneMineCaptureArea;
                break;
            default:
                throw new Exception("Resource gathering building " + bt.buildingName + " is missing");
        }

        int availableLocations = 0;
        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (availableForBuilding[i,j] && !captureArea[i,j] && resourceCoverGrid[i,j])
                    availableLocations++;
            }
        }

        if (availableLocations == 0)
            return (-1, -1);

        int chosenPos = UnityEngine.Random.Range(0, availableLocations);
        int currentPos = -1;

        for (int i = 0; i < gridX - bt.sizeX + 1; i++)
        {
            for (int j = 0; j < gridY - bt.sizeY + 1; j++)
            {
                if (availableForBuilding[i,j] && !captureArea[i,j] && resourceCoverGrid[i,j])
                {
                    currentPos++;
                    if(currentPos == chosenPos)
                        return (i, j);
                }
            }
        }

        return (0,0);
    }

    bool[,] CreateResourceCoverGrid(TerrainType groundResource, int range)
    {
        void FillGrid(int x1, int x2, int y1, int y2, bool[,] resourceCoverGrid)
        {
            for (int i = Math.Min(x1, 0); i < Math.Max(x2 + 1, gridX); i++)
            {
                for (int j = Math.Min(y1, 0); j < Math.Max(y2 + 1, gridY); j++)
                {
                    resourceCoverGrid[i,j] = true;
                }
            }
        }

        var resourceCoverGrid = new bool[gridX, gridY];

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (terrainGrid[i,j] == groundResource)
                {
                    if (i + 1 < gridX && terrainGrid[i + 1, j] != groundResource)
                        FillGrid(i + 1, i + range, j - range, j + range, resourceCoverGrid);
                    if (i - 1 >= 0 && terrainGrid[i - 1, j] != groundResource)
                        FillGrid(i - range, i - 1, j - range, j + range, resourceCoverGrid);
                    if (j + 1 < gridY && terrainGrid[i, j + 1] != groundResource)
                        FillGrid(i - range, i + range, j + 1, j + range, resourceCoverGrid);
                    if (j - 1 >= 0 && terrainGrid[i, j - 1] != groundResource)
                        FillGrid(i - range, i + range, j - range, j - 1, resourceCoverGrid);
                }
            }
        }

        return resourceCoverGrid;
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
        return buildingTag switch
        {
            BuildingTag.House => houseTemplate,
            BuildingTag.Market => marketTemplate,
            BuildingTag.Church => churchTemplate,
            BuildingTag.Inn => innTemplate,
            BuildingTag.Well => wellTemplate,
            BuildingTag.CottonPlantation => cottonPlantationTemplate,
            BuildingTag.HopsFarm => hopsFarmTemplate,
            BuildingTag.WheatFarm => wheatFarmTemplate,
            BuildingTag.Bakery => bakeryTemplate,
            BuildingTag.Brewery => breweryTemplate,
            BuildingTag.Clothier => clothierTemplate,
            BuildingTag.Forge => forgeTemplate,
            BuildingTag.Windmill => windmillTemplate,
            BuildingTag.FishingHut => fishingHutTemplate,
            BuildingTag.HuntersCabin => huntersCabinTemplate,
            BuildingTag.IronMine => ironMineTemplate,
            BuildingTag.SaltMine => saltMineTemplate,
            BuildingTag.Sawmill => sawmillTemplate,
            BuildingTag.StoneMine => stoneMineTemplate,
            _ => throw new ArgumentException("building " + nameof(buildingTag) + " is missing"),
        };
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
