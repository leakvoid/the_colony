using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainMeshRenderer : MonoBehaviour
{
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] GameObject waterPrefab;
    [SerializeField] float maxDepth = -1f;

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

    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;
    int vertX;
    int vertY;
    float center = 0.5f;

    struct Vertex
    {
        public Vector3 v;
        public int index;

        public Vertex(Vector3 _v, int _index)
        {
            v = _v;
            index = _index;
        }
    }
    enum Direction
    {
        Left,
        Right,
        Top,
        Bottom
    }
    Dictionary<(int x, int y, Direction d), List<Vertex>> splitSideVertices;

    void Implementation()
    {
        void AddTriangle(int first, int second, int third)
        {
            triangles.Add(first);
            triangles.Add(second);
            triangles.Add(third);
        }

        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        vertX = sizeX + 1;
        vertY = sizeY + 1;
        splitSideVertices = new Dictionary<(int x, int y, Direction d), List<Vertex>>();

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
                    vertices.Add(new Vector3(x, maxDepth, y));
                }
                uvs.Add(new Vector2 (x / (float)vertX, y / (float)vertY));
            }
        }

        int index = 0;

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                bool splitTile = false;
                if(terrainGrid[x, y] == TerrainType.Water)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int dx = x + i;
                            int dy = y + j;
                            if (dx >= 0 && dx < sizeX && dy >= 0 && dy < sizeY &&
                                terrainGrid[dx, dy] != TerrainType.Water)
                            {
                                splitTile = true;
                                break;
                            }
                        }
                    }
                }
                if (splitTile)
                {
                    //SplitWaterTile(x, y);
                }
                else
                {
                    AddTriangle(index, index + vertX + 1, index + 1);
                    AddTriangle(index, index + vertX, index + vertX + 1);
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

    void SplitWaterTile(int tileX, int tileY)
    {
        float x = tileX + UnityEngine.Random.Range(0.3f, 0.7f);
        float y = tileY + UnityEngine.Random.Range(0.3f, 0.7f);
        var vec = new Vector3(x, FindVertexHeight(x, y, (tileX, tileY)), y);
        Vertex center = new Vertex(vec, vertices.Count);

        vertices.Add(vec);
        uvs.Add(new Vector2 (x / (float)vertX, y / (float)vertY));

        int index = tileX + tileY * vertX;
        Vertex bottomLeft = new Vertex(vertices[index], index);
        index = index + 1;
        Vertex bottomRight = new Vertex(vertices[index], index);
        index = tileX + (tileY + 1) * vertX;
        Vertex topLeft = new Vertex(vertices[index], index);
        index = index + 1;
        Vertex topRight = new Vertex(vertices[index], index);

        SplitDirection((tileX - 1, tileY, Direction.Right), (tileX, tileY), Direction.Left, bottomLeft, topLeft);
        SplitDirection((tileX + 1, tileY, Direction.Left), (tileX, tileY), Direction.Right, bottomRight, topRight);
        SplitDirection((tileX, tileY - 1, Direction.Top), (tileX, tileY), Direction.Bottom, bottomLeft, bottomRight);
        SplitDirection((tileX, tileY + 1, Direction.Bottom), (tileX, tileY), Direction.Top, topLeft, topRight);

        void SplitDirection((int, int, Direction) firstKey, (int x, int y) tile, Direction direction, Vertex v1, Vertex v2)
        {
            if (splitSideVertices.ContainsKey(firstKey))
            {
                var sideVertices = splitSideVertices[firstKey];
                if (direction == Direction.Left || direction == Direction.Right)
                    sideVertices.Sort((a, b) => a.v.y.CompareTo(b.v.y));
                else
                    sideVertices.Sort((a, b) => a.v.x.CompareTo(b.v.x));

                for (int i = 0; i < sideVertices.Count - 1; i++)
                    SplitTriangle(sideVertices[i], sideVertices[i + 1], center, tile, direction);
            }
            else
            {
                var secondKey = (tile.x, tile.y, direction);
                splitSideVertices[secondKey] = new List<Vertex>();
                splitSideVertices[secondKey].Add(v1);
                splitSideVertices[secondKey].Add(v2);

                SplitTriangle(v1, v2, center, tile, direction);
            }
        }
    }

    float FindVertexHeight(float x, float y, (int x, int y) tile)
    {
        var coastDistance = UnityEngine.Random.Range(0, 0.2f);
        float minDepth = maxDepth;

        void FindDepth(bool condition, float diff)
        {
            if (condition)
            {
                float depth = Mathf.InverseLerp(coastDistance, center, diff) * maxDepth;
                if (depth > minDepth)
                    minDepth = depth;
            }
        }

        FindDepth(tile.x > 0 && terrainGrid[tile.x - 1, tile.y] != TerrainType.Water,
            x - tile.x);
        FindDepth(tile.x + 1 < sizeX && terrainGrid[tile.x + 1, tile.y] != TerrainType.Water,
            tile.x + 1 - x);
        FindDepth(tile.y > 0 && terrainGrid[tile.x, tile.y - 1] != TerrainType.Water,
            y - tile.y);
        FindDepth(tile.y + 1 < sizeY && terrainGrid[tile.x, tile.y + 1] != TerrainType.Water,
            tile.y + 1 - y);

        int index = tile.x + tile.y * vertX;
        Vector3 bottomLeft = vertices[index];
        index += 1;
        Vector3 bottomRight = vertices[index];
        index = tile.x + (tile.y + 1) * vertX;
        Vector3 topLeft = vertices[index];
        index += 1;
        Vector3 topRight = vertices[index];
        Vector2 pos = new Vector2(x, y);

        FindDepth(bottomLeft.y == 0,
            Vector2.Distance(new Vector2(bottomLeft.x, bottomLeft.z), pos));
        FindDepth(bottomRight.y == 0,
            Vector2.Distance(new Vector2(bottomRight.x, bottomRight.z), pos));
        FindDepth(topLeft.y == 0,
            Vector2.Distance(new Vector2(topLeft.x, topLeft.z), pos));
        FindDepth(topRight.y == 0,
            Vector2.Distance(new Vector2(topRight.x, topRight.z), pos));

        return minDepth;
    }

    void SplitTriangle(Vertex a, Vertex b, Vertex c, (int x, int y) tile, Direction direction)
    {
        triangles.Add(a.index);
        triangles.Add(b.index);
        triangles.Add(c.index);
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