using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    BottomPanelController bpc;
    MainCameraController mcc;

    [SerializeField] Camera minimapCamera;

    void Awake()
    {
        bpc = FindObjectOfType<BottomPanelController>();
        mcc = FindObjectOfType<MainCameraController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var minimapPos = minimapCamera.ScreenToViewportPoint(Input.mousePosition);
            if (minimapPos.x <= 0.6 && minimapPos.y <= 0.6)
            {
                //print("main ScreenToViewportPoint: " + Camera.main.ScreenToViewportPoint(Input.mousePosition));
                //print("minimap ScreenToViewportPoint: " + minimapCamera.ScreenToViewportPoint(Input.mousePosition));
                //print("main ScreenToWorldPoint: " + Camera.main.ScreenToWorldPoint(Input.mousePosition));
                //print("minimap ScreenToWorldPoint: " + minimapCamera.ScreenToWorldPoint(Input.mousePosition));
                //print("main ScreenPointToRay: " + Camera.main.ScreenPointToRay(Input.mousePosition));
                //print("minimap ScreenPointToRay: " + minimapCamera.ScreenPointToRay(Input.mousePosition));
                mcc.MoveCameraViaMinimap(minimapCamera.ScreenToWorldPoint(Input.mousePosition));
            }
            else
            {
                SelectGameObject();
            }

        }
    }

    void SelectGameObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool stateChanged = false;
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.transform.name == "StoneDepositMesh(Clone)")
            {
                bpc.ShowDepositInfoPanel(ResourceType.Stone);
                stateChanged = true;
            }
            else if (hit.transform.name == "IronDepositMesh(Clone)")
            {
                bpc.ShowDepositInfoPanel(ResourceType.Iron);
                stateChanged = true;
            }
            else if (hit.transform.name == "SaltDepositMesh(Clone)")
            {
                bpc.ShowDepositInfoPanel(ResourceType.Salt);
                stateChanged = true;
            }
            else if (hit.transform.parent && hit.transform.parent.transform.parent)
            {
                var selectedObject = hit.transform.parent.transform.parent;

                if (selectedObject.name == "BuildingData(Clone)")
                {
                    bpc.ShowBuildingInfoPanel(selectedObject.GetComponent<BuildingData>());
                    stateChanged = true;
                }
                else if (selectedObject.name == "ColonistData(Clone)")
                {
                    bpc.ShowColonistInfoPanel(selectedObject.GetComponent<ColonistData>());
                    stateChanged = true;
                }
            }
        }
        
        if (!stateChanged)
        {
            bpc.ShowSecondaryResourcePanel();
        }
    }
}
