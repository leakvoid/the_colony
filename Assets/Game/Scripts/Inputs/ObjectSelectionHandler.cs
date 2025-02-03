using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelectionHandler : MonoBehaviour
{
    [SerializeField] GameObject buildingCoverArea;
    [SerializeField] GameObject selectedObjectBorder;

    public void SelectBuilding(BuildingData bd)
    {
        DeselectObject();

        selectedObjectBorder.transform.parent = null;
        selectedObjectBorder.transform.position = new Vector3(bd.gridLocation.x + (float)bd.template.SizeX / 2, 0,
            bd.gridLocation.y + (float)bd.template.SizeY / 2);
        selectedObjectBorder.transform.localScale = new Vector3(bd.template.SizeX + 0.3f, 1, bd.template.SizeY + 0.3f);

        selectedObjectBorder.transform.GetChild(0).transform.localScale = new Vector3(1, 0.05f, 0.05f / bd.template.SizeX);
        selectedObjectBorder.transform.GetChild(1).transform.localScale = new Vector3(1, 0.05f, 0.05f / bd.template.SizeX);
        selectedObjectBorder.transform.GetChild(2).transform.localScale = new Vector3(1, 0.05f, 0.05f / bd.template.SizeY);
        selectedObjectBorder.transform.GetChild(3).transform.localScale = new Vector3(1, 0.05f, 0.05f / bd.template.SizeY);
        selectedObjectBorder.SetActive(true);

        if (bd.template.BuildingType == BuildingType.Service || bd.template.BuildingType == BuildingType.ResourceGathering)
        {
            DrawCoverArea(bd);
        }
    }

    void DrawCoverArea(BuildingData bd)
    {
        int area;
        if (bd.template.BuildingType == BuildingType.Service)
        {
            ServiceBT bt = (ServiceBT)bd.template;
            area = bt.CoverArea;
        }
        else
        {
            ResourceGatheringBT bt = (ResourceGatheringBT)bd.template;
            area = bt.CaptureGatheringArea;
        }

        buildingCoverArea.transform.position = new Vector3(bd.gridLocation.x + (float)bd.template.SizeX / 2, 0,
            bd.gridLocation.y + (float)bd.template.SizeY / 2);
        
        buildingCoverArea.transform.localScale = new Vector3(area * 2 + (float)bd.template.SizeX / 2, 6,
            area * 2 + (float)bd.template.SizeY / 2);

        buildingCoverArea.SetActive(true);
    }

    public void SelectColonist(GameObject colonist)
    {
        DeselectObject();

        selectedObjectBorder.transform.position = new Vector3(colonist.transform.position.x, 0,
            colonist.transform.position.z);
        selectedObjectBorder.transform.parent = colonist.transform;
        selectedObjectBorder.transform.localScale = new Vector3(0.8f, 1, 0.8f);

        selectedObjectBorder.transform.GetChild(0).transform.localScale = new Vector3(1, 0.05f, 0.08f);
        selectedObjectBorder.transform.GetChild(1).transform.localScale = new Vector3(1, 0.05f, 0.08f);
        selectedObjectBorder.transform.GetChild(2).transform.localScale = new Vector3(1, 0.05f, 0.08f);
        selectedObjectBorder.transform.GetChild(3).transform.localScale = new Vector3(1, 0.05f, 0.08f);
        selectedObjectBorder.SetActive(true);
    }

    public void DeselectObject()
    {
        selectedObjectBorder.transform.parent = null;
        selectedObjectBorder.SetActive(false);
        buildingCoverArea.SetActive(false);
    }

    public void DisconnectBorder(GameObject model)
    {
        if (selectedObjectBorder.transform.parent && selectedObjectBorder.transform.parent.gameObject == model)
        {
            selectedObjectBorder.transform.parent = null;
            selectedObjectBorder.SetActive(false);
        }
    }
}
