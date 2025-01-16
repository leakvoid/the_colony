using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPathModule : MonoBehaviour
{
    public struct Pair
    {
        public int x, y;

        public Pair(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    public struct Cell
    {
        public int parentX, parentY;
        public double f, g;
    }

    BuildingLocationModule blm;

    TileAvailability[,] availableSpace;
    bool[,] roads;
    int sizeX;
    int sizeY;

    void Awake()
    {
        blm = FindObjectOfType<BuildingLocationModule>();
    }

    void Initialize()
    {
        availableSpace = blm.GetAvailableSpace();
        sizeX = availableSpace.GetLength(0);
        sizeY = availableSpace.GetLength(1);
        roads = new bool[sizeX, sizeY];
        // TODO place initial road tile
    }

    public void BuildRoad((int x, int y) from)
    {
        Pair start = new Pair(from.x, from.y);

        Pair closestRoad = new Pair(-1, -1);
        
    }

    static void AStar(int[,] grid, Pair src, Pair dest)
    {
        int row = grid.GetLength(0);
        int col = grid.GetLength(1);

        if (src.x == dest.x && src.y == dest.y)
            return;

        bool[,] closedList = new bool[row, col];

        Cell[,] cellDetails = new Cell[row, col];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                cellDetails[i, j].f = double.MaxValue;
                cellDetails[i, j].g = double.MaxValue;
                cellDetails[i, j].parentX = -1;
                cellDetails[i, j].parentY = -1;
            }
        }

        int x = src.x, y = src.y;
        cellDetails[x, y].f = 0.0;
        cellDetails[x, y].g = 0.0;
        cellDetails[x, y].parentX = x;
        cellDetails[x, y].parentY = y;

        SortedSet<(double, Pair)> openList = new SortedSet<(double, Pair)>(
            Comparer<(double, Pair)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));// TODO priority queue

        openList.Add((0.0, new Pair(x, y)));

        while (openList.Count > 0)
        {
            (double f, Pair pair) p = openList.Min;
            openList.Remove(p);

            x = p.pair.x;
            y = p.pair.y;
            closedList[x, y] = true;

            // Generating all the 8 successors of this cell
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;

                    int newX = x + i;
                    int newY = y + j;

                    if (IsValid(newX, newY, row, col))
                    {
                        if (IsDestination(newX, newY, dest))
                        {
                            cellDetails[newX, newY].parentX = x;
                            cellDetails[newX, newY].parentY = y;
                            TracePath(cellDetails, dest);
                            return;
                        }

                        if (!closedList[newX, newY] && IsUnBlocked(grid, newX, newY))
                        {
                            double gNew = cellDetails[x, y].g + 1.0;
                            double fNew = gNew + CalculateDistance(newX, newY, dest);

                            if (cellDetails[newX, newY].f == double.MaxValue || cellDetails[newX, newY].f > fNew)
                            {
                                openList.Add((fNew, new Pair(newX, newY)));

                                cellDetails[newX, newY].f = fNew;
                                cellDetails[newX, newY].g = gNew;
                                cellDetails[newX, newY].parentX = x;
                                cellDetails[newX, newY].parentY = y;
                            }
                        }
                    }
                }
            }
        }
    }

    // A Utility Function to check whether given cell (row, col)
    // is a valid cell or not.
    public static bool IsValid(int row, int col, int ROW, int COL)
    {
        // Returns true if row number and column number
        // is in range
        return (row >= 0) && (row < ROW) && (col >= 0) && (col < COL);
    }

    // A Utility Function to check whether the given cell is
    // blocked or not
    public static bool IsUnBlocked(int[,] grid, int row, int col)
    {
        // Returns true if the cell is not blocked else false
        return grid[row, col] == 1;
    }

    // A Utility Function to check whether destination cell has
    // been reached or not
    public static bool IsDestination(int row, int col, Pair dest)
    {
        return (row == dest.x && col == dest.y);
    }

    public static double CalculateDistance(int x, int y, Pair dest)
    {
        return Mathf.Sqrt(Mathf.Pow(x - dest.x, 2) + Mathf.Pow(y - dest.y, 2));
    }

    // A Utility Function to trace the path from the source
    // to destination
    public static void TracePath(Cell[,] cellDetails, Pair dest)
    {
        print("\nThe Path is ");
        int ROW = cellDetails.GetLength(0);
        int COL = cellDetails.GetLength(1);

        int row = dest.x;
        int col = dest.y;

        Stack<Pair> Path = new Stack<Pair>();

        while (!(cellDetails[row, col].parentX == row && cellDetails[row, col].parentY == col))
        {
            Path.Push(new Pair(row, col));
            int temp_row = cellDetails[row, col].parentX;
            int temp_col = cellDetails[row, col].parentY;
            row = temp_row;
            col = temp_col;
        }

        Path.Push(new Pair(row, col));
        while (Path.Count > 0)
        {
            Pair p = Path.Peek();
            Path.Pop();
            print(" -> (" + p.x + ", " + p.y);
        }
    }

    // Driver method
    public static void Main(string[] args)
    {
        /* Description of the Grid-
            1--> The cell is not blocked
            0--> The cell is blocked */
        int[,] grid =
        {
            {1, 0, 1, 1, 1, 1, 0, 1, 1, 1},
            {1, 1, 1, 0, 1, 1, 1, 0, 1, 1},
            {1, 1, 1, 0, 1, 1, 0, 1, 0, 1},
            {0, 0, 1, 0, 1, 0, 0, 0, 0, 1},
            {1, 1, 1, 0, 1, 1, 1, 0, 1, 0},
            {1, 0, 1, 1, 1, 1, 0, 1, 0, 0},
            {1, 0, 0, 0, 0, 1, 0, 0, 0, 1},
            {1, 0, 1, 1, 1, 1, 0, 1, 1, 1},
            {1, 1, 1, 0, 0, 0, 1, 0, 0, 1}
        };

        // Source is the left-most bottom-most corner
        Pair src = new Pair(8, 0);

        // Destination is the left-most top-most corner
        Pair dest = new Pair(0, 0);

        AStar(grid, src, dest);
    }







}
