using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForTesting : MonoBehaviour
{
    [SerializeField] GameObject myPrefab;
    [SerializeField] CubeData myPrefab2;

    GameObject prefabInstance1;
    GameObject prefabInstance2;

    void Start()
    {
        prefabInstance1 = Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        CubeData cubeData1 = prefabInstance1.GetComponent<CubeData>();
        cubeData1.foodNeed = 150;

        prefabInstance2 = Instantiate(myPrefab, new Vector3(3, 3, 0), Quaternion.identity);
        CubeData cubeData2 = prefabInstance2.GetComponent<CubeData>();
        cubeData2.foodNeed = 69;

        CubeData cubeData3 = Instantiate(myPrefab2);
        cubeData3.foodNeed = 77;

        print("cubeData1 " + cubeData1.foodNeed);
        print("cubeData2 " + cubeData2.foodNeed);
        print("cubeData3 " + cubeData3.foodNeed);
    }

    void Update()
    {

    }
}
