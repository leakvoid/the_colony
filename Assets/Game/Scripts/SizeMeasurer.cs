using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeMeasurer : MonoBehaviour
{
    void Start()
    {
        /*Debug.Log("Name: " + gameObject.name);
        var mr = GetComponent<MeshRenderer>();
        if(mr)
        {
            Debug.Log("Mesh Bounds: " + mr.bounds);
        }
        var bc = GetComponent<BoxCollider>();
        if (bc)
        {
            print("Collider Bounds: " + bc.bounds);
        }*/
        (int x, int y) buildingSize = (4, 4);

        if (transform.childCount > 0)
        {
            var child = transform.GetChild(0);
            var mr = child.GetComponent<MeshRenderer>();
            if(mr)
            {
                Debug.Log("Child Mesh Bounds: " + mr.bounds);
                var scaleX = buildingSize.x / mr.bounds.extents.x / 2;
                print("scale factor X: " + scaleX);
                var scaleY = buildingSize.y / mr.bounds.extents.z / 2;
                print("scale factor Y: " + scaleY);
                Debug.Log("Target scale: " + new Vector3(child.transform.localScale.x * scaleX,
                    child.transform.localScale.y * scaleY, child.transform.localScale.z * scaleX));
            }
        }

        //var scaleFactor = 4 / mr.bounds.extents.x;
        //print("scale factor: " + scaleFactor);
        //transform.localScale *= scaleFactor;
        //print("New Bounds: " + mesh.bounds.extents);
        //print("local scale: " + transform.localScale);
    }
}
