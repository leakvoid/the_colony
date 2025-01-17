using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

    [SerializeField] GameObject roadPrefab;

    TileAvailability[,] availableSpace;
    bool[,] roads;
    int sizeX;
    int sizeY;

    void Awake()
    {
        blm = FindObjectOfType<BuildingLocationModule>();
    }

    public void Initialize()
    {
        availableSpace = blm.GetAvailableSpace();
        sizeX = availableSpace.GetLength(0);
        sizeY = availableSpace.GetLength(1);
        roads = new bool[sizeX, sizeY];
    }

    public void SetFirstRoad((int x, int y) pos)
    {
        roads[pos.x, pos.y] = true;
    }

    public void BuildRoad((int x, int y) from)
    {
        Pair start = new Pair(from.x, from.y);

        Pair closestRoad = new Pair();
        double minDistance = double.MaxValue;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (roads[x, y])
                {
                    double distance = CalculateDistance(x, y, start);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestRoad.x = x;
                        closestRoad.y = y;
                    }
                }
            }
        }

        AStar(start, closestRoad);
    }

    void AStar(Pair start, Pair closestRoad)
    {
        if (start.x == closestRoad.x && start.y == closestRoad.y)
            return;

        bool[,] closedList = new bool[sizeX, sizeY];

        Cell[,] cellDetails = new Cell[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                cellDetails[i, j].f = double.MaxValue;
                cellDetails[i, j].g = double.MaxValue;
                cellDetails[i, j].parentX = -1;
                cellDetails[i, j].parentY = -1;
            }
        }

        int x = start.x, y = start.y;
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

            for (int i = 0; i < 4; i++)
            {
                int newX, newY;
                switch (i)
                {
                    case 0:
                        newX = x + 1;
                        newY = y;
                        break;
                    case 1:
                        newX = x - 1;
                        newY = y;
                        break;
                    case 2:
                        newX = x;
                        newY = y + 1;
                        break;
                    default:
                        newX = x;
                        newY = y - 1;
                        break;
                }

                if (IsValid(newX, newY))
                {
                    if (roads[newX, newY])
                    {
                        cellDetails[newX, newY].parentX = x;
                        cellDetails[newX, newY].parentY = y;
                        TracePath(cellDetails, new Pair(newX, newY));
                        return;
                    }

                    if (!closedList[newX, newY] && !IsBlocked(newX, newY))
                    {
                        double gNew = cellDetails[x, y].g + 1.0;
                        double fNew = gNew + CalculateDistance(newX, newY, closestRoad);

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
        throw new Exception("path for " + start.x + ", " + start.y + " not found");
    }

    bool IsValid(int x, int y)
    {
        return (x >= 0) && (x < sizeX) && (y >= 0) && (y < sizeY);
    }

    bool IsBlocked(int x, int y)
    {
        return availableSpace[x, y] == TileAvailability.Taken;
    }

    double CalculateDistance(int x, int y, Pair dest)
    {
        return Mathf.Abs(x - dest.x) + Mathf.Abs(y - dest.y);
        //return Mathf.Sqrt(Mathf.Pow(x - dest.x, 2) + Mathf.Pow(y - dest.y, 2));
    }

    void TracePath(Cell[,] cellDetails, Pair dest)
    {
        int x = dest.x;
        int y = dest.y;

        while (!(cellDetails[x, y].parentX == x && cellDetails[x, y].parentY == y))
        {
            Instantiate(roadPrefab, Globals.GridToGlobalCoordinates((x, y)), Quaternion.identity);
            roads[x, y] = true;
            availableSpace[x, y] = TileAvailability.Taken;

            int tempX = cellDetails[x, y].parentX;
            int tempY = cellDetails[x, y].parentY;
            x = tempX;
            y = tempY;
        }
        Instantiate(roadPrefab, Globals.GridToGlobalCoordinates((x, y)), Quaternion.identity);
        roads[x, y] = true;
        availableSpace[x, y] = TileAvailability.Taken;
    }
}
