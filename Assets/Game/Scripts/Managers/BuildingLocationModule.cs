using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLocationModule : MonoBehaviour
{
    Globals globals;
    AbstractMapGenerator mapGenerator;

    // data for placing houses
    BuildingData[,] marketCoverArea;
    BuildingData[,] churchCoverArea;
    BuildingData[,] innCoverArea;
    BuildingData[,] wellCoverArea;

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
    TileAvailability[,] availableSpace;

    TerrainType[,] terrainGrid;
    int gridX;
    int gridY;

    // initialization
    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        mapGenerator = FindObjectOfType<AbstractMapGenerator>();
    }

    public void Initialize()
    {
        terrainGrid = mapGenerator.GetTerrainGrid();
        gridX = terrainGrid.GetLength(0);
        gridY = terrainGrid.GetLength(1);

        marketCoverArea = new BuildingData[gridX, gridY];
        churchCoverArea = new BuildingData[gridX, gridY];
        innCoverArea = new BuildingData[gridX, gridY];
        wellCoverArea = new BuildingData[gridX, gridY];

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

        availableSpace = new TileAvailability[gridX, gridY];

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                availableSpace[i,j] = TileAvailability.Empty;
            }
        }

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (terrainGrid[i,j] == TerrainType.Ground)
                    continue;
                
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int posX = i + dx;
                        int posY = j + dy;
                        if (posX >= 0 && posX < gridX && posY >= 0 && posY < gridY)
                        {
                            if (terrainGrid[posX, posY] == TerrainType.Ground)
                                availableSpace[posX, posY] = TileAvailability.ForRoadOnly;
                            else
                                availableSpace[posX, posY] = TileAvailability.Taken;
                        }
                    }
                }
            }
        }
    }

    public TileAvailability[,] GetAvailableSpace()
    {
        return availableSpace;
    }

    public (int x, int y) PickNewBuildingLocation(BuildingTemplate buildingTemplate)
    {
        // pick location
        (int x, int y) location;
        switch (buildingTemplate.BuildingType)
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
            print("Building space run out for " + buildingTemplate.BuildingTag.ToString());
            return location;
        }

        return location;
    }

    public void UpdateAfterBuildingCreation(BuildingData buildingData, BuildingTemplate buildingTemplate)
    {
        // update building affected areas
        UpdateAvailableSpace(buildingData.gridLocation, buildingTemplate);
        switch (buildingTemplate.BuildingType)
        {
            case BuildingType.Housing:
                UpdateServicesForHouse(buildingData.gridLocation, buildingTemplate);
                break;
            case BuildingType.Service:
                UpdateServiceCoverArea(buildingData, buildingTemplate);
                break;
            case BuildingType.ResourceGathering:
                UpdateResourceCaptureArea(buildingData.gridLocation, buildingTemplate);
                break;
            default:
                break;
        }
    }

    bool[,] GetAvailableBuildingSpots(BuildingTemplate bt)
    {
        var canBuild = new bool[gridX - bt.SizeX + 1, gridY - bt.SizeY + 1];
        for (int x = 0; x < canBuild.GetLength(0); x++)
        {
            for (int y = 0; y < canBuild.GetLength(1); y++)
            {
                if (x > 0 && y > 0 && canBuild[x - 1, y] && canBuild[x, y - 1])
                {
                    if (availableSpace[x + bt.SizeX - 1, y + bt.SizeY - 1] == TileAvailability.Empty)
                        canBuild[x, y] = true;
                }
                else if (x > 0 && canBuild[x - 1, y])
                {
                    canBuild[x, y] = true;
                    for (int j = 0; j < bt.SizeY; j++)
                    {
                        if (availableSpace[x + bt.SizeX - 1, y + j] != TileAvailability.Empty)
                        {
                            canBuild[x, y] = false;
                            break;
                        }
                    }
                }
                else if (y > 0 && canBuild[x, y - 1])
                {
                    canBuild[x, y] = true;
                    for (int i = 0; i < bt.SizeX; i++)
                    {
                        if (availableSpace[x + i, y + bt.SizeY - 1] != TileAvailability.Empty)
                        {
                            canBuild[x, y] = false;
                            break;
                        }
                    }
                }
                else
                {
                    void CheckSpace()
                    {
                        canBuild[x, y] = true;
                        for (int i = 0; i < bt.SizeX; i++)
                        {
                            for (int j = 0; j < bt.SizeY; j++)
                            {
                                if (availableSpace[x + i, y + j] != TileAvailability.Empty)
                                {
                                    canBuild[x, y] = false;
                                    return;
                                }
                            }
                        }
                    }
                    CheckSpace();
                }
            }
        }
        return canBuild;
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
        BuildingData[,] alreadyCovered;
        switch (bt.BuildingTag)
        {
            case BuildingTag.Market:
                uncoveredHouses = uncoveredByMarket;
                alreadyCovered = marketCoverArea;
                break;
            case BuildingTag.Church:
                uncoveredHouses = uncoveredByChurch;
                alreadyCovered = churchCoverArea;
                break;
            case BuildingTag.Inn:
                uncoveredHouses = uncoveredByInn;
                alreadyCovered = innCoverArea;
                break;
            case BuildingTag.Well:
                uncoveredHouses = uncoveredByWell;
                alreadyCovered = wellCoverArea;
                break;
            default:
                throw new Exception("Unknown service building " + bt.BuildingTag.ToString());
        }

        var coverValueGrid = new int[gridX - bt.SizeX + 1, gridY - bt.SizeY + 1];
        int bestCoverValue = 0;
        int numberOfBestSpots = 0;
        for (int i = 0; i < availableForBuilding.GetLength(0); i++)
        {
            for (int j = 0; j < availableForBuilding.GetLength(1); j++)
            {
                if (!availableForBuilding[i,j])
                    continue;
                
                int GetServiceCoverAreaValue(int x, int y)
                {
                    int count;
                    int overlapFactor = 0;
                    if (x > 0 && availableForBuilding[x - 1, y])
                    {
                        count = coverValueGrid[x - 1, y];
                        int leftEdge = x - 1 - bt.CoverArea;
                        int rightEdge = x + bt.CoverArea;
                        for (int j = Math.Max(y - bt.CoverArea, 0); j < Math.Min(y + bt.CoverArea, gridY); j++)
                        {
                            if (leftEdge >= 0)
                            {
                                if (uncoveredHouses[leftEdge, j])
                                    count--;
                                
                                if (alreadyCovered[leftEdge, j] != null)
                                    overlapFactor++;
                            }
                                
                            
                            if (rightEdge < gridX)
                            {
                                if (uncoveredHouses[rightEdge, j])
                                    count++;
                                
                                if (alreadyCovered[rightEdge, j] != null)
                                    overlapFactor--;
                            }
                        }
                    }
                    else if (y > 0 && availableForBuilding[x, y - 1])
                    {
                        count = coverValueGrid[x, y - 1];
                        int bottomEdge = y - 1 - bt.CoverArea;
                        int topEdge = y + bt.CoverArea;
                        for (int i = Math.Max(x - bt.CoverArea, 0); i < Math.Min(x + bt.CoverArea, gridX); i++)
                        {
                            if (bottomEdge >= 0)
                            {
                                if (uncoveredHouses[i, bottomEdge])
                                    count--;

                                if (alreadyCovered[i, bottomEdge] != null)
                                    overlapFactor++;
                            }
                            
                            if (topEdge < gridY && uncoveredHouses[i, topEdge])
                                count++;
                        }
                    }
                    else
                    {
                        count = 0;
                        for (int i = Math.Max(x - bt.CoverArea, 0); i < Math.Min(x + bt.CoverArea, gridX); i++)
                        {
                            for (int j = Math.Max(y - bt.CoverArea, 0); j < Math.Min(y + bt.CoverArea, gridY); j++)
                            {
                                if (uncoveredHouses[i, j])
                                    count++;
                            }
                        }
                    }
                    return count;
                }
                
                int coverValue = GetServiceCoverAreaValue(i, j);
                coverValueGrid[i, j] = coverValue;
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

        if (bestCoverValue == 0)
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

    // TODO logic is wrong, any building space, not just top left corner; similar problem in other cases
    (int, int) PickLocationNearResource(BuildingTemplate buildingTemplate)
    {
        ResourceGatheringBT bt = (ResourceGatheringBT)buildingTemplate;

        var availableForBuilding = GetAvailableBuildingSpots(bt);
        var resourceCoverGrid = CreateResourceCoverGrid(bt.GroundResource, bt.MinDistanceToResource);
        bool[,] captureArea;
        switch (bt.BuildingTag)
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
                throw new Exception("Resource gathering building " + bt.BuildingTag.ToString() + " is missing");
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
        int leftEdge = location.x - 1;
        int rightEdge = location.x + bt.SizeX;
        int bottomEdge = location.y - 1;
        int topEdge = location.y + bt.SizeY;
        for (int i = Math.Max(leftEdge, 0); i < Math.Min(rightEdge + 1, gridX - 1); i++)// TODO available space yes, no, roads-only
        {
            for (int j = Math.Max(bottomEdge, 0); j < Math.Min(topEdge + 1, gridY - 1); j++)
            {
                if (i == leftEdge || i == rightEdge || j == bottomEdge || j == topEdge)
                    availableSpace[i,j] = TileAvailability.ForRoadOnly;
                else
                    availableSpace[i,j] = TileAvailability.Taken;
            }
        }
    }

    void UpdateServicesForHouse((int x, int y) pos, BuildingTemplate bt)
    {
        uncoveredByMarket[pos.x, pos.y] = !marketCoverArea[pos.x, pos.y];
        uncoveredByChurch[pos.x, pos.y] = !churchCoverArea[pos.x, pos.y];
        uncoveredByInn[pos.x, pos.y] = !innCoverArea[pos.x, pos.y];
        uncoveredByWell[pos.x, pos.y] = !wellCoverArea[pos.x, pos.y];
    }

    // TODO top left issue (adjust for service building size; adjust for house overlap 11,11,10,10)
    void UpdateServiceCoverArea(BuildingData buildingData, BuildingTemplate buildingTemplate)
    {
        ServiceBT bt = (ServiceBT)buildingTemplate;
        bool[,] uncoveredByService;
        BuildingData[,] serviceCoverArea;
        switch (bt.BuildingTag)
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

        for (int i = Math.Max(buildingData.gridLocation.x - bt.CoverArea, 0); i < Math.Min(buildingData.gridLocation.x + bt.CoverArea, gridX); i++)
        {
            for (int j = Math.Max(buildingData.gridLocation.y - bt.CoverArea, 0); j < Math.Min(buildingData.gridLocation.y + bt.CoverArea, gridY); j++)
            {
                uncoveredByService[i,j] = false;
                serviceCoverArea[i,j] = buildingData;
            }
        }
    }

    void UpdateResourceCaptureArea((int x, int y) location, BuildingTemplate buildingTemplate)
    {
        ResourceGatheringBT bt = (ResourceGatheringBT) buildingTemplate;

        bool[,] captureArea;
        switch (bt.BuildingTag)
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
                throw new Exception("Resource gathering building " + bt.BuildingTag.ToString() + " is missing");
        }

        for (int i = Math.Max(location.x - bt.CaptureGatheringArea, 0); i < Math.Min(location.x + bt.CaptureGatheringArea, gridX); i++)
        {
            for (int j = Math.Max(location.y - bt.CaptureGatheringArea, 0); j < Math.Min(location.y + bt.CaptureGatheringArea, gridY); j++)
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

    public BuildingData GetClosestService(BuildingData house, BuildingTag buildingTag)
    {
        switch (buildingTag)
        {
            case BuildingTag.Market:
                return marketCoverArea[house.gridLocation.x, house.gridLocation.y];
            case BuildingTag.Church:
                return churchCoverArea[house.gridLocation.x, house.gridLocation.y];
            case BuildingTag.Inn:
                return innCoverArea[house.gridLocation.x, house.gridLocation.y];
            case BuildingTag.Well:
                return wellCoverArea[house.gridLocation.x, house.gridLocation.y];
            default:
                throw new Exception("Unknown service " + buildingTag.ToString());
        }
    }
}

public enum TileAvailability
{
    Empty,
    ForRoadOnly,
    Taken
}
