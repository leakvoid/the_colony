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
    Vector3 currentPos;
    float currentZoomValue = 0;
    float cameraSpeedFactor = 1;

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

        float[] distances = new float[32];
        distances[7] = 25;
        Camera.main.layerCullDistances = distances;
    }

    void Update()
    {
        UpdateCameraPosition();
        UpdateCameraSize();
        UpdateCameraRotation();
        UpdateCameraClipping();
        UpdateMinimapShadow();
    }

    void UpdateCameraClipping()
    {
        if (currentZoomValue != transform.position.y)
        {
            currentZoomValue = transform.position.y;

            // 90 -> 1, 0 -> 4, 180 -> 4
            float angleCoefficient = Mathf.Lerp(1f, 10f, Mathf.Abs(transform.eulerAngles.x - 90) / 90);
            Camera.main.farClipPlane = (currentZoomValue + 5) * angleCoefficient;

            cameraSpeedFactor = 0.3f + Mathf.InverseLerp(5f, 25f, transform.position.y) * 0.7f;
        }
    }

    void UpdateMinimapShadow()
    {
        if (currentPos != transform.position)
        {
            currentPos = transform.position;
            mcs.Redraw();
        }
    }

    void UpdateCameraPosition()
    {
        bool IsValidMovement(Vector3 shift)
        {
            var edges = mcs.GetEdges();
            Vector3 center = (edges[0] + edges[1] + edges[2] + edges[3]) / 4;

            if (center.x < 0 || center.x > gridX || center.z < 0 || center.z > gridY)
                return true;
            
            var shifted = center + shift;
            if (shifted.x < 0 || shifted.x > gridY || shifted.z < 0 || shifted.z > gridY)
                return false;

            return true;
        }

        var screenPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);// new Vector3(0.5f, 0.5f);
        if (screenPos.x <= 0.02 || Input.GetButton("Left"))
        {
            var shift = transform.right * cameraMoveSpeed * cameraSpeedFactor * Time.deltaTime * -1;
            if (IsValidMovement(shift))
                transform.position += shift;
        }
        else if (screenPos.x >= 0.98 || Input.GetButton("Right"))
        {
            var shift = transform.right * cameraMoveSpeed * cameraSpeedFactor * Time.deltaTime;
            if (IsValidMovement(shift))
                transform.position += shift;
        }
        if (screenPos.y <= 0.02 || Input.GetButton("Down"))
        {
            var direction = new Vector3(transform.up.x, 0, transform.up.z);
            float correction = 1 / (Mathf.Abs(transform.up.x) + Mathf.Abs(transform.up.z));
            var shift = direction * correction * cameraMoveSpeed * cameraSpeedFactor * Time.deltaTime * -1;
            if (IsValidMovement(shift))
            {
                transform.position += shift;
            }
        }
        else if (screenPos.y >= 0.98 || Input.GetButton("Up"))
        {
            var direction = new Vector3(transform.up.x, 0, transform.up.z);
            float correction = 1 / (Mathf.Abs(transform.up.x) + Mathf.Abs(transform.up.z));
            var shift = direction * correction * cameraMoveSpeed * cameraSpeedFactor * Time.deltaTime;
            if (IsValidMovement(shift))
            {
                transform.position += shift;
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
            if (zoom.y < 30f)
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

            if ((transform.eulerAngles.x < 30 || transform.eulerAngles.x > 330) &&
                ((Mathf.Abs(transform.eulerAngles.z) < 0.001f && dy == -1) ||
                (Mathf.Abs(transform.eulerAngles.z - 180) < 0.001f && dy == 1)))
                return;

            transform.RotateAround(rotationPoint, transform.right, dy * cameraRotationSpeed * Time.deltaTime);
        }
    }

    public void MoveCameraViaMinimap(Vector3 minimapPos)
    {
        float shiftX = Mathf.InverseLerp(0, 120, minimapPos.x);
        float shiftY = Mathf.InverseLerp(0, 120, minimapPos.z);
        transform.position = new Vector3(gridX * shiftX, transform.position.y, gridY * shiftY);
    }
}
