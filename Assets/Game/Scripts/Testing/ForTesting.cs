using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ForTesting : MonoBehaviour
{
    [SerializeField] GameObject myPrefab;
    [SerializeField] CubeData myPrefab2;

    GameObject prefabInstance1;
    GameObject prefabInstance2;
    GameObject prefabInstance3;

    void Start()
    {
        prefabInstance1 = Instantiate(myPrefab, new Vector3(1, 1, 0), Quaternion.identity);
        CubeData cubeData1 = prefabInstance1.GetComponent<CubeData>();
        cubeData1.foodNeed = 150;

        prefabInstance2 = Instantiate(myPrefab, new Vector3(3, 3, 0), Quaternion.identity);
        CubeData cubeData2 = prefabInstance2.GetComponent<CubeData>();
        cubeData2.foodNeed = 69;

        prefabInstance3 = Instantiate(myPrefab, new Vector3(5, 5, 0), Quaternion.identity);

        CubeData cubeData3 = Instantiate(myPrefab2);// also prefab
        cubeData3.foodNeed = 77;

        print("cubeData1 " + cubeData1.foodNeed);
        print("cubeData2 " + cubeData2.foodNeed);
        print("cubeData3 " + cubeData3.foodNeed);

        StartCoroutine(MoveOverSpeed(prefabInstance1, new Vector3(0f, 5f, 0f), 5f));
        StartCoroutine(MoveOverSpeed(prefabInstance2, new Vector3(5f, 7f, 0f), 5f));

        CreateWorker();
    }

    [SerializeField] WorkerData abstractWorker;
    [SerializeField] GameObject workerModel;

    void CreateWorker()
    {
        WorkerData abstractWorkerInstance = Instantiate(abstractWorker);

        abstractWorkerInstance.workerModel = Instantiate(workerModel, new Vector3(-1, -1, 0), Quaternion.identity);
        abstractWorkerInstance.workerModel.transform.parent = abstractWorkerInstance.transform;
    }

    IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
    {
        var wait = new WaitForEndOfFrame();
        while (objectToMove.transform.position != end)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return wait;
        }
    }

    void Update()
    {
        var end = new Vector3(2f, 2f, 2f);
        if (prefabInstance3.transform.position != end)
            prefabInstance3.transform.position = Vector3.MoveTowards(prefabInstance3.transform.position, end, 0.5f * Time.deltaTime);
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.gameObject.name == "Cube")
                {
                    var cubeData = hit.transform.parent.gameObject.GetComponent<CubeData>();
                    print(cubeData.foodNeed);
                }
                if (hit.transform.gameObject.name == "WorkerModel(Clone)")
                {
                    var workerData = hit.transform.parent.gameObject.GetComponent<WorkerData>();
                    print("foodNeed: " + workerData.foodNeed);
                }

                Debug.Log("name: " + hit.transform.gameObject.name);
                //Debug.Log(hit.collider.gameObject.name);
            }
        }
    }

    /*
         void CastRay()
         {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast (ray.origin, ray.direction, Mathf.Infinity);
            if (hit.collider !=null)
            {
                Debug.Log (hit.collider.gameObject.name);
            }
        }
    */
}
