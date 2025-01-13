using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    Globals globals;
    MinimapCameraShadow mcs;

    [SerializeField] float cameraMoveSpeed = 100f;
    [SerializeField] float cameraZoomFactor = 0.8f;
    [SerializeField] float cameraRotationSpeed = 100f;

    Vector3 screenPosSnapshot;
    Vector3 rotationPoint;

    int gridX;
    int gridY;
    float aspectRatio = 16f/9f;
    Vector3 currentPos;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        mcs = FindObjectOfType<MinimapCameraShadow>();
    }

    public void Initialize()
    {
        var grid = FindObjectOfType<AbstractMapGenerator>().GetTerrainGrid();
        gridX = grid.GetLength(0);
        gridY = grid.GetLength(1);

        mcs.Initialize();
        currentPos = transform.position;
    }

    void Update()
    {
        UpdateCameraPosition();
        UpdateCameraSize();
        UpdateCameraRotation();
        if (currentPos != transform.position)
        {
            currentPos = transform.position;
            mcs.Redraw();
        }
    }

    void UpdateCameraPosition()// TODO camera speed scales with zoom
    {
        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var cameraPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (screenPos.x <= 0.02 || Input.GetButton("Left"))
        {
            if ((cameraPos.x - cameraPos.y) >= 0)
                transform.position -= transform.right * cameraMoveSpeed * Time.deltaTime;
        }
        else if (screenPos.x >= 0.98 || Input.GetButton("Right"))
        {
            if ((cameraPos.x + cameraPos.y) <= gridX)
                transform.localPosition += transform.right * cameraMoveSpeed * Time.deltaTime;
        }
        if (screenPos.y <= 0.02 || Input.GetButton("Down"))
        {
            if ((cameraPos.z - cameraPos.y / aspectRatio) >= 0)
            {
                var direction = new Vector3(transform.up.x, 0, transform.up.z);
                float correction = 1 / (Mathf.Abs(transform.up.x) + Mathf.Abs(transform.up.z));
                transform.localPosition -= direction * correction * cameraMoveSpeed * Time.deltaTime;
            }
        }
        else if (screenPos.y >= 0.98 || Input.GetButton("Up"))
        {
            if ((cameraPos.z + cameraPos.y / aspectRatio) <= gridY)
            {
                var direction = new Vector3(transform.up.x, 0, transform.up.z);
                float correction = 1 / (Mathf.Abs(transform.up.x) + Mathf.Abs(transform.up.z));
                transform.localPosition += direction * correction * cameraMoveSpeed * Time.deltaTime;
            }
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

    void UpdateCameraRotation()
    {
        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(2))
        {
            screenPosSnapshot = screenPos;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (globals.GroundPlane.Raycast(ray, out float enter))
            {
                rotationPoint = ray.GetPoint(enter);
            }
            else
            {
                rotationPoint = transform.position;
                rotationPoint.y = 0f;
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

            transform.RotateAround(rotationPoint, Vector3.up, dx * cameraRotationSpeed * Time.deltaTime);
            transform.RotateAround(rotationPoint, transform.right, dy * cameraRotationSpeed * Time.deltaTime);// TODO clamp
        }
    }

    public void MoveCameraViaMinimap(Vector3 minimapPos)
    {
        transform.position = new Vector3(gridX * minimapPos.x / 100, transform.position.y, gridY * minimapPos.z / 100);
    }
}
