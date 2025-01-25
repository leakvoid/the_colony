using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class TerrainMeshRenderer : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] GameObject waterPrefab;

    AbstractMapGenerator amg;

    TerrainType[,] terrainGrid;
    int sizeX;
    int sizeY;

    Mesh mesh;
    float[,] heightMap;

    void Awake()
    {
        amg = FindObjectOfType<AbstractMapGenerator>();
    }

    public void Initialize()
    {
        terrainGrid = amg.GetTerrainGrid();
        sizeX = terrainGrid.GetLength(0);
        sizeY = terrainGrid.GetLength(1);

        //CreateHeightMap();
        //CreateTerrainMesh();
        Implementation();
        AddMinimapIcons();
        SpawnTrees();
        //CreateWater();

        meshFilter.sharedMesh = mesh;
    }

    /*void CreateHeightMap()
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
    }*/



    void Implementation()
    {
        void AddTriangle(int first, int second, int third, List<int> triangles)
        {
            triangles.Add(first);
            triangles.Add(second);
            triangles.Add(third);
        }

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        int vertX = sizeX + 1;
        int vertY = sizeY + 1;
        for (int y = 0; y < vertY; y++)
        {
            for (int x = 0; x < vertX; x++)
            {
                if ((x > 0 && y < sizeY && terrainGrid[x - 1, y] != TerrainType.Water) ||
                    (x < sizeX && y > 0 && terrainGrid[x, y - 1] != TerrainType.Water) ||
                    (x > 0 && y > 0 && terrainGrid[x - 1, y - 1] != TerrainType.Water) ||
                    (x < sizeX && y < sizeY && terrainGrid[x, y] != TerrainType.Water))
                {
                    vertices.Add(new Vector3(x, 0f, y));
                }
                else
                {
                    vertices.Add(new Vector3(x, -1f, y));
                }
                uvs.Add(new Vector2 (x / (float)vertX, y / (float)vertY));
            }
        }

        int foo = 0, index = 0;

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if(terrainGrid[x, y] == TerrainType.Water &&
                    ((x > 0 && terrainGrid[x - 1, y] != TerrainType.Water) ||
                    (x + 1 < sizeX && terrainGrid[x + 1, y] != TerrainType.Water) ||
                    (y > 0 && terrainGrid[x, y - 1] != TerrainType.Water) ||
                    (y + 1 < sizeY && terrainGrid[x, y + 1] != TerrainType.Water)))
                {
                    foo++;
                }
                else
                {
                    AddTriangle(index, index + vertX + 1, index + 1, triangles);
                    AddTriangle(index, index + vertX, index + vertX + 1, triangles);
                }
                index++;
            }
            index++;
        }

        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
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

    void CreateWater()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (terrainGrid[x, y] == TerrainType.Water)
                {
                    var water = Instantiate(waterPrefab,
                        new Vector3(x + 0.5f, -0.4f, y + 0.5f),
                        Quaternion.Euler(new Vector3(90,0,0)));
                }
            }
        }
    }
}