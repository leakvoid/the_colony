using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeMeasurer : MonoBehaviour
{
    void Start()
    {
        var mr = GetComponent<BoxCollider>();
        print("Bounds: " + mr.bounds.extents);
        var scaleFactor = 4 / mr.bounds.extents.x;
        print("scale factor: " + scaleFactor);
        //transform.localScale *= scaleFactor;
        //print("New Bounds: " + mesh.bounds.extents);
        //print("local scale: " + transform.localScale);
    }
}
