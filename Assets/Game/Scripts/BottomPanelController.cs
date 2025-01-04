using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanelController : MonoBehaviour
{
    Globals globals;
    ColonistManager cm;

    // strategic resources
    [SerializeField] TextMeshProUGUI woodText;
    [SerializeField] TextMeshProUGUI stoneText;
    [SerializeField] TextMeshProUGUI toolsText;
    [SerializeField] TextMeshProUGUI goldText;

    // secondary resources
    [SerializeField] TextMeshProUGUI ironText;
    [SerializeField] TextMeshProUGUI cottonText;
    [SerializeField] TextMeshProUGUI wheatText;
    [SerializeField] TextMeshProUGUI hopsText;
    [SerializeField] TextMeshProUGUI flourText;
    [SerializeField] TextMeshProUGUI citizenText;

    [SerializeField] TextMeshProUGUI saltText;
    [SerializeField] TextMeshProUGUI clothText;
    [SerializeField] TextMeshProUGUI meatText;
    [SerializeField] TextMeshProUGUI fishText;
    [SerializeField] TextMeshProUGUI breadText;
    [SerializeField] TextMeshProUGUI beerText;

    // colonist info / house panel
    [SerializeField] TextMeshProUGUI typeOccupationText;

    [SerializeField] Slider sleepNeedSlider;
    [SerializeField] Slider foodNeedSlider;
    [SerializeField] Slider waterNeedSlider;

    [SerializeField] Slider clothesNeedSlider;
    [SerializeField] Slider religionNeedSlider;

    [SerializeField] Slider beerNeedSlider;
    [SerializeField] Slider saltNeedSlider;

    // panels
    [SerializeField] GameObject secondaryResourcePanel;
    [SerializeField] GameObject buildingInfoPanel;
    [SerializeField] GameObject colonistInfoPanel;

    // fps
    [SerializeField] TextMeshProUGUI fpsText;
    float deltaTime;

    BuildingData building;
    ColonistData colonist;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        cm = FindObjectOfType<ColonistManager>();
    }
    
    // TODO inefficient, events or every second
    void Update()
    {
        UpdateStrategicResourcePanel();
        UpdateSecondaryResourcePanel();
        UpdateColonistInfoPanel();
        UpdateBuildingInfoPanel();

        // fps
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		float fps = 1.0f / deltaTime;
		fpsText.text = Mathf.Ceil (fps).ToString ();
    }

    void UpdateStrategicResourcePanel()
    {
        woodText.text = globals.woodAmount.ToString("0");
        stoneText.text = globals.stoneAmount.ToString("0");
        toolsText.text = globals.toolsAmount.ToString("0");
        goldText.text = globals.goldAmount.ToString("0");
    }

    void UpdateSecondaryResourcePanel()
    {
        if (secondaryResourcePanel.activeSelf)
        {
            ironText.text = globals.ironAmount.ToString("0");
            cottonText.text = globals.cottonAmount.ToString("0");
            wheatText.text = globals.wheatAmount.ToString("0");
            hopsText.text = globals.hopsAmount.ToString("0");
            flourText.text = globals.flourAmount.ToString("0");
            citizenText.text = cm.GetColonists().Count.ToString("0");

            saltText.text = globals.saltAmount.ToString("0");
            clothText.text = globals.clothAmount.ToString("0");
            meatText.text = globals.meatAmount.ToString("0");
            fishText.text = globals.fishAmount.ToString("0");
            breadText.text = globals.breadAmount.ToString("0");
            beerText.text = globals.beerAmount.ToString("0");
        }
    }

    void UpdateColonistInfoPanel()
    {
        if (colonistInfoPanel.activeSelf)
        {
            string descriptionText;
            int sleepNeedMeter = 0;
            int foodNeedMeter = 0;
            int waterNeedMeter = 0;
            int clothesNeedMeter = 0;
            int religionNeedMeter = 0;
            int beerNeedMeter = 0;
            int saltNeedMeter = 0;
            ColonistData.Type colonistType;

            if (colonist)
            {
                descriptionText = colonist.type.ToString() + " (" + colonist.occupation.ToString() + ")";
                sleepNeedMeter = colonist.SleepNeedMeter;
                foodNeedMeter = colonist.FoodNeedMeter;
                waterNeedMeter = colonist.WaterNeedMeter;
                clothesNeedMeter = colonist.ClothesNeedMeter;
                religionNeedMeter = colonist.ReligionNeedMeter;
                beerNeedMeter = colonist.BeerNeedMeter;
                saltNeedMeter = colonist.SaltNeedMeter;
                colonistType = colonist.type;
            }
            else
            {
                if (building.upgradeTier == 0)
                    descriptionText = "Tent (" + globals.HouseTemplate.Tier0ColonistCapacity + " peasants)";
                else if (building.upgradeTier == 1)
                    descriptionText = "Wooden house (" + globals.HouseTemplate.Tier1ColonistCapacity + " citizens)";
                else
                    descriptionText = "Stone house (" + globals.HouseTemplate.Tier2ColonistCapacity + " nobleman)";

                foreach (var colonistInside in building.colonists)
                {
                    sleepNeedMeter += colonistInside.SleepNeedMeter;
                    foodNeedMeter += colonistInside.FoodNeedMeter;
                    waterNeedMeter += colonistInside.WaterNeedMeter;
                    clothesNeedMeter += colonistInside.ClothesNeedMeter;
                    religionNeedMeter += colonistInside.ReligionNeedMeter;
                    beerNeedMeter += colonistInside.BeerNeedMeter;
                    saltNeedMeter += colonistInside.SaltNeedMeter;
                }
                sleepNeedMeter /= building.colonists.Count;
                foodNeedMeter /= building.colonists.Count;
                waterNeedMeter /= building.colonists.Count;
                clothesNeedMeter /= building.colonists.Count;
                religionNeedMeter /= building.colonists.Count;
                beerNeedMeter /= building.colonists.Count;
                saltNeedMeter /= building.colonists.Count;
                colonistType = building.colonists[0].type;
            }

            typeOccupationText.text = descriptionText;

            sleepNeedSlider.value = sleepNeedMeter;
            foodNeedSlider.value = foodNeedMeter;
            waterNeedSlider.value = waterNeedMeter;

            if (colonistType != ColonistData.Type.Peasant)
            {
                clothesNeedSlider.value = clothesNeedMeter;
                religionNeedSlider.value = religionNeedMeter;

                clothesNeedSlider.transform.parent.gameObject.SetActive(true);
                religionNeedSlider.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                clothesNeedSlider.transform.parent.gameObject.SetActive(false);
                religionNeedSlider.transform.parent.gameObject.SetActive(false);
            }

            if (colonistType == ColonistData.Type.Nobleman)
            {
                beerNeedSlider.value = beerNeedMeter;
                saltNeedSlider.value = saltNeedMeter;

                beerNeedSlider.transform.parent.gameObject.SetActive(true);
                saltNeedSlider.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                beerNeedSlider.transform.parent.gameObject.SetActive(false);
                saltNeedSlider.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    void UpdateBuildingInfoPanel()
    {

    }

    public void ShowBuildingInfoPanel(BuildingData buildingData)
    {
        if (buildingData.template.BuildingTag != BuildingTag.House || !buildingData.isConstructed)
        {
            buildingInfoPanel.SetActive(true);
            colonistInfoPanel.SetActive(false);
        }
        else
        {
            buildingInfoPanel.SetActive(false);
            colonistInfoPanel.SetActive(true);
        }
        secondaryResourcePanel.SetActive(false);

        building = buildingData;
        colonist = null;
    }

    public void ShowColonistInfoPanel(ColonistData colonistData)
    {
        buildingInfoPanel.SetActive(false);
        colonistInfoPanel.SetActive(true);
        secondaryResourcePanel.SetActive(false);

        colonist = colonistData;
        building = null;
    }

    public void ShowSecondaryResourcePanel()
    {
        buildingInfoPanel.SetActive(false);
        colonistInfoPanel.SetActive(false);
        secondaryResourcePanel.SetActive(true);

        building = null;
        colonist = null;
    }
}
