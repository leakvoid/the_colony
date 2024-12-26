using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMeshRenderer : MonoBehaviour
{
    AbstractMapGenerator amg;
    TerrainType[,] grid;
    int gridX;
    int gridY;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    void Awake()
    {
        amg = FindObjectOfType<AbstractMapGenerator>();
    }

    void Initialize()
    {
        grid = amg.GetTerrainGrid();
        gridX = grid.GetLength(0);
        gridY = grid.GetLength(1);

        CreateMesh();
    }

    void CreateMesh()
    {
        mesh = new Mesh();
        vertices = new Vector3[(gridX + 1) * (gridY + 1)];
        triangles = new int[gridX * gridY * 6];

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}
