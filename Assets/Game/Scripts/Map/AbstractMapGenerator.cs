using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMapGenerator : MonoBehaviour
{
    [SerializeField] int gridX = 150;
    [SerializeField] int gridY = 150;

    // resources
    [SerializeField] float forestPercentage = 15;
    [SerializeField] int numberOfForests = 8;

    [SerializeField] float lakePercentage = 10;
    [SerializeField] int numberOfLakes = 6;

    [SerializeField] float ironDepositPercentage = 0.5f;
    [SerializeField] int numberOfIronDeposits = 5;

    [SerializeField] float stoneDepositPercentage = 0.5f;
    [SerializeField] int numberOfStoneDeposits = 5;

    [SerializeField] float saltDepositPercentage = 0.5f;
    [SerializeField] int numberOfSaltDeposits = 5;

    TerrainType[,] grid;
    bool[,] takenByCluster;

    public TerrainType[,] GetTerrainGrid()
    {
        return grid;
    }

    public void GenerateNewMap()
    {
        grid = new TerrainType[gridX, gridY];
        takenByCluster = new bool[gridX, gridY];
        GenerateTerrainClusters(forestPercentage, numberOfForests, TerrainType.Forest);
        GenerateTerrainClusters(lakePercentage, numberOfLakes, TerrainType.Water);
        GenerateTerrainClusters(ironDepositPercentage, numberOfIronDeposits, TerrainType.IronDeposit);
        GenerateTerrainClusters(stoneDepositPercentage, numberOfStoneDeposits, TerrainType.StoneDeposit);
        GenerateTerrainClusters(saltDepositPercentage, numberOfSaltDeposits, TerrainType.SaltDeposit);
        FillTerrainGaps();
    }

    void GenerateTerrainClusters(float tilePercentage, int numberOfClusters, TerrainType tileType)
    {
        if (numberOfClusters == 0)
            return;

        // get clusters of randomized size
        int totalTileCount = (int)(gridX * gridY * tilePercentage / 100);

        var clusterTileCount = new int[numberOfClusters];
        int avgTileCount = totalTileCount / numberOfClusters;
        clusterTileCount[0] = avgTileCount;
        for (int i = 1; i < numberOfClusters; i++)
        {
            int variance = avgTileCount * Random.Range(0, 51) / 100;
            clusterTileCount[i - 1] -= variance;
            clusterTileCount[i] = avgTileCount + variance;
        }

        for (int i = 0; i < numberOfClusters; i++)
        {
            // get starting point for cluster
            (int x, int y) pos;
            while (true)
            {
                pos = (Random.Range(1, gridX - 1), Random.Range(1, gridY - 1));
                if (grid[pos.x, pos.y] == TerrainType.Ground &&
                    ScanLeft(pos) &&
                    ScanRight(pos) &&
                    ScanUp(pos) &&
                    ScanDown(pos) &&
                    ScanUp((pos.x - 1, pos.y)) &&
                    ScanDown((pos.x - 1, pos.y)) &&
                    ScanUp((pos.x + 1, pos.y)) &&
                    ScanDown((pos.x + 1, pos.y)))
                    break;
            }

            // generate cluster
            GenerateCluster(pos, clusterTileCount[i], tileType);
        }

    }

    [SerializeField] int tilePropagationChance = 60;

    void GenerateCluster((int x,int y) startingPos, int tileCount, TerrainType tileType)
    {
        var edges = new Queue<(int x, int y)>();
        var clusterPos = new bool[gridX, gridY];

        tileCount -= 1;
        grid[startingPos.x, startingPos.y] = tileType;
        edges.Enqueue(startingPos);

        while (tileCount > 0 && edges.Count > 0)
        {
            int potentialEdgeCount = 0;
            foreach (var pos in edges)
            {
                if (IsValidLeft(pos, tileType))
                    potentialEdgeCount++;
                if (IsValidRight(pos, tileType))
                    potentialEdgeCount++;
                if (IsValidUp(pos, tileType))
                    potentialEdgeCount++;
                if (IsValidDown(pos, tileType))
                    potentialEdgeCount++;
            }

            int tilesToGenerate = potentialEdgeCount * tilePropagationChance / 100;
            if (tilesToGenerate == 0)
                tilesToGenerate = 1;
            if (tilesToGenerate > tileCount)
                tilesToGenerate = tileCount;
            tileCount -= tilesToGenerate;

            int edgesCount = edges.Count;
            for (int i = 0; i < edgesCount; i++)
            {
                (int x, int y) pos = edges.Dequeue();

                if (IsValidLeft(pos, tileType))
                    TryAddingTile((pos.x - 1, pos.y), tileType, edges, clusterPos, ref tilesToGenerate, ref potentialEdgeCount);
                if (IsValidRight(pos, tileType))
                    TryAddingTile((pos.x + 1, pos.y), tileType, edges, clusterPos, ref tilesToGenerate, ref potentialEdgeCount);
                if (IsValidUp(pos, tileType))
                    TryAddingTile((pos.x, pos.y + 1), tileType, edges, clusterPos, ref tilesToGenerate, ref potentialEdgeCount);
                if (IsValidDown(pos, tileType))
                    TryAddingTile((pos.x, pos.y - 1), tileType, edges, clusterPos, ref tilesToGenerate, ref potentialEdgeCount);
            }
        }

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (clusterPos[i, j])
                    takenByCluster[i, j] = true;
            }
        }
    }

    void TryAddingTile((int x, int y) pos, TerrainType tileType, Queue<(int x, int y)> edges,
                        bool[,] clusterPos, ref int tilesToGenerate, ref int potentialEdgeCount)
    {
        // surrounded ground tile case
        if (potentialEdgeCount <= 0 || tilesToGenerate <= 0)
            return;

        // try to add tile
        float tileChance = (float)tilesToGenerate / potentialEdgeCount;
        if (Random.Range(0f, 1f) <= tileChance)
        {
            grid[pos.x, pos.y] = tileType;
            clusterPos[pos.x, pos.y] = true;
            tilesToGenerate -= 1;
            edges.Enqueue(pos);
        }

        potentialEdgeCount -= 1;
    }

    bool IsValidLeft((int x, int y) pos, TerrainType tileType)
    {
        int dx = pos.x - 1;
        (int, int) shift = (pos.x - 2, pos.y);
        return (dx >= 0 &&
            grid[dx, pos.y] == TerrainType.Ground &&
            ScanLeft((dx, pos.y)) &&
            ScanUp(shift) &&
            ScanDown(shift));
    }

    bool IsValidRight((int x, int y) pos, TerrainType tileType)
    {
        int dx = pos.x + 1;
        (int, int) shift = (pos.x + 2, pos.y);
        return (dx < gridX &&
            grid[dx, pos.y] == TerrainType.Ground &&
            ScanRight((dx, pos.y)) &&
            ScanUp(shift) &&
            ScanDown(shift));
    }

    bool IsValidUp((int x, int y) pos, TerrainType tileType)
    {
        int dy = pos.y + 1;
        (int, int) shift = (pos.x, pos.y + 2);
        return (dy < gridY &&
            grid[pos.x, dy] == TerrainType.Ground &&
            ScanUp((pos.x, dy)) &&
            ScanLeft(shift) &&
            ScanRight(shift));
    }

    bool IsValidDown((int x, int y) pos, TerrainType tileType)
    {
        int dy = pos.y - 1;
        (int, int) shift = (pos.x, pos.y - 2);
        return (dy >= 0 &&
            grid[pos.x, dy] == TerrainType.Ground &&
            ScanDown((pos.x, dy)) &&
            ScanLeft(shift) &&
            ScanRight(shift));
    }

    bool ScanLeft((int x, int y) pos)
    {
        int dx = pos.x - 1;
        return (dx >= 0 && !takenByCluster[dx, pos.y]);
    }

    bool ScanRight((int x, int y) pos)
    {
        int dx = pos.x + 1;
        return (dx < gridX && !takenByCluster[dx, pos.y]);
    }

    bool ScanUp((int x, int y) pos)
    {
        int dy = pos.y + 1;
        return (dy < gridY && !takenByCluster[pos.x, dy]);
    }

    bool ScanDown((int x, int y) pos)
    {
        int dy = pos.y - 1;
        return (dy >= 0 && !takenByCluster[pos.x, dy]);
    }

    void FillTerrainGaps()
    {
        void FillGroundGrid(bool[,] groundGrid, (int x, int y) pos)
        {
            if (pos.x < 0 || pos.x >= gridX || pos.y < 0 || pos.y >= gridY ||
                groundGrid[pos.x, pos.y] || grid[pos.x, pos.y] != TerrainType.Ground)
                return;
            
            groundGrid[pos.x, pos.y] = true;

            FillGroundGrid(groundGrid, (pos.x - 1, pos.y));
            FillGroundGrid(groundGrid, (pos.x + 1, pos.y));
            FillGroundGrid(groundGrid, (pos.x, pos.y - 1));
            FillGroundGrid(groundGrid, (pos.x, pos.y + 1));
        }

        bool[,] groundGrid = new bool[gridX, gridY];
        FillGroundGrid(groundGrid, (0, 0));
        
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (groundGrid[i, j] == false && grid[i, j] == TerrainType.Ground)
                {
                    grid[i, j] = grid[i - 1, j];
                }
            }
        }
    }
}
