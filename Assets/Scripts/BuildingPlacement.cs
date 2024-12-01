using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    AbstractMapGenerator mapGenerator;

    bool[,] marketCoverArea;
    bool[,] churchCoverArea;
    bool[,] innCoverArea;
    bool[,] wellCoverArea;

    bool[,] uncoveredByMarket;
    bool[,] uncoveredByChurch;
    bool[,] uncoveredByInn;
    bool[,] uncoveredByWell;

    bool[,] availableSpaces;

    // TODO resource capture area

    void Awake()
    {
        mapGenerator = FindObjectOfType<AbstractMapGenerator>();
    }

    void Start()
    {
        Globals.TerrainType[,] terrainGrid = mapGenerator.GetTerrainGrid();
    }



    /*
    What is the right approach for building placement?
    
    1. flagMap of every service building placement
        overlap of those flagMaps for best house building placement

    2. flagMap of all houses unaffected by a particular service

    3. flagMap of all spaces with buildings / natural barriers (free or taken)
    ? OR list of all free spaces

    1 + 3 cascading rule for placing a house (from highest overlap to no service buildings)

    2 + 3 find empty space where service building area (square) covers highest number of unaffected houses

    farming and processing can be placed wherever (MAYBE introduce farmland / warehouse logic)

    gathering nearby resources 4. get flagMap for each already mined resource
     */
}
