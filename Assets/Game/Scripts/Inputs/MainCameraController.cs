using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] float cameraMoveSpeed = 100f;
    [SerializeField] float cameraZoomFactor = 0.8f;
    [SerializeField] float cameraRotationSpeed = 100f;

    Vector3 screenPosSnapshot;
    Plane groundPlane;

    int gridX;
    int gridY;

    float aspectRatio = 16f/9f;

    public void Initialize()
    {
        var grid = FindObjectOfType<AbstractMapGenerator>().GetTerrainGrid();
        gridX = grid.GetLength(0);
        gridY = grid.GetLength(1);
        groundPlane = new Plane(new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(0,0,1));
    }

    void Update()
    {
        UpdateCameraPosition();
        UpdateCameraSize();
        UpdateCameraRotation();
    }

    void UpdateCameraPosition()// TODO camera speed scales with zoom
    {
        var screenPos = new Vector3(0.5f, 0.5f, 0);//Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var cameraPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (screenPos.x <= 0.02 || Input.GetButton("Left"))
        {
            if ((cameraPos.x - cameraPos.y) >= 0)
                transform.position -= Globals.NewVector(cameraMoveSpeed * Time.deltaTime, 0);
        }
        else if (screenPos.x >= 0.98 || Input.GetButton("Right"))
        {
            if ((cameraPos.x + cameraPos.y) <= gridX)
                transform.position += Globals.NewVector(cameraMoveSpeed * Time.deltaTime, 0);
        }
        if (screenPos.y <= 0.02 || Input.GetButton("Down"))
        {
            if ((cameraPos.z + cameraPos.y / aspectRatio) >= 0)
                transform.position -= Globals.NewVector(0, cameraMoveSpeed * Time.deltaTime);
        }
        else if (screenPos.y >= 0.98 || Input.GetButton("Up"))
        {
            if ((cameraPos.z - cameraPos.y / aspectRatio) <= gridY)
                transform.position += Globals.NewVector(0, cameraMoveSpeed * Time.deltaTime);
        }
    }

    void UpdateCameraSize()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            var zoom = transform.position;
            zoom.y *= cameraZoomFactor;
            if (zoom.y > 0.3f)
                transform.position = zoom;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            var zoom = transform.position;
            zoom.y /= cameraZoomFactor;
            if (zoom.y < 50f)
                transform.position = zoom;
        }
    }

    Vector3 target;
    float zRotation;
    float xRotation;
    float distance = 0;
    void Start()
    {
        zRotation = transform.eulerAngles.z;
        xRotation = Camera.main.transform.eulerAngles.x;
    }

    void UpdateCameraRotation()
    {
        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(2))
        {
            screenPosSnapshot = screenPos;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            print("mouse pos: " + Input.mousePosition);
            if (groundPlane.Raycast(ray, out float enter))
            {
                target = ray.GetPoint(enter);
                distance = Vector3.Distance(transform.position, target);
                print("distance: " + distance);
                print("YES:" + target);
            }
            else
            {
                target = transform.position;
                distance = target.y;
                target.y = 0f;
                print("NO");
            }
        }
        if (Input.GetButton("HoldMiddleMouse"))
        {
            var dx = screenPosSnapshot.x - screenPos.x;
            var dy = screenPosSnapshot.y - screenPos.y;
            if (Math.Abs(dx) < 0.05f)
                dx = 0;
            if (Math.Abs(dy) < 0.05f)
                dy = 0;
            if (dx < 0)
                dx = -1;
            else if(dx > 0)
                dx = 1;
            if (dy < 0)
                dy = -1;
            else if (dy > 0)
                dy = 1;

            //transform.Rotate(0, dx * cameraRotationSpeed * Time.deltaTime, 0, Space.World);
            //transform.Rotate(dy * cameraRotationSpeed * Time.deltaTime, 0, 0, Space.Self);

            //Vector3 negDistance = new Vector3(0.0f, -distance, 0.0f);
            //transform.position = transform.rotation * negDistance + target;


            transform.RotateAround(target, Vector3.up, dx * cameraRotationSpeed * Time.deltaTime);
            transform.RotateAround(target, Vector3.right, dy * cameraRotationSpeed * Time.deltaTime);


            //zRotation += Input.GetAxis("Mouse X") * cameraRotationSpeed * Time.deltaTime / 10;
            //xRotation -= Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime / 10;

            //Quaternion zQuaternion = Quaternion.Euler(0, 0, zRotation);
            //cameraFlatRotationBody.transform.rotation = zQuaternion;

            /*Quaternion xQuaternion = Quaternion.Euler(xRotation, 0, 0);
            transform.rotation = xQuaternion;
            Vector3 negDistance = new Vector3(0.0f, 0.0f, distance);
            transform.position = xQuaternion * negDistance + target;*/



            //y = ClampAngle(y, yMinLimit, yMaxLimit);
        }
    }
}
