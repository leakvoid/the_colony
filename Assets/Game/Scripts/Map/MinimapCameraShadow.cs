using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraShadow : MonoBehaviour
{
    Globals globals;

    [SerializeField] GameObject minimapCameraMesh;

    Vector3 bottomLeft;
    Vector3 bottomRight;
    Vector3 topLeft;
    Vector3 topRight;
    Vector3[] vertices;
    int[] triangles;
    
    Mesh mesh;
    MeshFilter meshFilter;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
    }

    public void Initialize()
    {
        bottomLeft = new Vector3(0f,0f);
        bottomRight = new Vector3(1f,0f);
        topLeft = new Vector3(0f,1f);
        topRight = new Vector3(1f,1f);

        vertices = new Vector3[4];
        triangles = new int[6];
        mesh = new Mesh();
        meshFilter = minimapCameraMesh.GetComponent<MeshFilter>();

        var grid = FindObjectOfType<AbstractMapGenerator>().GetTerrainGrid();
        var sizeX = grid.GetLength(0);
        var sizeY = grid.GetLength(1);
        

        Redraw();
    }

    public Vector3[] GetEdges()
    {
        return vertices;
    }

    public void Redraw()
    {
        Vector3 GetScreenEdge(Vector3 corner)
        {
            Ray ray = Camera.main.ViewportPointToRay(corner);
            globals.GroundPlane.Raycast(ray, out float entry);
            return ray.GetPoint(entry);
        }

        var bottomLeftVector = GetScreenEdge(bottomLeft);
        var bottomRightVector = GetScreenEdge(bottomRight);
        var topLeftVector = GetScreenEdge(topLeft);
        var topRightVector = GetScreenEdge(topRight);
        bottomLeftVector.y = 5f;
        bottomRightVector.y = 5f;
        topLeftVector.y = 5f;
        topRightVector.y = 5f;

        vertices[0] = bottomLeftVector;
        vertices[1] = bottomRightVector;
        vertices[2] = topLeftVector;
        vertices[3] = topRightVector;
        
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
