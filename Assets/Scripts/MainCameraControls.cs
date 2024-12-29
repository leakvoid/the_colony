using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraControls : MonoBehaviour
{
    [SerializeField] float cameraMoveSpeed = 100f;
    [SerializeField] float cameraZoomSpeed = 100f;
    [SerializeField] float cameraRotationSpeed = 100f;

    Vector3 mousePos;
    Vector3 oldPosition;

    Vector3 screenPosSnapshot;

    void Update()
    {
        mousePos = Input.mousePosition;
        if (mousePos != oldPosition)
        {
            oldPosition = mousePos;
            print("World pos: " + mousePos);
            print("Screen pos: " + Camera.main.ScreenToWorldPoint(mousePos));
            print("viewport: " + Camera.main.ScreenToViewportPoint(mousePos));
        }

        var screenPos = Camera.main.ScreenToViewportPoint(mousePos);
        if (screenPos.x <= 0.02 || Input.GetButton("Left"))
        {
            Camera.main.transform.position -= new Vector3(cameraMoveSpeed * Time.deltaTime, 0);
        }
        else if (screenPos.x >= 0.98 || Input.GetButton("Right"))
        {
            Camera.main.transform.position += new Vector3(cameraMoveSpeed * Time.deltaTime, 0);
        }
        if (screenPos.y <= 0.02 || Input.GetButton("Down"))
        {
            Camera.main.transform.position -= new Vector3(0, cameraMoveSpeed * Time.deltaTime);
        }
        else if (screenPos.y >= 0.98 || Input.GetButton("Up"))
        {
            Camera.main.transform.position += new Vector3(0, cameraMoveSpeed * Time.deltaTime);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            Camera.main.transform.position += new Vector3(0, 0, cameraZoomSpeed * Time.deltaTime);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            Camera.main.transform.position -= new Vector3(0, 0, cameraZoomSpeed * Time.deltaTime);
        }

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
