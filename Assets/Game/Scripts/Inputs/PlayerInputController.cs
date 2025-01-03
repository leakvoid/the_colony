using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    BottomPanelController bpc;

    void Awake()
    {
        bpc = FindObjectOfType<BottomPanelController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
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
}
