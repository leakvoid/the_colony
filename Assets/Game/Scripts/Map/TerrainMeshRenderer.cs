using System;
using System.Collections.Generic;
using UnityEngine;
using DelaunayTriangulation;

public class TerrainMeshRenderer : MonoBehaviour
{
    [SerializeField] GameObject terrainPrefab;
    [SerializeField] GameObject outsideTerrainPrefab;
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

        CreateTerrainMesh();
        CreateSideTerrain();
        AddMinimapIcons();
        SpawnTrees();
        CreateWater();
    }

    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;
    int vertX;
    int vertY;

    struct Vertex3d
    {
        public Vector3 v;
        public int index;

        public Vertex3d(Vector3 _v, int _index)
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
    Dictionary<(int x, int y, Direction d), List<Vertex3d>> tileSidePoints;

    void AddTriangle(int first, int second, int third)
    {
        triangles.Add(first);
        triangles.Add(second);
        triangles.Add(third);
    }

    void CreateTerrainMesh()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        vertX = sizeX + 1;
        vertY = sizeY + 1;
        tileSidePoints = new Dictionary<(int x, int y, Direction d), List<Vertex3d>>();

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
                    //DelaunayTriangulation((x, y));
                    MakeShoreline((x, y));
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
        var terrainMesh = Instantiate(terrainPrefab);
        terrainMesh.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void CreateSideTerrain()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                var nVertices = new Vector3[4];
                var nUvs = new Vector2[4];
                var nTriangles = new int[6];

                int index = 0;
                for (int y = 0; y <= sizeY; y += sizeY)
                {
                    for (int x = 0; x <= sizeX; x += sizeX)
                    {
                        nVertices[index] = new Vector3(x, 0f, y);
                        nUvs[index] = new Vector2 (x / (float)vertX, y / (float)vertY);

                        index++;
                    }
                }
                nTriangles[0] = 0;
                nTriangles[1] = 3;
                nTriangles[2] = 1;
                nTriangles[3] = 0;
                nTriangles[4] = 2;
                nTriangles[5] = 3;

                mesh = new Mesh();
                mesh.vertices = nVertices;
                mesh.triangles = nTriangles;
                mesh.uv = nUvs;
                mesh.RecalculateNormals();
                var terrainMesh = Instantiate(outsideTerrainPrefab, new Vector3(i * sizeX, 0, j * sizeY), Quaternion.identity);
                terrainMesh.GetComponent<MeshFilter>().sharedMesh = mesh;
            }
        }
    }

    /* triangulation */
    float splitFactor = 0.25f;
    float noiseScale = 1.5f;
    float shoreDistance = 0.4f;

    void MakeShoreline((int x, int y) tile)
    {
        int index = tile.x + tile.y * vertX;
        Vertex3d bottomLeft = new Vertex3d(vertices[index], index);
        index = index + 1;
        Vertex3d bottomRight = new Vertex3d(vertices[index], index);
        index = tile.x + (tile.y + 1) * vertX;
        Vertex3d topLeft = new Vertex3d(vertices[index], index);
        index = index + 1;
        Vertex3d topRight = new Vertex3d(vertices[index], index);

        List<Vertex3d> sidePoints = new List<Vertex3d>();
        sidePoints.Add(bottomLeft);
        sidePoints.Add(bottomRight);
        sidePoints.Add(topLeft);
        sidePoints.Add(topRight);

        var keys = new List<(int, int, Direction)>() {
            (tile.x - 1, tile.y, Direction.Right),
            (tile.x + 1, tile.y, Direction.Left),
            (tile.x, tile.y - 1, Direction.Top),
            (tile.x, tile.y + 1, Direction.Bottom)
        };
        foreach (var key in keys)
        {
            if (tileSidePoints.ContainsKey(key))
            {
                sidePoints.AddRange(tileSidePoints[key]);
            }
        }
        tileSidePoints[(tile.x, tile.y, Direction.Left)] = new List<Vertex3d>();
        tileSidePoints[(tile.x, tile.y, Direction.Right)] = new List<Vertex3d>();
        tileSidePoints[(tile.x, tile.y, Direction.Top)] = new List<Vertex3d>();
        tileSidePoints[(tile.x, tile.y, Direction.Bottom)] = new List<Vertex3d>();

        var quadCenter = FindOrCreateVertex(0.5f, 0.5f);
        var leftMid = FindOrCreateVertex(0, 0.5f);
        var rightMid = FindOrCreateVertex(1, 0.5f);
        var bottomMid = FindOrCreateVertex(0.5f, 0);
        var topMid = FindOrCreateVertex(0.5f, 1);
        if (bottomLeft.v.y == 0)
        {
            SplitQuad(0, 0);
        }
        else
        {
            AddTriangle(bottomLeft.index, quadCenter.index, bottomMid.index);
            AddTriangle(bottomLeft.index, leftMid.index, quadCenter.index);
        }
        if (bottomRight.v.y == 0)
        {
            SplitQuad(1, 0);
        }
        else
        {
            AddTriangle(bottomMid.index, rightMid.index, bottomRight.index);
            AddTriangle(bottomMid.index, quadCenter.index, rightMid.index);
        }
        if (topLeft.v.y == 0)
        {
            SplitQuad(0, 1);
        }
        else
        {
            AddTriangle(leftMid.index, topMid.index, quadCenter.index);
            AddTriangle(leftMid.index, topLeft.index, topMid.index);
        }
        if (topRight.v.y == 0)
        {
            SplitQuad(1, 1);
        }
        else
        {
            AddTriangle(quadCenter.index, topRight.index, rightMid.index);
            AddTriangle(quadCenter.index, topMid.index, topRight.index);
        }
        

        void SplitQuad(int quadX, int quadY)
        {
            int shiftX = quadX * 2;
            int shiftY = quadY * 2;
            for (int i = shiftX; i < 2 + shiftX; i++)
            {
                for (int j = shiftY; j < 2 + shiftY; j++)
                {
                    Vertex3d bl = FindOrCreateVertex(i * splitFactor, j * splitFactor);
                    Vertex3d br = FindOrCreateVertex((i + 1) * splitFactor, j * splitFactor);
                    Vertex3d tl = FindOrCreateVertex(i * splitFactor, (j + 1) * splitFactor);
                    Vertex3d tr = FindOrCreateVertex((i + 1) * splitFactor, (j + 1) * splitFactor);

                    float x = bl.v.x + UnityEngine.Random.Range(0.3f, 0.7f) * splitFactor;
                    float y = bl.v.z + UnityEngine.Random.Range(0.3f, 0.7f) * splitFactor;
                    var p = new Vector3(x, FindVertexHeight(x, y, tile), y);
                    Vertex3d center = new Vertex3d(p, vertices.Count);
                    vertices.Add(p);
                    uvs.Add(new Vector2 (x / (float)vertX, y / (float)vertY));

                    DrawTriangle(bl, tl, center);
                    DrawTriangle(br, tr, center);
                    DrawTriangle(bl, br, center);
                    DrawTriangle(tl, tr, center);
                }
            }
        }

        Vertex3d FindOrCreateVertex(float shiftX, float shiftY)
        {
            float x = tile.x + shiftX;
            float y = tile.y + shiftY;
            foreach(var p in sidePoints)
            {
                if (p.v.x == x && p.v.y == y)
                    return p;
            }

            Vector3 point = new Vector3(x, FindVertexHeight(x, y, tile), y);

            Vertex3d v = new Vertex3d(point, vertices.Count);
            vertices.Add(point);
            uvs.Add(new Vector2 (point.x / (float)vertX, point.z / (float)vertY));

            if (x == (float)tile.x)
            {
                tileSidePoints[(tile.x, tile.y, Direction.Left)].Add(v);
                sidePoints.Add(v);
            }
            else if (x == (float)tile.x + 1)
            {
                tileSidePoints[(tile.x, tile.y, Direction.Right)].Add(v);
                sidePoints.Add(v);
            }
            else if (y == (float)tile.y)
            {
                tileSidePoints[(tile.x, tile.y, Direction.Bottom)].Add(v);
                sidePoints.Add(v);
            }
            else if (y == (float)tile.y + 1)
            {
                tileSidePoints[(tile.x, tile.y, Direction.Top)].Add(v);
                sidePoints.Add(v);
            }

            return v;
        }
    }

    void DrawTriangle(Vertex3d a, Vertex3d b, Vertex3d c)
    {
        if (Vector3.Cross(b.v - a.v, c.v - a.v).y > 0)
        {
            triangles.Add(a.index);
            triangles.Add(b.index);
            triangles.Add(c.index);
        }
        else
        {
            triangles.Add(a.index);
            triangles.Add(c.index);
            triangles.Add(b.index);
        }
    }

    float FindVertexHeight(float x, float y, (int x, int y) tile)
    {
        var coastDistance = Mathf.PerlinNoise(x * noiseScale, y * noiseScale) * shoreDistance;
        float minDepth = maxDepth;

        void FindDepth(bool condition, float diff)
        {
            if (condition)
            {
                float depth = Mathf.InverseLerp(coastDistance, 0.5f, diff) * maxDepth;
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
            Vector2.Distance(v3tov2(bottomLeft), pos));
        FindDepth(bottomRight.y == 0,
            Vector2.Distance(v3tov2(bottomRight), pos));
        FindDepth(topLeft.y == 0,
            Vector2.Distance(v3tov2(topLeft), pos));
        FindDepth(topRight.y == 0,
            Vector2.Distance(v3tov2(topRight), pos));

        return minDepth;
    }

    /* Delaunay version */

    float RandomStep()
    {
        return UnityEngine.Random.Range(0.1f, 0.2f);
    }

    void DelaunayTriangulation((int x, int y) tile)
    {
        int index = tile.x + tile.y * vertX;
        Vertex3d bottomLeft = new Vertex3d(vertices[index], index);
        index = index + 1;
        Vertex3d bottomRight = new Vertex3d(vertices[index], index);
        index = tile.x + (tile.y + 1) * vertX;
        Vertex3d topLeft = new Vertex3d(vertices[index], index);
        index = index + 1;
        Vertex3d topRight = new Vertex3d(vertices[index], index);

        List<Vertex3d> allTilePoints = new List<Vertex3d>();
        allTilePoints.Add(bottomLeft);
        allTilePoints.Add(bottomRight);
        allTilePoints.Add(topLeft);
        allTilePoints.Add(topRight);

        var key = (tile.x - 1, tile.y, Direction.Right);
        if (tileSidePoints.ContainsKey(key))
        {
            allTilePoints.AddRange(tileSidePoints[key]);
        }
        else
        {
            key = (tile.x, tile.y, Direction.Left);
            tileSidePoints[key] = new List<Vertex3d>();

            var point = bottomLeft.v;
            point.z += RandomStep();
            point.y = FindVertexHeight(point.x, point.z, tile);
            while(point.z < topLeft.v.z)
            {
                Vertex3d v = new Vertex3d(point, vertices.Count);

                vertices.Add(point);
                uvs.Add(new Vector2 (point.x / (float)vertX, point.z / (float)vertY));

                allTilePoints.Add(v);
                tileSidePoints[key].Add(v);

                point.z += RandomStep();
                point.y = FindVertexHeight(point.x, point.z, tile);
            }
        }

        key = (tile.x + 1, tile.y, Direction.Left);
        if (tileSidePoints.ContainsKey(key))
        {
            allTilePoints.AddRange(tileSidePoints[key]);
        }
        else
        {
            key = (tile.x, tile.y, Direction.Right);
            tileSidePoints[key] = new List<Vertex3d>();

            var point = bottomRight.v;
            point.z += RandomStep();
            point.y = FindVertexHeight(point.x, point.z, tile);
            while(point.z < topRight.v.z)
            {
                Vertex3d v = new Vertex3d(point, vertices.Count);

                vertices.Add(point);
                uvs.Add(new Vector2 (point.x / (float)vertX, point.z / (float)vertY));

                allTilePoints.Add(v);
                tileSidePoints[key].Add(v);

                point.z += RandomStep();
                point.y = FindVertexHeight(point.x, point.z, tile);
            }
        }

        key = (tile.x, tile.y - 1, Direction.Top);
        if (tileSidePoints.ContainsKey(key))
        {
            allTilePoints.AddRange(tileSidePoints[key]);
        }
        else
        {
            key = (tile.x, tile.y, Direction.Bottom);
            tileSidePoints[key] = new List<Vertex3d>();

            var point = bottomLeft.v;
            point.x += RandomStep();
            point.y = FindVertexHeight(point.x, point.z, tile);
            while(point.x < bottomRight.v.x)
            {
                Vertex3d v = new Vertex3d(point, vertices.Count);

                vertices.Add(point);
                uvs.Add(new Vector2 (point.x / (float)vertX, point.z / (float)vertY));

                allTilePoints.Add(v);
                tileSidePoints[key].Add(v);

                point.x += RandomStep();
                point.y = FindVertexHeight(point.x, point.z, tile);
            }
        }

        key = (tile.x, tile.y + 1, Direction.Bottom);
        if (tileSidePoints.ContainsKey(key))
        {
            allTilePoints.AddRange(tileSidePoints[key]);
        }
        else
        {
            key = (tile.x, tile.y, Direction.Top);
            tileSidePoints[key] = new List<Vertex3d>();

            var point = topLeft.v;
            point.x += RandomStep();
            point.y = FindVertexHeight(point.x, point.z, tile);
            while(point.x < topRight.v.x)
            {
                Vertex3d v = new Vertex3d(point, vertices.Count);

                vertices.Add(point);
                uvs.Add(new Vector2 (point.x / (float)vertX, point.z / (float)vertY));

                allTilePoints.Add(v);
                tileSidePoints[key].Add(v);

                point.x += RandomStep();
                point.y = FindVertexHeight(point.x, point.z, tile);
            }
        }

        float pY = tile.y + RandomStep();
        while(pY + 0.05f < tile.y + 1)
        {
            float pX = tile.x + RandomStep();
            while(pX < tile.x + 1)
            {
                var shiftedY = pY + 0;//UnityEngine.Random.Range(-0.05f, 0.05f);
                var point = new Vector3(pX, FindVertexHeight(pX, shiftedY, tile), shiftedY);

                Vertex3d v = new Vertex3d(point, vertices.Count);

                vertices.Add(point);
                uvs.Add(new Vector2 (point.x / (float)vertX, point.z / (float)vertY));

                allTilePoints.Add(v);

                pX += RandomStep();
            }
            pY += RandomStep();
        }

        List<Vertex> points = new List<Vertex>();
        foreach(var point in allTilePoints)
        {
            points.Add(new Vertex(new Vector2(point.v.x, point.v.z), point.index));
        }
        var triangulator = new Triangulation(points);
        var tr = triangulator.triangles;
        foreach(var triangle in tr)
        {
            var a = new Vertex3d(vertices[triangle.vertex0.index], triangle.vertex0.index);
            var b = new Vertex3d(vertices[triangle.vertex1.index], triangle.vertex1.index);
            var c = new Vertex3d(vertices[triangle.vertex2.index], triangle.vertex2.index);

            if (Vector3.Cross(b.v - a.v, c.v - a.v).y > 0)
            {   
                triangles.Add(a.index);
                triangles.Add(b.index);
                triangles.Add(c.index);
            }
            else
            {
                triangles.Add(a.index);
                triangles.Add(c.index);
                triangles.Add(b.index);
            }
        }
    }

    /* Incorrect implementation */

    void SplitWaterTile(int tileX, int tileY)
    {
        float x = tileX + 0.5f;//UnityEngine.Random.Range(0.3f, 0.7f);
        float y = tileY + 0.5f;//UnityEngine.Random.Range(0.3f, 0.7f);
        var vec = new Vector3(x, FindVertexHeight(x, y, (tileX, tileY)), y);
        Vertex3d center = new Vertex3d(vec, vertices.Count);

        vertices.Add(vec);
        uvs.Add(new Vector2 (x / (float)vertX, y / (float)vertY));

        int index = tileX + tileY * vertX;
        Vertex3d bottomLeft = new Vertex3d(vertices[index], index);
        index = index + 1;
        Vertex3d bottomRight = new Vertex3d(vertices[index], index);
        index = tileX + (tileY + 1) * vertX;
        Vertex3d topLeft = new Vertex3d(vertices[index], index);
        index = index + 1;
        Vertex3d topRight = new Vertex3d(vertices[index], index);

        SplitDirection((tileX - 1, tileY, Direction.Right), (tileX, tileY), Direction.Left, bottomLeft, topLeft);
        SplitDirection((tileX + 1, tileY, Direction.Left), (tileX, tileY), Direction.Right, bottomRight, topRight);
        SplitDirection((tileX, tileY - 1, Direction.Top), (tileX, tileY), Direction.Bottom, bottomLeft, bottomRight);
        SplitDirection((tileX, tileY + 1, Direction.Bottom), (tileX, tileY), Direction.Top, topLeft, topRight);

        void SplitDirection((int, int, Direction) firstKey, (int x, int y) tile, Direction direction, Vertex3d v1, Vertex3d v2)
        {
            if (tileSidePoints.ContainsKey(firstKey))
            {
                var sideVertices = tileSidePoints[firstKey];
                if (direction == Direction.Left || direction == Direction.Right)
                    sideVertices.Sort((a, b) => a.v.z.CompareTo(b.v.z));
                else
                    sideVertices.Sort((a, b) => a.v.x.CompareTo(b.v.x));

                for (int i = 0; i < sideVertices.Count - 1; i++)
                {
                    SplitTriangle(sideVertices[i], sideVertices[i + 1], center, tile, direction);
                }
            }
            else
            {
                var secondKey = (tile.x, tile.y, direction);
                tileSidePoints[secondKey] = new List<Vertex3d>();
                tileSidePoints[secondKey].Add(v1);
                tileSidePoints[secondKey].Add(v2);

                SplitTriangle(v1, v2, center, tile, direction);
            }
        }
    }

    float PickSide(float a, float b, float c)
    {
        if (a < 0.5f && b < 0.5f && c < 0.5f)
            return 0;
        if (a > b && a > c)
            return a;
        if (b > a && b > c)
            return b;
        return c;
    }

    Vector2 v3tov2(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    void SplitTriangle(Vertex3d a, Vertex3d b, Vertex3d c, (int x, int y) tile, Direction direction)
    {
        float ab = Vector2.Distance(v3tov2(a.v), v3tov2(b.v));
        float ac = Vector2.Distance(v3tov2(a.v), v3tov2(c.v));
        float bc = Vector2.Distance(v3tov2(b.v), v3tov2(c.v));

        float side = PickSide(ab, ac, bc);
        if (side == 0)
        {
            if (Vector3.Cross(b.v - a.v, c.v - a.v).y > 0)
            {   
                triangles.Add(a.index);
                triangles.Add(b.index);
                triangles.Add(c.index);
            }
            else
            {
                triangles.Add(a.index);
                triangles.Add(c.index);
                triangles.Add(b.index);
            }
            return;
        }

        float split = UnityEngine.Random.Range(0.3f, 0.7f);
        Vector3 p;
        Vertex3d first, second, third;
        if (side == ab)
        {
            p = a.v + (b.v - a.v) * split;
            first = a;
            second = b;
            third = c;
        }
        else if (side == ac)
        {
            p = a.v + (c.v - a.v) * split;
            first = c;
            second = a;
            third = b;
        }
        else
        {
            p = b.v + (c.v - b.v) * split;
            first = b;
            second = c;
            third = a;
        }

        p.y = FindVertexHeight(p.x, p.z, tile);
        Vertex3d v = new Vertex3d(p, vertices.Count);

        vertices.Add(p);
        uvs.Add(new Vector2 (p.x / (float)vertX, p.z / (float)vertY));

        if ((direction == Direction.Left && p.x == (float)tile.x) ||
            (direction == Direction.Right && p.x == (float)tile.x + 1) ||
            (direction == Direction.Bottom && p.z == (float)tile.y) ||
            (direction == Direction.Top && p.z == (float)tile.y + 1))
            tileSidePoints[(tile.x, tile.y, direction)].Add(v);
        
        SplitTriangle(first, v, third, tile, direction);
        SplitTriangle(second, v, third, tile, direction);
    }

    /* other methods */

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
                        new Vector3(x + 0.5f, -0.2f, y + 0.5f),
                        Quaternion.Euler(new Vector3(90,0,0)));
                }
            }
        }
    }
}