using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanelController : MonoBehaviour
{
    Globals globals;
    ColonistManager cm;

    [Header("Strategic resources")]
    [SerializeField] TextMeshProUGUI woodText;
    [SerializeField] TextMeshProUGUI stoneText;
    [SerializeField] TextMeshProUGUI toolsText;
    [SerializeField] TextMeshProUGUI goldText;

    [Header("Secondary resources")]
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

    [Header("Construction panel")]
    [SerializeField] GameObject constructionGroup;
    [SerializeField] TextMeshProUGUI constructionStatusText;
    [SerializeField] TextMeshProUGUI remainingTimeText;
    [SerializeField] TextMeshProUGUI constructionDurationText;
    [SerializeField] Slider constructionProgressBar;

    [Header("Colonist and house panel")]
    [SerializeField] TextMeshProUGUI descriptionText;

    [SerializeField] Slider sleepNeedSlider;
    [SerializeField] Slider foodNeedSlider;
    [SerializeField] Slider waterNeedSlider;

    [SerializeField] Slider clothesNeedSlider;
    [SerializeField] Slider religionNeedSlider;

    [SerializeField] Slider beerNeedSlider;
    [SerializeField] Slider saltNeedSlider;

    [Header("Building info panel")]
    [SerializeField] TextMeshProUGUI buildingDescriptionText;

    [SerializeField] GameObject productionGroup;
    [SerializeField] GameObject processingGroup;
    [SerializeField] Image leftResourceImage;
    [SerializeField] TextMeshProUGUI leftResourceText;
    [SerializeField] Image rightResourceImage;
    [SerializeField] TextMeshProUGUI rightResourceText;
    [SerializeField] GameObject gatheringGroup;
    [SerializeField] Image centerResourceImage;
    [SerializeField] TextMeshProUGUI centerResourceText;

    [SerializeField] GameObject serviceGroup;
    [SerializeField] GameObject marketGroup;
    [SerializeField] Image serviceResourceImage;
    [SerializeField] TextMeshProUGUI serviceResourceText;
    [SerializeField] TextMeshProUGUI serviceFoodText;
    [SerializeField] TextMeshProUGUI serviceClothText;

    [SerializeField] TextMeshProUGUI numberOfWorkersText;
    [SerializeField] TextMeshProUGUI salaryText;

    [Header("Deposit info panel")]
    [SerializeField] TextMeshProUGUI depositNameText;
    [SerializeField] Image depositResourceImage;

    [Header("Panels")]
    [SerializeField] GameObject secondaryResourcePanel;
    [SerializeField] GameObject buildingInfoPanel;
    [SerializeField] GameObject colonistInfoPanel;
    [SerializeField] GameObject depositInfoPanel;

    [Header("FPS")]
    [SerializeField] TextMeshProUGUI fpsText;
    float deltaTime;

    BuildingData building;
    ColonistData colonist;
    ResourceType resource;

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
        UpdateDepositInfoPanel();

        // fps
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		float fps = 1.0f / deltaTime;
		fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
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

            this.descriptionText.text = descriptionText;

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
        if (buildingInfoPanel.activeSelf)
        {
            buildingDescriptionText.text = building.template.BuildingTag.ToString();

            if (!building.isConstructed)
            {
                serviceGroup.SetActive(false);
                productionGroup.SetActive(false);
                constructionGroup.SetActive(true);

                constructionDurationText.text = building.template.ConstructionTime.ToString();
                if (building.buildStartTime == 0)
                {
                    constructionStatusText.text = "Builder is coming";
                    remainingTimeText.text = building.template.ConstructionTime.ToString();
                    constructionProgressBar.value = 0;
                    return;
                }

                constructionStatusText.text = "Building";
                float timePassed = Time.time - building.buildStartTime;
                remainingTimeText.text = (building.template.ConstructionTime - (int)timePassed).ToString();
                constructionProgressBar.maxValue = building.template.ConstructionTime;
                constructionProgressBar.value = timePassed;
                return;
            }

            if (building.template.BuildingTag == BuildingTag.House)
            {
                buildingInfoPanel.SetActive(false);
                colonistInfoPanel.SetActive(true);
                return;
            }
            WorkableBT wbt = (WorkableBT)building.template;
            numberOfWorkersText.text = "workers: " + building.colonists.Count.ToString();
            salaryText.text = "salary: " + wbt.Salary.ToString();

            if (building.template.BuildingType == BuildingType.Service)
            {
                serviceGroup.SetActive(true);
                productionGroup.SetActive(false);
                constructionGroup.SetActive(false);

                switch (building.template.BuildingTag)
                {
                    case BuildingTag.Market:
                        marketGroup.SetActive(true);

                        serviceResourceImage.sprite = Resources.Load<Sprite>("Icons/powder");
                        serviceResourceText.text = globals.SaltPrice.ToString();
                        serviceFoodText.text = globals.FoodPrice.ToString();
                        serviceClothText.text = globals.ClothPrice.ToString();
                        break;
                    case BuildingTag.Church:
                        marketGroup.SetActive(false);

                        serviceResourceImage.sprite = Resources.Load<Sprite>("Icons/cross");
                        serviceResourceText.text = globals.ChurchDonation.ToString();
                        break;
                    case BuildingTag.Well:
                        marketGroup.SetActive(false);

                        serviceResourceImage.sprite = Resources.Load<Sprite>("Icons/bucket");
                        serviceResourceText.text = "0";
                        break;
                    case BuildingTag.Inn:
                        marketGroup.SetActive(false);

                        serviceResourceImage.sprite = Resources.Load<Sprite>("Icons/barrel");
                        serviceResourceText.text = globals.BeerPrice.ToString();
                        break;
                    default:
                        throw new Exception("Unknown service building");
                }
            }
            else
            {
                productionGroup.SetActive(true);
                serviceGroup.SetActive(false);
                constructionGroup.SetActive(false);

                if (building.template.BuildingType == BuildingType.Processing)
                {
                    processingGroup.SetActive(true);
                    gatheringGroup.SetActive(false);

                    ProcessingBT bt = (ProcessingBT)building.template;
                    leftResourceText.text = bt.AmountConsumedPerInterval.ToString();
                    rightResourceText.text = bt.AmountProducedPerInterval.ToString();

                    switch (bt.BuildingTag)
                    {
                        case BuildingTag.Bakery:
                            leftResourceImage.sprite = Resources.Load<Sprite>("Icons/sugar");
                            rightResourceImage.sprite = Resources.Load<Sprite>("Icons/bread");
                            break;
                        case BuildingTag.Brewery:
                            leftResourceImage.sprite = Resources.Load<Sprite>("Icons/hops");
                            rightResourceImage.sprite = Resources.Load<Sprite>("Icons/barrel");
                            break;
                        case BuildingTag.Clothier:
                            leftResourceImage.sprite = Resources.Load<Sprite>("Icons/cotton");
                            rightResourceImage.sprite = Resources.Load<Sprite>("Icons/coat");
                            break;
                        case BuildingTag.Forge:
                            leftResourceImage.sprite = Resources.Load<Sprite>("Icons/stone");
                            rightResourceImage.sprite = Resources.Load<Sprite>("Icons/tools");
                            break;
                        case BuildingTag.Windmill:
                            leftResourceImage.sprite = Resources.Load<Sprite>("Icons/wheat");
                            rightResourceImage.sprite = Resources.Load<Sprite>("Icons/sugar");
                            break;
                        default:
                            throw new Exception("Unknown processing building");
                    }
                }
                else
                {
                    processingGroup.SetActive(false);
                    gatheringGroup.SetActive(true);

                    ProductionBT bt = (ProductionBT)building.template;
                    centerResourceText.text = bt.AmountProducedPerInterval.ToString();

                    switch (bt.BuildingTag)
                    {
                        case BuildingTag.CottonPlantation:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/cotton");
                            break;
                        case BuildingTag.HopsFarm:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/hops");
                            break;
                        case BuildingTag.WheatFarm:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/wheat");
                            break;
                        case BuildingTag.FishingHut:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/fish");
                            break;
                        case BuildingTag.HuntersCabin:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/ham");
                            break;
                        case BuildingTag.IronMine:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/stone");
                            break;
                        case BuildingTag.SaltMine:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/powder");
                            break;
                        case BuildingTag.Sawmill:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/logs");
                            break;
                        case BuildingTag.StoneMine:
                            centerResourceImage.sprite = Resources.Load<Sprite>("Icons/stoneblock");
                            break;
                        default:
                            throw new Exception("Unknown production building");
                    }
                }
            }
        }
    }

    void UpdateDepositInfoPanel()
    {
        if (depositInfoPanel.activeSelf)
        {

            if (resource == ResourceType.Stone)
            {
                depositNameText.text = "Stone deposit";
                depositResourceImage.sprite = Resources.Load<Sprite>("Icons/stoneblock");
            }
            else if (resource == ResourceType.Iron)
            {
                depositNameText.text = "Iron deposit";
                depositResourceImage.sprite = Resources.Load<Sprite>("Icons/stone");
            }
            else
            {
                depositNameText.text = "Salt deposit";
                depositResourceImage.sprite = Resources.Load<Sprite>("Icons/powder");
            }
        }
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
        depositInfoPanel.SetActive(false);

        building = buildingData;
        colonist = null;
    }

    public void ShowColonistInfoPanel(ColonistData colonistData)
    {
        buildingInfoPanel.SetActive(false);
        colonistInfoPanel.SetActive(true);
        secondaryResourcePanel.SetActive(false);
        depositInfoPanel.SetActive(false);

        colonist = colonistData;
        building = null;
    }

    public void ShowSecondaryResourcePanel()
    {
        buildingInfoPanel.SetActive(false);
        colonistInfoPanel.SetActive(false);
        secondaryResourcePanel.SetActive(true);
        depositInfoPanel.SetActive(false);

        building = null;
        colonist = null;
    }

    public void ShowDepositInfoPanel(ResourceType resourceType)
    {
        buildingInfoPanel.SetActive(false);
        colonistInfoPanel.SetActive(false);
        secondaryResourcePanel.SetActive(false);
        depositInfoPanel.SetActive(true);

        building = null;
        colonist = null;
        resource = resourceType;
    }
}
