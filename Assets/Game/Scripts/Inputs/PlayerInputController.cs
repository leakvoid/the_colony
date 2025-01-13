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

        if (Physics.Raycast(ray, out hit, 100))// TODO refactor
        {
            var selectedObject = hit.transform.gameObject.transform.parent.transform.parent.gameObject;

            if (selectedObject.name == "BuildingData(Clone)")
            {
                bpc.ShowBuildingInfoPanel(selectedObject.GetComponent<BuildingData>());
            }
            else if (selectedObject.name == "ColonistData(Clone)")
            {
                bpc.ShowColonistInfoPanel(selectedObject.GetComponent<ColonistData>());
            }
            else
            {
                bpc.ShowSecondaryResourcePanel();
            }
        }
        else
        {
            bpc.ShowSecondaryResourcePanel();
        }
    }
}
