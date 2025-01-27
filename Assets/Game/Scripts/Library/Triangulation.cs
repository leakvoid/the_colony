using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DelaunayTriangulation
{
    public class Edge
    {
        private float m_Length;
        public float length
        {
            get
            {
                return m_Length;
            }
        }

        private Vertex m_Point0;
        public Vertex point0
        {
            get
            {
                return m_Point0;
            }
        }

        private Vertex m_Point1;
        public Vertex point1
        {
            get
            {
                return m_Point1;
            }
        }

        public Edge(Vertex point0, Vertex point1)
        {
            m_Point0 = point0;
            m_Point1 = point1;

            Vector2 edgeVector = m_Point1.position - m_Point0.position;
            m_Length = edgeVector.magnitude;
        }

        public float Length()
        {
            return m_Length;
        }

        public override bool Equals(object obj)
        {
            Edge other = obj as Edge;

            if (other != null)
            {
                // Check if the two first points overlap
                bool isSame = other.point0.Equals(point0) && other.point1.Equals(point1);

                // Check if the points overlap in cross
                isSame |= other.point1.Equals(point0) && other.point0.Equals(point1);

                return isSame;
            }
            
            return false;
        }

        public override int GetHashCode()
        {
            return m_Point0.GetHashCode() + 31 * m_Point1.GetHashCode();
        }
    }

    public class Triangle
    {
        private Vertex m_Vertex0, m_Vertex1, m_Vertex2;
        private Edge m_Edge0, m_Edge1, m_Edge2;

        private Vector2 m_CircumcircleCenter;
        private Vector2 circumcircleCenter
        {
            get
            {
                if (m_CircumcircleCenter == null)
                {
                    m_CircumcircleCenter = new Vector2();
                }
                return m_CircumcircleCenter;
            }
            set
            {
                m_CircumcircleCenter = value;
            }
        }

        private float m_CircumcircleRadius;
        private float circumcircleRadius
        {
            get
            {
                return m_CircumcircleRadius;
            }

            set
            {
                m_CircumcircleRadius = value;
            }
        }

        public Vertex vertex0
        {
            get
            {
                return m_Vertex0;
            }
        }

        public Vertex vertex1
        {
            get
            {
                return m_Vertex1;
            }
        }

        public Vertex vertex2
        {
            get
            {
                return m_Vertex2;
            }
        }

        public Edge edge0
        {
            get
            {
                return m_Edge0;
            }
        }

        public Edge edge1
        {
            get
            {
                return m_Edge1;
            }
        }

        public Edge edge2
        {
            get
            {
                return m_Edge2;
            }
        }

        public Triangle(Vertex p0, Vertex p1, Vertex p2, bool clockwise)
        {
            List<Vertex> inputPoints = new List<Vertex>();

            inputPoints.Add(p0);
            inputPoints.Add(p1);
            inputPoints.Add(p2);

            inputPoints = inputPoints.OrderBy(x => x.position.x).ToList();

            m_Vertex0 = inputPoints[0].Clone();

            Vector2 up = inputPoints[2].position - inputPoints[0].position;
            up = new Vector2(-up.y, up.x);

            float distanceToPlane = Vector2.Dot(up, (inputPoints[1].position - inputPoints[0].position));

            int clockWiseShift = clockwise ? 0 : 1;

            if (distanceToPlane > 0f)
            {
                m_Vertex1 = inputPoints[1 + clockWiseShift].Clone();
                m_Vertex2 = inputPoints[2 - clockWiseShift].Clone();
            }
            else
            {
                m_Vertex1 = inputPoints[2 - clockWiseShift].Clone();
                m_Vertex2 = inputPoints[1 + clockWiseShift].Clone();
            }

            m_Edge0 = new Edge(m_Vertex0, m_Vertex1);
            m_Edge1 = new Edge(m_Vertex1, m_Vertex2);
            m_Edge2 = new Edge(m_Vertex2, m_Vertex0);

            float len0Square = (vertex0.position.x * vertex0.position.x) + (vertex0.position.y * vertex0.position.y);
            float len1Square = (vertex1.position.x * vertex1.position.x) + (vertex1.position.y * vertex1.position.y);
            float len2Square = (vertex2.position.x * vertex2.position.x) + (vertex2.position.y * vertex2.position.y);

            // Compute the circumcircle of the triangle.
            // TODO: Find better solution for this.
            Vector2 circleCenter = new Vector2();

            circleCenter.x = (len0Square * (vertex2.position.y - vertex1.position.y) + len1Square * (vertex0.position.y - vertex2.position.y) + len2Square * (vertex1.position.y - vertex0.position.y)) / (vertex0.position.x * (vertex2.position.y - vertex1.position.y) + vertex1.position.x * (vertex0.position.y - vertex2.position.y) + vertex2.position.x * (vertex1.position.y - vertex0.position.y)) / 2f;
            circleCenter.y = (len0Square * (vertex2.position.x - vertex1.position.x) + len1Square * (vertex0.position.x - vertex2.position.x) + len2Square * (vertex1.position.x - vertex0.position.x)) / (vertex0.position.y * (vertex2.position.x - vertex1.position.x) + vertex1.position.y * (vertex0.position.x - vertex2.position.x) + vertex2.position.y * (vertex1.position.x - vertex0.position.x)) / 2f;

            m_CircumcircleCenter = circleCenter;

            circumcircleRadius = Mathf.Sqrt(((vertex1.position.x - circumcircleCenter.x) * (vertex1.position.x - circumcircleCenter.x)) + ((vertex1.position.y - circumcircleCenter.y) * (vertex1.position.y - circumcircleCenter.y)));
        }

        public Triangle(Vertex p0, Vertex p1, Vertex p2) : this(p0, p1, p2, true)
        {
            // Do nothing special for defualt case
        }

        public bool PointInCurcumcircle(Vector2 vertex)
        {
            float xDiff = (vertex.x - circumcircleCenter.x);
            float yDiff = (vertex.y - circumcircleCenter.y);
            float distance = Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);

            return distance <= circumcircleRadius;
        }

        public bool PointInCurcumcircle(Vertex vertex)
        {
            bool isInCircumcircle = PointInCurcumcircle(vertex.position);

            return isInCircumcircle;
        }

        public bool Contains(Vertex vertex)
        {
            bool contains = vertex.Equals(vertex0);
            contains |= vertex.Equals(vertex1);
            contains |= vertex.Equals(vertex2);

            return contains;
        }

        public bool Contains(Edge edge)
        {
            bool contains = edge.Equals(edge0);
            contains |= edge.Equals(edge1);
            contains |= edge.Equals(edge2);

            return contains;
        }

        public List<Vertex> GetOverlappingSet(Triangle other)
        {
            List<Vertex> overlap = new List<Vertex>();

            if (other.Contains(vertex0))
            {
                overlap.Add(vertex0);
            }

            if (other.Contains(vertex1))
            {
                overlap.Add(vertex1);
            }

            if (other.Contains(vertex2))
            {
                overlap.Add(vertex2);
            }

            return overlap;
        }

        public override bool Equals(object obj)
        {
            Triangle other = obj as Triangle;

            if (other == null)
            {
                return false;
            }

            bool isSame = m_Vertex0.Equals(other.vertex0) ||
                m_Vertex0.Equals(other.vertex1) || m_Vertex0.Equals(other.vertex2);

            isSame &= m_Vertex1.Equals(other.vertex0) ||
                m_Vertex1.Equals(other.vertex1) || m_Vertex1.Equals(other.vertex2);

            isSame &= m_Vertex2.Equals(other.vertex0) ||
                m_Vertex2.Equals(other.vertex1) || m_Vertex2.Equals(other.vertex2);

            return isSame;
        }

        public override int GetHashCode()
        {
            int hash0 = vertex0.index.GetHashCode();
            int hash1 = vertex1.index.GetHashCode();
            int hash2 = vertex2.index.GetHashCode();
            int combined = ((hash0 << 3) + hash0 ^ hash1) << 5;
            combined += combined ^ hash2;

            return combined;
        }

    }

    public class Vertex
    {
        private Vector2 m_Position;
        public Vector2 position
        {
            get
            {
                return m_Position;
            }
        }

        private int m_Index;
        public int index
        {
            get
            {
                return m_Index;
            }
        }

        public Vertex(Vector2 position, int index)
        {
            m_Position = position;
            m_Index = index;
        }

        public override bool Equals(object obj)
        {
            Vertex other = obj as Vertex;
            
            if (other == null)
            {
                return false;
            }

            if (other.index != m_Index)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return m_Index.GetHashCode();
        }

        public Vertex Clone()
        {
            return new Vertex(m_Position, index);
        }
    }

    public class Triangulation
    {
        private List<Triangle> m_Trinagles;
        public List<Triangle> triangles
        {
            get
            {
                return m_Trinagles;
            }
        }

        private Rect m_BoundingBox;
        private Rect boundingBox
        {
            get
            {
                if (m_BoundingBox == null)
                {
                    m_BoundingBox = new Rect(0, 0, 0, 0);
                }
                return m_BoundingBox;
            }
            set
            {
                m_BoundingBox = value;
            }
        }

        public Triangulation()
        {
            m_Trinagles = new List<Triangle>();
        }

        // Make triangulation from set of triangled, will not actually
        // triangulate, but only store tringles and allow insertion of points
        public Triangulation(List<Triangle> triangles)
        {

            m_Trinagles = triangles;

            // If there are triangles, create a bounding box
            if (m_Trinagles.Count > 0)
            {
                Vertex v = m_Trinagles[0].vertex0;

                float firstX = v.position.x;
                float firstY = v.position.y;

                Rect bBox = new Rect(firstX, firstY, firstX, firstY);

                foreach (Triangle triangle in m_Trinagles)
                {
                    List<Vertex> triangleVertecies = new List<Vertex>();

                    triangleVertecies.Add(triangle.vertex0);
                    triangleVertecies.Add(triangle.vertex1);
                    triangleVertecies.Add(triangle.vertex2);

                    foreach (Vertex vertex in triangleVertecies)
                    {
                        bBox.x = Mathf.Min(boundingBox.x, vertex.position.x);
                        bBox.y = Mathf.Min(boundingBox.y, vertex.position.y);
                        bBox.width = Mathf.Max(boundingBox.width, vertex.position.x);
                        bBox.height = Mathf.Max(boundingBox.height, vertex.position.y);
                    }
                }

                boundingBox = bBox;
            }
        }

        // Make triangulation from set of points
        public Triangulation(List<Vertex> points) : this()
        {
            if (points.Count < 3)
            {
                return;
            }

            // Start with empty triangulation
            List<Triangle> triangulation = new List<Triangle>();

            float firstX = points[0].position.x;
            float firstY = points[0].position.y;

            Rect bBox = new Rect(firstX, firstY, firstX, firstY);

            foreach (Vertex vertex in points)
            {
                bBox.x = Mathf.Min(boundingBox.x, vertex.position.x);
                bBox.y = Mathf.Min(boundingBox.y, vertex.position.y);
                bBox.width = Mathf.Max(boundingBox.width, vertex.position.x);
                bBox.height = Mathf.Max(boundingBox.height, vertex.position.y);
            }

            boundingBox = bBox;

            float superWidth = (boundingBox.size.x - boundingBox.position.x) * 3f + 1f;
            float superHeight = (boundingBox.size.y - boundingBox.position.y) * 3f + 1f;

            // Make super triangle
            Vertex super0 = new Vertex(boundingBox.position - new Vector2(10f, 12f) * 100f, -1);
            Vertex super1 = new Vertex(new Vector2(boundingBox.position.x + superWidth * 100f, boundingBox.position.y * 100f - .95f), -2);
            Vertex super2 = new Vertex(new Vector2(boundingBox.position.x - .95f, boundingBox.position.y + superHeight * 100f), -3);

            Triangle super = new Triangle(super0, super1, super2);

            triangulation.Add(super);

            // Iterate over each point, insert and triangulate
            foreach (Vertex vertex in points)
            {
                // Triangles invalidated at vertex insertion
                List<Triangle> badTriangles = new List<Triangle>();

                // List of edges that should be removed
                List<Edge> polygon = new List<Edge>();

                // List of edges that are incorrectly marked for rmoval
                List<Edge> badEdges = new List<Edge>();

                // Find all invalid triangles
                foreach (Triangle triangle in triangulation)
                {
                    // If the point is in the circumcircle in
                    // a trinagle, add it to the bad set
                    if (triangle.PointInCurcumcircle(vertex))
                    {
                        // Add the triangle and the edges
                        badTriangles.Add(triangle);
                        polygon.Add(triangle.edge0);
                        polygon.Add(triangle.edge1);
                        polygon.Add(triangle.edge2);
                    }
                }

                // Remove bad trinagles from current triangulation
                triangulation.RemoveAll(x => badTriangles.Contains(x));

                // Identify edges in the polygon that are shared
                // among multiple triangles
                for (int i = 0; i < polygon.Count; ++i)
                {
                    for (int j = 0; j < polygon.Count; ++j)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        // If an edge is shared with another triangle
                        // it is removed from the list and will not be
                        // retriangulated.
                        if (polygon[i].Equals(polygon[j]))
                        {
                            badEdges.Add(polygon[i]);
                            badEdges.Add(polygon[j]);
                        }
                    }
                }

                // Remove shared edges from the polygon
                polygon.RemoveAll(x => badEdges.Contains(x));

                // Retriangulate the empty polygonal space
                foreach (Edge edge in polygon)
                {
                    triangulation.Add(new Triangle(edge.point0, edge.point1, vertex));
                }
            }

            // Cleanup from the super trinagle
            for (int i = triangulation.Count - 1; i >= 0; --i)
            {
                // Check if any point is part of the super triangle
                bool isSuper = triangulation[i].Contains(super0);
                isSuper |= triangulation[i].Contains(super1);
                isSuper |= triangulation[i].Contains(super2);

                // If so, remove triangle from triangulation
                if (isSuper)
                {
                    triangulation.RemoveAt(i);
                }
            }

            m_Trinagles = triangulation;
        }

        // Used for adding an internal vertex
        // used only when the vertex falls within the triangulation
        public void AddInternal(Vertex vertex)
        {
            // Triangles invalidated at vertex insertion
            List<Triangle> badTriangles = new List<Triangle>();

            // List of edges that should be removed
            List<Edge> polygon = new List<Edge>();

            // List of edges that are incorrectly marked for rmoval
            List<Edge> badEdges = new List<Edge>();

            // Find all invalid triangles
            foreach (Triangle triangle in m_Trinagles)
            {
                // If the point is in the circumcircle in
                // a trinagle, add it to the bad set
                if (triangle.PointInCurcumcircle(vertex))
                {
                    // Add the triangle and the edges
                    badTriangles.Add(triangle);
                    polygon.Add(triangle.edge0);
                    polygon.Add(triangle.edge1);
                    polygon.Add(triangle.edge2);
                }
            }

            // Remove bad trinagles from current triangulation
            m_Trinagles.RemoveAll(x => badTriangles.Contains(x));

            // Identify edges in the polygon that are shared
            // among multiple triangles
            for (int i = 0; i < polygon.Count; ++i)
            {
                for (int j = 0; j < polygon.Count; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    // If an edge is shared with another triangle
                    // it is removed from the list and will not be
                    // retriangulated.
                    if (polygon[i].Equals(polygon[j]))
                    {
                        badEdges.Add(polygon[i]);
                        badEdges.Add(polygon[j]);
                    }
                }
            }

            // Remove shared edges from the polygon
            polygon.RemoveAll(x => badEdges.Contains(x));

            // Retriangulate the empty polygonal space
            foreach (Edge edge in polygon)
            {
                m_Trinagles.Add(new Triangle(edge.point0, edge.point1, vertex));
            }
        }
    }
}

