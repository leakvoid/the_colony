using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Utils;

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

    Globals globals;
    BuildingLocationModule blm;

    [SerializeField] GameObject roadPrefab;

    TileAvailability[,] availableSpace;
    bool[,] roads;
    int sizeX;
    int sizeY;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        blm = FindObjectOfType<BuildingLocationModule>();
    }

    public void Initialize()
    {
        availableSpace = blm.GetAvailableSpace();
        sizeX = availableSpace.GetLength(0);
        sizeY = availableSpace.GetLength(1);
        roads = new bool[sizeX, sizeY];
        savedPaths = new Dictionary<(Pair, Pair), Stack<Pair>>();
    }

    public (int x, int y) SetFirstRoad((int x, int y) pos, BuildingTemplate bt)
    {
        if (pos.x > 0)
        {
            roads[pos.x - 1, pos.y] = true;
            return (pos.x - 1, pos.y);
        }
        else
        {
            roads[pos.x + bt.SizeX, pos.y] = true;
            return (pos.x + bt.SizeX, pos.y);
        }
    }

    public (int x, int y) BuildRoad((int x, int y) from, BuildingTemplate bt)
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

        Pair res = new Pair();
        for (int x = start.x; x < start.x + bt.SizeX; x++)
        {
            if (start.y > 0)
            {
                double distance = CalculateDistance(x, start.y - 1, closestRoad);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    res.x = x;
                    res.y = start.y - 1;
                }
            }
            if (start.y + bt.SizeY < sizeY)
            {
                double distance = CalculateDistance(x, start.y + bt.SizeY, closestRoad);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    res.x = x;
                    res.y = start.y + bt.SizeY;
                }
            }
        }

        for (int y = start.y; y < start.y + bt.SizeY; y++)
        {
            if (start.x > 0)
            {
                double distance = CalculateDistance(start.x - 1, y, closestRoad);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    res.x = start.x - 1;
                    res.y = y;
                }
            }
            if (start.x + bt.SizeX < sizeX)
            {
                double distance = CalculateDistance(start.x + bt.SizeX, y, closestRoad);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    res.x = start.x + bt.SizeX;
                    res.y = y;
                }
            }
        }
        start = res;

        AStar(start, closestRoad);
        return (start.x, start.y);
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

        var openList = new PriorityQueue<Pair, double>();
        openList.Enqueue(new Pair(x, y), 0.0);

        while (openList.Count > 0)
        {
            var pair = openList.Dequeue();

            x = pair.x;
            y = pair.y;
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

                if ((newX >= 0) && (newX < sizeX) && (newY >= 0) && (newY < sizeY))
                {
                    if (roads[newX, newY])
                    {
                        cellDetails[newX, newY].parentX = x;
                        cellDetails[newX, newY].parentY = y;
                        TracePath(cellDetails, new Pair(newX, newY));
                        return;
                    }

                    if (!closedList[newX, newY] && availableSpace[newX, newY] != TileAvailability.Taken)
                    {
                        double gNew = cellDetails[x, y].g + 1.0;
                        double fNew = gNew + CalculateDistance(newX, newY, closestRoad);

                        if (cellDetails[newX, newY].f > fNew)
                        {
                            openList.Enqueue(new Pair(newX, newY), fNew);

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

    double CalculateDistance(int x, int y, Pair dest)
    {
        return Mathf.Abs(x - dest.x) + Mathf.Abs(y - dest.y);
    }

    void TracePath(Cell[,] cellDetails, Pair dest)
    {
        int x = dest.x;
        int y = dest.y;

        while (!(cellDetails[x, y].parentX == x && cellDetails[x, y].parentY == y))
        {
            Instantiate(roadPrefab, Globals.GridToGlobalCoordinates((x, y), roadPrefab), Quaternion.identity);
            roads[x, y] = true;
            availableSpace[x, y] = TileAvailability.Taken;

            int tempX = cellDetails[x, y].parentX;
            int tempY = cellDetails[x, y].parentY;
            x = tempX;
            y = tempY;
        }
        Instantiate(roadPrefab, Globals.GridToGlobalCoordinates((x, y), roadPrefab), Quaternion.identity);
        roads[x, y] = true;
        availableSpace[x, y] = TileAvailability.Taken;
    }

    Dictionary<(Pair, Pair), Stack<Pair>> savedPaths;

    public IEnumerator MoveColonist((int x, int y) _from, (int x, int y) _to, GameObject colonistModel)
    {
        Pair from = new Pair(_from.x, _from.y);
        Pair to = new Pair(_to.x, _to.y);

        Stack<Pair> path;
        if (savedPaths.ContainsKey((from, to)))
            path = savedPaths[(from, to)];
        else
            path = CreateNewPath(from, to);

        foreach (var p in path)
        {
            var end = Globals.GridToGlobalCoordinates((p.x, p.y), colonistModel);
            while (colonistModel.transform.position != end)
            {
                colonistModel.transform.position = Vector3.MoveTowards(colonistModel.transform.position,
                    end, globals.ColonistMovementSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    Stack<Pair> CreateNewPath(Pair from, Pair to)
    {
        bool RecursivePath(Pair pos, Pair target, Stack<Pair> path, bool[,] traversed)
        {
            if (pos.x < 0 || pos.x >= sizeX || pos.y < 0 || pos.y >= sizeY ||
                !roads[pos.x, pos.y] || traversed[pos.x, pos.y])
                return false;
            
            path.Push(pos);
            traversed[pos.x, pos.y] = true;

            if (pos.x == target.x && pos.y == target.y)
                return true;

            if (RecursivePath(new Pair(pos.x + 1, pos.y), target, path, traversed) ||
                RecursivePath(new Pair(pos.x - 1, pos.y), target, path, traversed) ||
                RecursivePath(new Pair(pos.x, pos.y + 1), target, path, traversed) ||
                RecursivePath(new Pair(pos.x, pos.y - 1), target, path, traversed))
                return true;

            path.Pop();
            return false;
        }

        var path = new Stack<Pair>();// TODO improve stack space and logic for remaking road when building placed on top of road
        var traversed = new bool[sizeX, sizeY];
        RecursivePath(to, from, path, traversed);

        savedPaths[(from, to)] = path;
        savedPaths[(to, from)] = new Stack<Pair>(path);
        return path;
    }
}