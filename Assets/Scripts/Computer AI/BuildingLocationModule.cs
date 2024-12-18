using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BuildingLocationModule : MonoBehaviour
{
    Globals globals;
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
        globals = FindObjectOfType<Globals>();
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

    public (int x, int y) PickNewBuildingLocation(BuildingTag buildingTag)
    {
        BuildingTemplate buildingTemplate = globals.NameToTemplate(buildingTag);

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
            return location;
        }

        // place building
        // TODO instantiate building and place prefab onto scene
        // TODO split into BuildingLocationFinder and BuildingInstantiationModule

        // update building affected areas
        UpdateAvailableSpace(location, buildingTemplate);
        switch (buildingTemplate.buildingType)
        {
            case BuildingType.Housing:
                UpdateServicesForHouse(location, buildingTemplate);
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
        
        return location;
    }

    bool[,] GetAvailableBuildingSpots(BuildingTemplate bt)
    {
        // TODO O(x^4) can be optimized
        var availableForBuilding = new bool[gridX - bt.sizeX + 1, gridY - bt.sizeY + 1];
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
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
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
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

        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
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

    // TODO hacky
    int lastCoverValue = 0;
    public int GetServiceCoverValue()
    {
        return lastCoverValue;
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
                throw new Exception("Unknown service building " + bt.buildingName);
        }

        var coverValueGrid = new int[gridX - bt.sizeX + 1, gridY - bt.sizeY + 1];
        int bestCoverValue = 0;
        int numberOfBestSpots = 0;
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
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

        if (numberOfBestSpots == 0)// TODO bestCoverValue == 0 ?
            return (-1, -1);

        lastCoverValue = bestCoverValue;

        int chosenPos = UnityEngine.Random.Range(0, numberOfBestSpots);

        int currentPos = -1;
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
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
        for (int i = Math.Max(x - bt.coverArea, 0); i < Math.Min(x + bt.coverArea, gridX); i++)
        {
            for (int j = Math.Max(y - bt.coverArea, 0); j < Math.Min(y + bt.coverArea, gridY); j++)
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
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
            {
                if (availableForBuilding[i,j] && !captureArea[i,j] && resourceCoverGrid[i,j])
                    availableLocations++;
            }
        }

        if (availableLocations == 0)
            return (-1, -1);

        int chosenPos = UnityEngine.Random.Range(0, availableLocations);
        int currentPos = -1;

        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
            {
                if (availableForBuilding[i,j] && !captureArea[i,j] && resourceCoverGrid[i,j])
                {
                    currentPos++;
                    if(currentPos == chosenPos)
                        return (i, j);
                }
            }
        }

        throw new Exception("Should never happen. Picked random resource gathering position not found");
    }

    bool[,] CreateResourceCoverGrid(TerrainType groundResource, int range)
    {
        void FillGrid(int x1, int x2, int y1, int y2, bool[,] resourceCoverGrid)
        {
            for (int i = Math.Max(x1, 0); i < Math.Min(x2 + 1, gridX); i++)
            {
                for (int j = Math.Max(y1, 0); j < Math.Min(y2 + 1, gridY); j++)
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

    (int, int) PickRandomBuildingLocation(BuildingTemplate bt)
    {
        var availableForBuilding = GetAvailableBuildingSpots(bt);

        int availableLocations = 0;
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
            {
                if (availableForBuilding[i,j])
                    availableLocations++;
            }
        }

        if (availableLocations == 0)
            return (-1, -1);

        int chosenPos = UnityEngine.Random.Range(0, availableLocations);
        int currentPos = -1;

        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
            {
                if (availableForBuilding[i,j])
                {
                    currentPos++;
                    if(currentPos == chosenPos)
                        return (i, j);
                }
            }
        }

        throw new Exception("Should never happen. Picked random building position not found");
    }

    void UpdateAvailableSpace((int x, int y) location, BuildingTemplate bt)
    {
        for (int i = location.x; i < location.x + bt.sizeX; i++)
        {
            for (int j = location.y; j < location.y + bt.sizeY; j++)
            {
                availableSpace[i,j] = false;
            }
        }
    }

    // TODO top left corner issue
    void UpdateServicesForHouse((int x, int y) pos, BuildingTemplate bt)
    {
        uncoveredByMarket[pos.x, pos.y] = !marketCoverArea[pos.x, pos.y];
        uncoveredByChurch[pos.x, pos.y] = !churchCoverArea[pos.x, pos.y];
        uncoveredByInn[pos.x, pos.y] = !innCoverArea[pos.x, pos.y];
        uncoveredByWell[pos.x, pos.y] = !wellCoverArea[pos.x, pos.y];
    }

    // TODO top left issue (adjust for service building size; adjust for house overlap 11,11,10,10)
    void UpdateServiceCoverArea((int x, int y) location, BuildingTemplate buildingTemplate)
    {
        ServiceBT bt = (ServiceBT)buildingTemplate;
        bool[,] uncoveredByService;
        bool[,] serviceCoverArea;
        switch (bt.buildingTag)
        {
            case BuildingTag.Market:
                uncoveredByService = uncoveredByMarket;
                serviceCoverArea = marketCoverArea;
                break;
            case BuildingTag.Church:
                uncoveredByService = uncoveredByChurch;
                serviceCoverArea = churchCoverArea;
                break;
            case BuildingTag.Inn:
                uncoveredByService = uncoveredByInn;
                serviceCoverArea = innCoverArea;
                break;
            case BuildingTag.Well:
                uncoveredByService = uncoveredByWell;
                serviceCoverArea = wellCoverArea;
                break;
            default:
                throw new Exception("Service not found");
        }

        for (int i = Math.Max(location.x - bt.coverArea, 0); i < Math.Min(location.x + bt.coverArea, gridX); i++)
        {
            for (int j = Math.Max(location.y - bt.coverArea, 0); j < Math.Min(location.y + bt.coverArea, gridY); j++)
            {
                uncoveredByService[i,j] = false;
                serviceCoverArea[i,j] = true;
            }
        }
    }

    void UpdateResourceCaptureArea((int x, int y) location, BuildingTemplate buildingTemplate)
    {
        ResourceGatheringBT bt = (ResourceGatheringBT) buildingTemplate;

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

        for (int i = Math.Max(location.x - bt.captureGatheringArea, 0); i < Math.Min(location.x + bt.captureGatheringArea, gridX); i++)
        {
            for (int j = Math.Max(location.y - bt.captureGatheringArea, 0); j < Math.Min(location.y + bt.captureGatheringArea, gridY); j++)
            {
                captureArea[i,j] = true;
            }
        }
    }

    public bool CheckServiceOverlap((int x, int y) location, BuildingTag buildingTag)
    {
        if (buildingTag == BuildingTag.Market)
            return marketCoverArea[location.x, location.y];
        if (buildingTag == BuildingTag.Well)
            return wellCoverArea[location.x, location.y];
        if (buildingTag == BuildingTag.Church)
            return churchCoverArea[location.x, location.y];
        if (buildingTag == BuildingTag.Inn)
            return innCoverArea[location.x, location.y];
        return false;
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

    /*

    1. citizen logic
        if need low -> check service availability (via house) -> is building constructed ->
         -> (check resource availability) -> (reserve resource purchase) -> go to service building
        go to work -> start working

    2. computer AI logic
        houses rather than workers should generate pressure to build services

        (house)

        (resource gathering)

        (processing)

        (service)
        check service availability -> pressure to build
        pressure to build reaches threshold -> construct building

        building construction available -> allocate builders and send workers
        building finished -> send workers

    3. temporal logic

        citizens:
        idle (default)
        walking (time is varied, cannot be precomputed)
        working (fixed duration)
        consuming (fixed duration)

    4. road generation

    5. graphics


    what is the interplay between needs fulfillment and worker growth?
    */
}
