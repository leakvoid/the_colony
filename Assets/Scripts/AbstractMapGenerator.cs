using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractMapGenerator : MonoBehaviour
{
    [SerializeField] int gridX = 1000;
    [SerializeField] int gridY = 1000;

    // resources
    [SerializeField] int forestPercentage = 15;
    [SerializeField] int numberOfForests = 8;

    [SerializeField] int lakePercentage = 10;
    [SerializeField] int numberOfLakes = 6;

    [SerializeField] int ironDepositPercentage = 2;
    [SerializeField] int numberOfIronDeposits = 5;

    [SerializeField] int stoneDepositPercentage = 2;
    [SerializeField] int numberOfStoneDeposits = 5;

    [SerializeField] int saltDepositPercentage = 2;
    [SerializeField] int numberOfSaltDeposits = 5;

    Globals.TileType[,] grid;

    void Start()
    {
        grid = new Globals.TileType[gridX, gridY];

        GenerateTerrainClusters(forestPercentage, numberOfForests, Globals.TileType.Forest);
        GenerateTerrainClusters(lakePercentage, numberOfLakes, Globals.TileType.Water);
        GenerateTerrainClusters(ironDepositPercentage, numberOfIronDeposits, Globals.TileType.IronDeposit);
        GenerateTerrainClusters(stoneDepositPercentage, numberOfStoneDeposits, Globals.TileType.StoneDeposit);
        GenerateTerrainClusters(saltDepositPercentage, numberOfSaltDeposits, Globals.TileType.SaltDeposit);
    }

    void GenerateTerrainClusters(int tilePercentage, int numberOfClusters, Globals.TileType tileType)
    {
        // get clusters of randomized size
        int totalTileCount = gridX * gridY * tilePercentage / 100;

        var clusterTileCount = new int[numberOfClusters];
        int avgTileCount = totalTileCount / numberOfClusters;
        clusterTileCount[0] = avgTileCount;
        for (int i = 1; i < numberOfClusters; i++)
        {
            int variance = avgTileCount * Random.Range(0, 71) / 100;
            clusterTileCount[i - 1] -= variance;
            clusterTileCount[i] = avgTileCount + variance;
        }

        for (int i = 0; i < numberOfClusters; i++)
        {
            // get starting point for cluster
            (int x, int y) startingPos;
            while (true)
            {
                startingPos = (Random.Range(0, gridX), Random.Range(0, gridY));
                if (grid[startingPos.x, startingPos.y] == Globals.TileType.Ground)
                    break;
            }

            // generate cluster
            GenerateCluster(startingPos, clusterTileCount[i], tileType);
        }

    }

    [SerializeField] int tilePropagationChance = 70;

    // TODO issues: 1. tileCount not always exhausted within enclosed areas / circular random paths 2. can still create closed off spaces
    void GenerateCluster((int x,int y) startingPos, int tileCount, Globals.TileType tileType)
    {
        var edges = new Queue<(int x, int y)>();

        tileCount -= 1;
        grid[startingPos.x, startingPos.y] = tileType;
        edges.Enqueue(startingPos);

        print("Tile count start: " + tileCount);

        while (tileCount > 0 && edges.Count > 0)
        {
            int potentialEdgeCount = 0;
            foreach (var pos in edges)
            {
                if (IsValidLeft(pos))
                    potentialEdgeCount++;
                if (IsValidRight(pos))
                    potentialEdgeCount++;
                if (IsValidUp(pos))
                    potentialEdgeCount++;
                if (IsValidDown(pos))
                    potentialEdgeCount++;
            }

            int tilesToGenerate = potentialEdgeCount * tilePropagationChance / 100;
            if (tilesToGenerate == 0)
                tilesToGenerate = 1;
            if (tilesToGenerate > tileCount)
                tilesToGenerate = tileCount;

            int edgesCount = edges.Count;
            for (int i = 0; i < edgesCount; i++)
            {
                (int x, int y) pos = edges.Dequeue();

                if (IsValidLeft(pos))
                    TryAddingTile((pos.x - 1, pos.y), tileType, edges, ref tilesToGenerate, ref potentialEdgeCount);
                if (IsValidRight(pos))
                    TryAddingTile((pos.x + 1, pos.y), tileType, edges, ref tilesToGenerate, ref potentialEdgeCount);
                if (IsValidUp(pos))
                    TryAddingTile((pos.x, pos.y + 1), tileType, edges, ref tilesToGenerate, ref potentialEdgeCount);
                if (IsValidDown(pos))
                    TryAddingTile((pos.x, pos.y - 1), tileType, edges, ref tilesToGenerate, ref potentialEdgeCount);
            }

            tileCount -= tilesToGenerate;
        }

        print("Tile count end: " + tileCount);
    }

    void TryAddingTile((int x, int y) pos, Globals.TileType tileType, Queue<(int x, int y)> edges, ref int tilesToGenerate, ref int potentialEdgeCount)
    {
        potentialEdgeCount -= 1;

        // surrounded ground tile case
        if (IsValidLeft(pos, tileType) &&
            IsValidRight(pos, tileType) &&
            IsValidUp(pos, tileType) &&
            IsValidDown(pos, tileType))
        {
            grid[pos.x, pos.y] = tileType;
            tilesToGenerate -= 1;
            return;
        }

        if (potentialEdgeCount <= 0 || tilesToGenerate <= 0)
            return;

        // try to add tile
        float tileChance = tilesToGenerate / potentialEdgeCount;
        if (Random.Range(0,1) <= tileChance)
        {
            grid[pos.x, pos.y] = tileType;
            tilesToGenerate -= 1;
            edges.Enqueue(pos);
        }
    }

    bool IsValidLeft((int x, int y) pos, Globals.TileType tileType = Globals.TileType.Ground)
    {
        return (pos.x - 1 >= 0 && grid[pos.x - 1, pos.y] == tileType);
    }

    bool IsValidRight((int x, int y) pos, Globals.TileType tileType = Globals.TileType.Ground)
    {
        return (pos.x + 1 < gridX && grid[pos.x + 1, pos.y] == tileType);
    }

    bool IsValidUp((int x, int y) pos, Globals.TileType tileType = Globals.TileType.Ground)
    {
        return (pos.y + 1 < gridY && grid[pos.x, pos.y + 1] == tileType);
    }

    bool IsValidDown((int x, int y) pos, Globals.TileType tileType = Globals.TileType.Ground)
    {
        return (pos.y - 1 >= 0 && grid[pos.x, pos.y - 1] == tileType);
    }
}
