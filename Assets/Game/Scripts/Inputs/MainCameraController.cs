using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] float cameraMoveSpeed = 100f;
    [SerializeField] float cameraZoomFactor = 0.8f;
    [SerializeField] float cameraRotationSpeed = 100f;

    [SerializeField] GameObject cameraPositionBody;
    [SerializeField] GameObject cameraFlatRotationBody;

    Vector3 screenPosSnapshot;

    int gridX;
    int gridY;

    float aspectRatio = 16f/9f;

    public void Initialize()
    {
        var grid = FindObjectOfType<AbstractMapGenerator>().GetTerrainGrid();
        gridX = grid.GetLength(0);
        gridY = grid.GetLength(1);
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
            if ((cameraPos.x + cameraPos.z) >= 0)
                cameraPositionBody.transform.position -= Globals.NewVector(cameraMoveSpeed * Time.deltaTime, 0);
        }
        else if (screenPos.x >= 0.98 || Input.GetButton("Right"))
        {
            if ((cameraPos.x - cameraPos.z) <= gridX)
                cameraPositionBody.transform.position += Globals.NewVector(cameraMoveSpeed * Time.deltaTime, 0);
        }
        if (screenPos.y <= 0.02 || Input.GetButton("Down"))
        {
            if ((cameraPos.y + cameraPos.z / aspectRatio) >= 0)
                cameraPositionBody.transform.position -= Globals.NewVector(0, cameraMoveSpeed * Time.deltaTime);
        }
        else if (screenPos.y >= 0.98 || Input.GetButton("Up"))
        {
            if ((cameraPos.y - cameraPos.z / aspectRatio) <= gridY)
                cameraPositionBody.transform.position += Globals.NewVector(0, cameraMoveSpeed * Time.deltaTime);
        }
    }

    void UpdateCameraSize()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            var zoom = cameraPositionBody.transform.position;
            zoom.y *= cameraZoomFactor;
            if (zoom.y > 0.3f)
                cameraPositionBody.transform.position = zoom;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            var zoom = cameraPositionBody.transform.position;
            zoom.y /= cameraZoomFactor;
            if (zoom.y < 50f)
                cameraPositionBody.transform.position = zoom;
        }
    }

    Vector3 target;
    float zRotation;
    float xRotation;
    float distance;
    void Start()
    {
        zRotation = cameraFlatRotationBody.transform.eulerAngles.z;
        xRotation = Camera.main.transform.eulerAngles.x;
    }

    void UpdateCameraRotation()
    {
        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(2))
        {
            screenPosSnapshot = screenPos;
            target = cameraPositionBody.transform.position;//Camera.main.ScreenToWorldPoint(Input.mousePosition);
            print(target);
            distance = target.z;
            target.z = 0f;
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

            //zRotation += Input.GetAxis("Mouse X") * cameraRotationSpeed * Time.deltaTime / 10;
            //xRotation -= Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime / 10;

            //Quaternion zQuaternion = Quaternion.Euler(0, 0, zRotation);
            //cameraFlatRotationBody.transform.rotation = zQuaternion;
            Camera.main.transform.Rotate(0, 0, dx * cameraRotationSpeed * Time.deltaTime, Space.World);

            //Quaternion xQuaternion = Quaternion.Euler(xRotation, 0, 0);
            //Camera.main.transform.rotation = xQuaternion;
            Camera.main.transform.Rotate(dy * cameraRotationSpeed * Time.deltaTime, 0, 0, Space.Self);




            //Vector3 negDistance = new Vector3(0.0f, 0.0f, distance);
            //cameraPositionBody.transform.position = xQuaternion * negDistance + target;



            //y = ClampAngle(y, yMinLimit, yMaxLimit);


            /*Vector3 relativePos = target - Camera.main.transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(relativePos);
            Camera.main.transform.rotation = rotationToTarget;*/

            /*Quaternion rotationY = Quaternion.AngleAxis(-dy * cameraRotationSpeed * Time.deltaTime, transform.right);
            Camera.main.transform.rotation *= rotationY;

            Quaternion rotationX = Quaternion.AngleAxis(-dx * cameraRotationSpeed * Time.deltaTime, Vector3.up);
            Camera.main.transform.rotation *= rotationX;*/
        }
    }
}
