using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainDepositsGenerator : MonoBehaviour
{
    TerrainType[,] terrainGrid;
    int terrainGridX;
    int terrainGridY;

    [SerializeField] GameObject stoneMeshPrefab;
    [SerializeField] GameObject ironMeshPrefab;
    [SerializeField] GameObject saltMeshPrefab;

    class DepositInfo
    {
        public int left;
        public int right;
        public int top;
        public int bottom;
        public TerrainType type;
        public int index;

        public DepositInfo(int _left, int _right, int _top, int _bottom, TerrainType _type, int _index)
        {
            left = _left;
            right = _right;
            top = _top;
            bottom = _bottom;
            type = _type;
            index = _index;
        }
    }
    Dictionary<int, DepositInfo> deposits;
    int[,] markedDeposits;

    public void Initialize()
    {
        AbstractMapGenerator amg = FindObjectOfType<AbstractMapGenerator>();
        terrainGrid = amg.GetTerrainGrid();
        terrainGridX = terrainGrid.GetLength(0);
        terrainGridY = terrainGrid.GetLength(1);

        deposits = new Dictionary<int, DepositInfo>();
        markedDeposits = new int[terrainGrid.GetLength(0), terrainGrid.GetLength(1)];
        int depositIndex = 0;
        for (int x = 0; x < terrainGrid.GetLength(0); x++)
        {
            for (int y = 0; y < terrainGrid.GetLength(1); y++)
            {
                if ((terrainGrid[x, y] == TerrainType.IronDeposit ||
                    terrainGrid[x, y] == TerrainType.SaltDeposit ||
                    terrainGrid[x, y] == TerrainType.StoneDeposit) &&
                    markedDeposits[x, y] == 0)
                {
                    depositIndex++;
                    deposits[depositIndex] = new DepositInfo(x, x, y, y, terrainGrid[x, y], depositIndex);
                    MarkDepositChunk((x, y), depositIndex);
                }
            }
        }

        foreach (var deposit in deposits)
        {
            CreateDepositMesh(deposit.Value);
        }
    }

    void MarkDepositChunk((int x, int y) pos, int depositIndex)
    {
        DepositInfo deposit = deposits[depositIndex];
        if (pos.x < 0 || pos.x >= terrainGridX || pos.y < 0 || pos.y >= terrainGridY ||
            terrainGrid[pos.x, pos.y] != deposit.type || markedDeposits[pos.x, pos.y] == depositIndex)
            return;
        
        markedDeposits[pos.x, pos.y] = depositIndex;
        if (pos.x < deposit.left)
            deposit.left = pos.x;
        if (pos.x > deposit.right)
            deposit.right = pos.x;
        if (pos.y > deposit.top)
            deposit.top = pos.y;
        if (pos.y < deposit.bottom)
            deposit.bottom = pos.y;
        
        MarkDepositChunk((pos.x + 1, pos.y), depositIndex);
        MarkDepositChunk((pos.x - 1, pos.y), depositIndex);
        MarkDepositChunk((pos.x, pos.y + 1), depositIndex);
        MarkDepositChunk((pos.x, pos.y - 1), depositIndex);
    }

    [SerializeField] int granularity = 10;
    [SerializeField] int depositSlope = 4;
    [SerializeField] float noiseScale = 0.3f;
    [SerializeField] float maxHeight = 2f;

    void CreateDepositMesh(DepositInfo deposit)
    {
        int sizeX = deposit.right - deposit.left + 1;
        int sizeY = deposit.top - deposit.bottom + 1;
        int refinedSizeX = sizeX * granularity;
        int refinedSizeY = sizeY * granularity;

        // needlessly overcomplicated way to count vertices
        int totalVertices = 0;
        int totalDepositCells = 0;
        for (int y = deposit.bottom; y < deposit.bottom + sizeY; y++)
        {
            for (int x = deposit.left; x < deposit.left + sizeX; x++)
            {
                if (markedDeposits[x, y] != deposit.index)
                    continue;
                totalDepositCells++;

                int newVertices = (granularity + 1) * (granularity + 1);
                if (y > 0)
                {
                    if (markedDeposits[x, y - 1] == deposit.index)
                    {
                        newVertices -= granularity + 1;
                    }
                    else
                    {
                        if (x > 0 && markedDeposits[x - 1, y - 1] == deposit.index)
                            newVertices -= 1;
                        if ((x + 1) < (deposit.left + sizeX) && markedDeposits[x + 1, y - 1] == deposit.index)
                            newVertices -= 1;
                    }
                }
                if (x > 0 && markedDeposits[x - 1, y] == deposit.index)
                {
                    newVertices -= granularity + 1;
                    if (y > 0 && (markedDeposits[x - 1, y - 1] == deposit.index || markedDeposits[x, y - 1] == deposit.index))
                        newVertices += 1;
                }
                totalVertices += newVertices;
            }
        }

        int noiseOffset = UnityEngine.Random.Range(0,100);

        Vector3[] vertices = new Vector3[totalVertices];
        int[,] vertexHelper = new int[refinedSizeX + 1,refinedSizeY + 1];
        var uvs = new Vector2[totalVertices];
        for (int idx = 0, y = 0; y <= refinedSizeY; y++)
        {
            int gridY = deposit.bottom + y / granularity;
            int leftGridY = (y > 0) ? (deposit.bottom + (y - 1) / granularity) : (deposit.bottom - 1);

            for (int x = 0; x <= refinedSizeX; x++)
            {
                int gridX = deposit.left + x / granularity;
                int leftGridX = (x > 0) ? (deposit.left + (x - 1) / granularity) : (deposit.left - 1);

                if (markedDeposits[gridX, gridY] == deposit.index ||
                    markedDeposits[leftGridX, gridY] == deposit.index ||
                    markedDeposits[gridX, leftGridY] == deposit.index ||
                    markedDeposits[leftGridX, leftGridY] == deposit.index)
                {
                    float leftMultiplier = GetEdgeFalloff(gridX - 1, gridY, x % granularity);
                    float rightMultiplier = GetEdgeFalloff(gridX + 1, gridY, granularity - 1 - x % granularity);
                    float bottomMultiplier = GetEdgeFalloff(gridX, gridY - 1, y % granularity);
                    float topMultiplier = GetEdgeFalloff(gridX, gridY + 1, granularity - 1 - y % granularity);

                    float shift = Mathf.Min(topMultiplier, bottomMultiplier, leftMultiplier, rightMultiplier);
                    shift = 1 - shift;
                    if (markedDeposits[gridX, gridY] != deposit.index ||
                        markedDeposits[leftGridX, gridY] != deposit.index ||
                        markedDeposits[gridX, leftGridY] != deposit.index ||
                        markedDeposits[leftGridX, leftGridY] != deposit.index)
                        shift = 1f;

                    float noiseValue = 0.05f + Mathf.PerlinNoise((x + noiseOffset) * noiseScale, (y + noiseOffset) * noiseScale) * 0.95f;
                    float height = (noiseValue - shift) * maxHeight;
                    vertices[idx] = Globals.NewVector((float)x / granularity, (float)y / granularity, height);
                    vertexHelper[x, y] = idx;
                    uvs[idx] = new Vector2 (x / (float)refinedSizeX, y / (float)refinedSizeY);
                    idx++;
                }
            }
        }

        int[] triangles = new int[totalDepositCells * granularity * granularity * 6];
        int ti = 0;
        for (int y = 0; y < refinedSizeY; y++)
        {
            int gridY = deposit.bottom + y / granularity;
            for (int x = 0; x < refinedSizeX; x++)
            {
                int gridX = deposit.left + x / granularity;
                if (markedDeposits[gridX, gridY] == deposit.index)
                {
                    triangles[ti] = vertexHelper[x, y];
                    triangles[ti + 1] = vertexHelper[x, y + 1];
                    triangles[ti + 2] = vertexHelper[x + 1, y];
                    triangles[ti + 3] = vertexHelper[x + 1, y];
                    triangles[ti + 4] = vertexHelper[x, y + 1];
                    triangles[ti + 5] = vertexHelper[x + 1, y + 1];
                    ti += 6;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GameObject prefab;
        if (deposit.type == TerrainType.StoneDeposit)
            prefab = stoneMeshPrefab;
        else if (deposit.type == TerrainType.IronDeposit)
            prefab = ironMeshPrefab;
        else
            prefab = saltMeshPrefab;

        var depositInstance = Instantiate(prefab, Globals.NewVector(deposit.left, deposit.bottom, 0), Quaternion.identity);
        MeshFilter meshFilter = depositInstance.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    float GetEdgeFalloff(int x, int y, int posInCell)
    {
        if (x < 0 || x >= terrainGrid.GetLength(0) || y < 0 || y >= terrainGrid.GetLength(1) || terrainGrid[x, y] == TerrainType.Ground)
        {
            return Mathf.InverseLerp(0, depositSlope, posInCell);
        }
        return 1f;
    }
}
