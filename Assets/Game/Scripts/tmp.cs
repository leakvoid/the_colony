using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShorelineMesh : MonoBehaviour
{
    public int gridSizeX = 100;
    public int gridSizeY = 100;
    public bool[,] groundTiles; // true for ground, false for water

    private MeshFilter meshFilter;
    private Mesh mesh;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) 
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        GenerateGrid();
        CreateMesh();
    }

    void GenerateGrid()
    {
        groundTiles = new bool[gridSizeX, gridSizeY];
        // Populate groundTiles based on your logic or input
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                groundTiles[x, y] = Random.value > 0.5f; // Example: random distribution
            }
        }
    }

    void CreateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                // Add vertices for each corner of the tile
                vertices.Add(new Vector3(x, groundTiles[x, y] ? 0 : -1, y)); // bottom left
                vertices.Add(new Vector3(x + 1, groundTiles[x + 1 < gridSizeX ? x + 1 : x, y] ? 0 : -1, y)); // bottom right
                //vertices.Add(new Vector3(x, groundTiles[x, y + 1 < gridSizeY ? x : y, y + 1 < gridSizeY ? y + 1 : y] ? 0 : -1, y + 1)); // top left
                vertices.Add(new Vector3(x + 1, groundTiles[x + 1 < gridSizeX ? x + 1 : x, y + 1 < gridSizeY ? y + 1 : y] ? 0 : -1, y + 1)); // top right

                if (!groundTiles[x, y]) // If it's water
                {
                    if (ShouldSplitWaterTile(x, y))
                    {
                        SplitWaterTile(x, y, vertices, ref triangles);
                    }
                    else
                    {
                        AddStandardWaterTile(vertices, ref triangles);
                    }
                }
                else
                {
                    AddStandardGroundTile(vertices, ref triangles);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    bool ShouldSplitWaterTile(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip itself

                int checkX = x + i;
                int checkY = y + j;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (groundTiles[checkX, checkY])
                    {
                        return true; // Adjacent to ground, should split
                    }
                }
            }
        }
        return false; // No adjacent ground tiles
    }

    void SplitWaterTile(int x, int y, List<Vector3> vertices, ref List<int> triangles)
    {
        int baseIndex = vertices.Count - 4;
        List<Vector3> newVertices = new List<Vector3>(vertices.GetRange(baseIndex, 4)); // Copy the original four vertices

        // Check which edges border ground tiles and split accordingly
        for (int i = 0; i < 4; i++) // Loop through each edge of the square
        {
            int checkX = x + (i % 2);
            int checkY = y + (i / 2);
            
            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY && groundTiles[checkX, checkY])
            {
                // If the edge borders ground, we'll split this edge into two segments
                Vector3 start = vertices[baseIndex + i];
                Vector3 end = vertices[baseIndex + ((i + 1) % 4)]; // Next vertex in clockwise direction
                
                // Add a new vertex at the midpoint but at ground level (assuming ground is at 0 height)
                Vector3 midpoint = (start + end) * 0.5f;
                midpoint.y = 0; // Setting the height to ground level
                newVertices.Add(midpoint);
                
                // Replace the original edge with two new edges
                int newVertexIndex = newVertices.Count - 1; // Index of the new midpoint vertex
                
                // Reorder vertices for correct triangulation
                if (i % 2 == 0) // Vertical edges (top or bottom)
                {
                    // Adjust indices to maintain correct triangle orientation
                    int prevIndex = (i == 0) ? 3 : i - 1;
                    newVertices[prevIndex] = midpoint;
                    newVertices.Add(vertices[baseIndex + prevIndex]);
                }
                else // Horizontal edges (left or right)
                {
                    int nextIndex = (i == 1) ? 0 : i + 1;
                    newVertices[nextIndex] = midpoint;
                    newVertices.Add(vertices[baseIndex + nextIndex]);
                }
            }
        }

        // Now, triangulate based on new vertices configuration
        for (int i = 0; i < newVertices.Count - 3; i += 2) // Step by two as we've added new vertices
        {
            // Assuming a clockwise triangle arrangement for the front face
            triangles.Add(baseIndex + i); // Original or adjusted vertex
            triangles.Add(baseIndex + i + 1); // New midpoint or adjusted vertex
            triangles.Add(baseIndex + i + 2); // Next original or adjusted vertex

            if (i + 3 < newVertices.Count) // Ensure we don't go out of bounds with the next triangle
            {
                triangles.Add(baseIndex + i + 1); // The new midpoint
                triangles.Add(baseIndex + i + 3); // The next original or adjusted vertex
                triangles.Add(baseIndex + i + 2); // The vertex after the new midpoint
            }
        }

        // Replace the original vertices with the new set
        vertices.RemoveRange(baseIndex, 4);
        vertices.InsertRange(baseIndex, newVertices);
    }

    void AddStandardWaterTile(List<Vector3> vertices, ref List<int> triangles)
    {
        int baseIndex = vertices.Count - 4;
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 3);
        triangles.Add(baseIndex + 2);
    }

    void AddStandardGroundTile(List<Vector3> vertices, ref List<int> triangles)
    {
        int baseIndex = vertices.Count - 4;
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 3);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);
    }
}
