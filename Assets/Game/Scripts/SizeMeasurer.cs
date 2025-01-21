using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeMeasurer : MonoBehaviour
{
    [SerializeField] int sizeX = 4;
    [SerializeField] int sizeY = 3;

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

        if (transform.childCount > 0)
        {
            var child = transform.GetChild(0);
            var mr = child.GetComponent<MeshRenderer>();
            if(mr)
            {
                Debug.Log("Child Mesh Bounds: " + mr.bounds);
                var scaleX = (float)sizeX / mr.bounds.extents.x / 2;
                print("scale factor X: " + scaleX);
                var scaleY = (float)sizeY / mr.bounds.extents.z / 2;
                print("scale factor Y: " + scaleY);
                var res1 = new Vector3(child.transform.localScale.x * scaleX,
                    child.transform.localScale.y * scaleY, child.transform.localScale.z * scaleX);
                Debug.Log("Target (-90,0,0) scale: (" + res1.x + ", " + res1.y + ", " + res1.z + ")");
                var res2 = new Vector3(child.transform.localScale.x * scaleX,
                    child.transform.localScale.y * scaleX, child.transform.localScale.z * scaleY);
                Debug.Log("Target (0,0,0) scale: (" + res2.x + ", " + res2.y + ", " + res2.z + ")");
            }
        }

        //var scaleFactor = 4 / mr.bounds.extents.x;
        //print("scale factor: " + scaleFactor);
        //transform.localScale *= scaleFactor;
        //print("New Bounds: " + mesh.bounds.extents);
        //print("local scale: " + transform.localScale);
    }
}
