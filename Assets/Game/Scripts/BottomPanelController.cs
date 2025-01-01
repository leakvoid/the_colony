using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BottomPanelController : MonoBehaviour
{
    Globals globals;
    ColonistManager cm;

    [SerializeField] TextMeshProUGUI woodText;
    [SerializeField] TextMeshProUGUI stoneText;
    [SerializeField] TextMeshProUGUI toolsText;
    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] TextMeshProUGUI ironText;
    [SerializeField] TextMeshProUGUI cottonText;
    [SerializeField] TextMeshProUGUI wheatText;
    [SerializeField] TextMeshProUGUI hopsText;
    [SerializeField] TextMeshProUGUI flourText;

    [SerializeField] TextMeshProUGUI saltText;
    [SerializeField] TextMeshProUGUI clothText;
    [SerializeField] TextMeshProUGUI meatText;
    [SerializeField] TextMeshProUGUI fishText;
    [SerializeField] TextMeshProUGUI breadText;
    [SerializeField] TextMeshProUGUI beerText;

    [SerializeField] TextMeshProUGUI citizenText;

    [SerializeField] TextMeshProUGUI fpsText;
    float deltaTime;

    void Awake()
    {
        globals = FindObjectOfType<Globals>();
        cm = FindObjectOfType<ColonistManager>();
    }
    
    // TODO inefficient, events or every second
    void Update()
    {
        woodText.text = globals.woodAmount.ToString("0");
        stoneText.text = globals.stoneAmount.ToString("0");
        toolsText.text = globals.toolsAmount.ToString("0");
        goldText.text = globals.goldAmount.ToString("0");

        ironText.text = globals.ironAmount.ToString("0");
        cottonText.text = globals.cottonAmount.ToString("0");
        wheatText.text = globals.wheatAmount.ToString("0");
        hopsText.text = globals.hopsAmount.ToString("0");
        flourText.text = globals.flourAmount.ToString("0");

        saltText.text = globals.saltAmount.ToString("0");
        clothText.text = globals.clothAmount.ToString("0");
        meatText.text = globals.meatAmount.ToString("0");
        fishText.text = globals.fishAmount.ToString("0");
        breadText.text = globals.breadAmount.ToString("0");
        beerText.text = globals.beerAmount.ToString("0");

        citizenText.text = cm.GetColonists().Count.ToString("0");

        // fps
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		float fps = 1.0f / deltaTime;
		fpsText.text = Mathf.Ceil (fps).ToString ();
    }
}
