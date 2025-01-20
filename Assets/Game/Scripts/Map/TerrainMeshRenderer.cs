using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class TerrainMeshRenderer : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;

    Globals globals;
    AbstractMapGenerator amg;

    TerrainType[,] terrainGrid;
    int sizeX;
    int sizeY;

    Mesh mesh;
    float[,] heightMap;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        amg = FindObjectOfType<AbstractMapGenerator>();
    }

    public void Initialize()
    {
        terrainGrid = amg.GetTerrainGrid();
        sizeX = terrainGrid.GetLength(0);
        sizeY = terrainGrid.GetLength(1);

        CreateHeightMap();
        CreateTerrainMesh();
        AddMinimapIcons();
        SpawnTrees();

        meshFilter.sharedMesh = mesh;
    }

    void CreateHeightMap()
    {
        heightMap = new float[sizeX + 1, sizeY + 1];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (terrainGrid[i,j] == TerrainType.Water)
                {
                    heightMap[i,j] = -1f;
                    heightMap[i + 1,j] = -1f;
                    heightMap[i,j + 1] = -1f;
                    heightMap[i + 1,j + 1] = -1f;
                }
            }
        }
    }

    int triangleIndex;
    int[] triangles;

    void CreateTerrainMesh()
    {
        void AddTriangle(int first, int second, int third)
        {
            triangles[triangleIndex] = first;
            triangles[triangleIndex + 1] = second;
            triangles[triangleIndex + 2] = third;
            triangleIndex += 3;
        }

        int vertexCountX = heightMap.GetLength(0);
        int vertexCountY = heightMap.GetLength(1);
        
        var vertices = new Vector3[vertexCountX * vertexCountY];
        var uvs = new Vector2[vertexCountX * vertexCountY];
        triangles = new int[(vertexCountX - 1) * (vertexCountY - 1) * 6];

        int vertexIndex = 0;
        triangleIndex = 0;
        for (int y = 0; y < vertexCountY; y++)
        {
            for (int x = 0; x < vertexCountX; x++)
            {
                vertices[vertexIndex] = Globals.NewVector(x, y, heightMap[x, y]);
                uvs[vertexIndex] = new Vector2 (x / (float)vertexCountX, y / (float)vertexCountY);

                if (x < vertexCountX - 1 && y < vertexCountY - 1)
                {
                    AddTriangle(vertexIndex, vertexIndex + vertexCountX + 1, vertexIndex + 1);
                    AddTriangle(vertexIndex, vertexIndex + vertexCountX, vertexIndex + vertexCountX + 1);
                }

                vertexIndex++;
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    [SerializeField] GameObject forestIconPrefab;
    [SerializeField] GameObject waterIconPrefab;
    [SerializeField] GameObject ironIconPrefab;
    [SerializeField] GameObject saltIconPrefab;
    [SerializeField] GameObject stoneIconPrefab;

    GameObject mip;

    void AddMinimapIcons()
    {
        mip = new GameObject("Minimap Icons");

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                switch (terrainGrid[i, j])
                {
                    case TerrainType.Forest:
                        Instantiate(forestIconPrefab,
                            Globals.GridToGlobalCoordinates((i, j), forestIconPrefab),
                            Quaternion.Euler(90, 0, 0)).transform.parent = mip.transform;
                        break;
                    case TerrainType.Water:
                        Instantiate(waterIconPrefab,
                            Globals.GridToGlobalCoordinates((i, j), waterIconPrefab),
                            Quaternion.Euler(90, 0, 0)).transform.parent = mip.transform;
                        break;
                    case TerrainType.IronDeposit:
                        Instantiate(ironIconPrefab,
                            Globals.GridToGlobalCoordinates((i, j), ironIconPrefab),
                            Quaternion.Euler(90, 0, 0)).transform.parent = mip.transform;
                        break;
                    case TerrainType.SaltDeposit:
                        Instantiate(saltIconPrefab,
                            Globals.GridToGlobalCoordinates((i, j), saltIconPrefab),
                            Quaternion.Euler(90, 0, 0)).transform.parent = mip.transform;
                        break;
                    case TerrainType.StoneDeposit:
                        Instantiate(stoneIconPrefab,
                            Globals.GridToGlobalCoordinates((i, j), stoneIconPrefab),
                            Quaternion.Euler(90, 0, 0)).transform.parent = mip.transform;
                        break;
                }
            }
        }
    }

    [SerializeField] GameObject treePrefab;

    void SpawnTrees()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (terrainGrid[x, y] == TerrainType.Forest)
                {
                    Instantiate(treePrefab, Globals.GridToGlobalCoordinates((x, y), treePrefab, true), Quaternion.identity);
                }
            }
        }
    }
}