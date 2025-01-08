using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainDepositsGenerator : MonoBehaviour
{
    TerrainType[,] terrainGrid;
    int terrainGridX;
    int terrainGridY;

    [SerializeField] GameObject depositMeshPrefab;

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
        int refinedSizeX = (deposit.right - deposit.left + 1) * granularity;
        int refinedSizeY = (deposit.top - deposit.bottom + 1) * granularity;
        Vector3[] vertices = new Vector3[(refinedSizeX + 1) * (refinedSizeY + 1)];// TODO generate mesh only for non-terrain
        for (int i = 0, y = 0; y <= refinedSizeY; y++)
        {
            int gridY = deposit.bottom + y / granularity;
            for (int x = 0; x <= refinedSizeX; x++, i++)
            {
                int gridX = deposit.left + x / granularity;
                float height = 0f;

                if (terrainGrid[gridX, gridY] != TerrainType.Ground && markedDeposits[gridX, gridY] == deposit.index)
                {
                    float topMultiplier = GetEdgeFalloff(gridX, gridY + 1, granularity - 1 - y % granularity);
                    float bottomMultiplier = GetEdgeFalloff(gridX, gridY - 1, y % granularity);
                    float leftMultiplier = GetEdgeFalloff(gridX - 1, gridY, x % granularity);
                    float rightMultiplier = GetEdgeFalloff(gridX + 1, gridY, granularity - 1 - x % granularity);

                    float multiplier = Mathf.Min(topMultiplier, bottomMultiplier, leftMultiplier, rightMultiplier);

                    float noiseValue = 0.5f + Mathf.PerlinNoise(x * noiseScale, y * noiseScale) / 2;
                    height = noiseValue * multiplier * maxHeight;
                }

                vertices[i] = new Vector3((float)x / granularity, (float)y / granularity, -height);
            }
        }

        int[] triangles = new int[refinedSizeX * refinedSizeY * 6];
        for (int ti = 0, vi = 0, y = 0; y < refinedSizeY; y++, vi++)
        {
            for (int x = 0; x < refinedSizeX; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + refinedSizeX + 1;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 1;
                triangles[ti + 4] = vi + refinedSizeX + 1;
                triangles[ti + 5] = vi + refinedSizeX + 2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        var depositInstance = Instantiate(depositMeshPrefab, new Vector3(deposit.left, deposit.bottom, 0), Quaternion.identity);
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
