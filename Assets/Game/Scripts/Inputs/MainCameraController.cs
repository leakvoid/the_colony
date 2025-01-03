using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [SerializeField] float cameraMoveSpeed = 100f;
    [SerializeField] float cameraZoomSpeed = 100f;
    [SerializeField] float cameraRotationSpeed = 100f;

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

    void UpdateCameraPosition()
    {
        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var cameraPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (screenPos.x <= 0.02 || Input.GetButton("Left"))
        {
            if ((cameraPos.x + cameraPos.z) >= 0)
                Camera.main.transform.position -= new Vector3(cameraMoveSpeed * Time.deltaTime, 0);
        }
        else if (screenPos.x >= 0.98 || Input.GetButton("Right"))
        {
            if ((cameraPos.x - cameraPos.z) <= gridX)
                Camera.main.transform.position += new Vector3(cameraMoveSpeed * Time.deltaTime, 0);
        }
        if (screenPos.y <= 0.02 || Input.GetButton("Down"))
        {
            if ((cameraPos.y + cameraPos.z / aspectRatio) >= 0)
                Camera.main.transform.position -= new Vector3(0, cameraMoveSpeed * Time.deltaTime);
        }
        else if (screenPos.y >= 0.98 || Input.GetButton("Up"))
        {
            if ((cameraPos.y - cameraPos.z / aspectRatio) <= gridY)
                Camera.main.transform.position += new Vector3(0, cameraMoveSpeed * Time.deltaTime);
        }
    }

    void UpdateCameraSize()// TODO change to zoom multiplier
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            Camera.main.transform.position += new Vector3(0, 0, cameraZoomSpeed * Time.deltaTime);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            Camera.main.transform.position -= new Vector3(0, 0, cameraZoomSpeed * Time.deltaTime);
        }
    }

    void UpdateCameraRotation()
    {
        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(2))
        {
            screenPosSnapshot = screenPos;
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
            Camera.main.transform.eulerAngles += new Vector3(dy * cameraRotationSpeed * Time.deltaTime, 0, dx * cameraRotationSpeed * Time.deltaTime);
        }
    }
}
