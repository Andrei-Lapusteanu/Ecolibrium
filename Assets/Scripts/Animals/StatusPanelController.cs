using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusPanelController : MonoBehaviour
{
    // Title
    Image animalIcon;
    TextMeshProUGUI animalTitleTmp;
    TextMeshProUGUI animalStateTmp;

    // HP panel
    Slider hpSlider;
    TextMeshProUGUI hpValueTmp;

    // Hunger panel
    Slider satietySlider;
    TextMeshProUGUI satietyValueTmp;

    // Tiredness panel
    Slider energySlider;
    TextMeshProUGUI energyValueTmp;

    // Speed panel
    Slider speedSlider;
    TextMeshProUGUI speedValueTmp;

    // Age panel
    Slider ageSlider;
    TextMeshProUGUI ageValueTmp;

    // Reproduce panel
    Slider reproduceSlider;
    TextMeshProUGUI reproduceValueTmp;

    // FoodSight panel
    TextMeshProUGUI foodSightValueTmp;

    int id;
    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        // Title
        AnimalTitleTmp = transform.Find("TitlePanel/TitleTextPanel/AnimalTitleText").GetComponent<TextMeshProUGUI>();
        AnimalStateTmp = gameObject.transform.Find("TitlePanel/TitleTextPanel/AnimalTitleState").GetComponent<TextMeshProUGUI>();
        AnimalIcon = gameObject.transform.Find("TitlePanel/AnimalImage").GetComponent<Image>();

        // HP panel
        HpSlider = gameObject.transform.Find("PanelHP/HPBar").GetComponent<Slider>();
        HpValueTmp = gameObject.transform.Find("PanelHP/HPValue").GetComponent<TextMeshProUGUI>();

        // Hunger panel
        SatietySlider = gameObject.transform.Find("PanelSatiety/SatietyBar").GetComponent<Slider>();
        SatietyValueTmp = gameObject.transform.Find("PanelSatiety/SatietyValue").GetComponent<TextMeshProUGUI>();

        // Tiredness panel
        EnergySlider = gameObject.transform.Find("PanelEnergy/EnergyBar").GetComponent<Slider>();
        EnergyValueTmp = gameObject.transform.Find("PanelEnergy/EnergyValue").GetComponent<TextMeshProUGUI>();

        // Speed panel
        SpeedSlider = gameObject.transform.Find("PanelSpeed/SpeedBar").GetComponent<Slider>();
        SpeedValueTmp = gameObject.transform.Find("PanelSpeed/SpeedValue").GetComponent<TextMeshProUGUI>();

        // Age panel
        AgeSlider = gameObject.transform.Find("PanelAge/AgeBar").GetComponent<Slider>();
        AgeValueTmp = gameObject.transform.Find("PanelAge/AgeValue").GetComponent<TextMeshProUGUI>();

        // Reproduce panel
        ReproduceSlider = gameObject.transform.Find("PanelReproduce/ReproduceBar").GetComponent<Slider>();
        ReproduceValueTmp = gameObject.transform.Find("PanelReproduce/ReproduceValue").GetComponent<TextMeshProUGUI>();

        // FoodSight panel
        FoodSightValueTmp = gameObject.transform.Find("PanelFoodSight/FoodSightValue").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static StatusPanelController InstantiateStatsPanel()
    {
        return Instantiate(Resources.Load("_Mine/StatsPanel") as GameObject, GameObject.Find("MainCanvas").transform, false).GetComponent<StatusPanelController>();
    }

    public static void UpdateStaticElementsOnce(StatusPanelController spc, Sprite icon, string title)
    {
        spc.AnimalIcon.sprite = icon;
        spc.AnimalTitleTmp.text = title;
    }

    public static void UpdatePanelStatus(StatusPanelController spc, StatusType sType, GameObject obj, float maxValue)
    {
        if (obj == null)
            spc.SpeedValueTmp.text = "NULL";
        else
            spc.SpeedValueTmp.text = obj.name;

    }
    public static void UpdatePanelStatus(StatusPanelController spc, StatusType sType, float value, float maxValue)
    {
        switch (sType)
        {
            case StatusType.HP:
                spc.HpValueTmp.text = value.ToString("0") + "/" + maxValue.ToString("0");
                spc.HpSlider.value = value / maxValue;
                break;

            case StatusType.Satiety:
                spc.SatietyValueTmp.text = value.ToString("0") + "/" + maxValue.ToString("0");
                spc.SatietySlider.value = value / maxValue;
                break;

            case StatusType.Energy:
                spc.EnergyValueTmp.text = value.ToString("0") + "/" + maxValue.ToString("0");
                spc.EnergySlider.value = value / maxValue;
                break;

            case StatusType.Speed:
                spc.SpeedValueTmp.text = value.ToString("0.00") + "/" + maxValue.ToString("0.00");
                spc.SpeedSlider.value = value / maxValue;
                break;

            case StatusType.Age:
                spc.AgeValueTmp.text = value.ToString("0.0") + "/" + maxValue.ToString("0.0");
                spc.AgeSlider.value = value / maxValue;
                break;

            case StatusType.Reproduce:
                spc.ReproduceValueTmp.text = value.ToString("0.0") + "/" + maxValue.ToString("0.0");
                spc.ReproduceSlider.value = value / maxValue;
                break;

            case StatusType.FoodSight:
                spc.FoodSightValueTmp.text = value.ToString("0.0");
                break;
        }
    }

    public static void UpdatePanelAnimalState(StatusPanelController spc, string state)
    {
        spc.AnimalStateTmp.text = state;
    }

    public void Hide()
    {
        IsActive = false;
        gameObject.SetActive(false);
    }

    public void Unhide()
    {
        IsActive = true;
        gameObject.SetActive(true);
    }

    public static void HideAll()
    {
        foreach (Transform child in GameObject.Find("MainCanvas").transform)
            if (child.GetComponent<StatusPanelController>() != null)
                child.GetComponent<StatusPanelController>().Hide();
    }

    public Image AnimalIcon { get => animalIcon; set => animalIcon = value; }
    public TextMeshProUGUI AnimalTitleTmp { get => animalTitleTmp; set => animalTitleTmp = value; }
    public TextMeshProUGUI AnimalStateTmp { get => animalStateTmp; set => animalStateTmp = value; }
    public Slider HpSlider { get => hpSlider; set => hpSlider = value; }
    public TextMeshProUGUI HpValueTmp { get => hpValueTmp; set => hpValueTmp = value; }
    public Slider SatietySlider { get => satietySlider; set => satietySlider = value; }
    public TextMeshProUGUI SatietyValueTmp { get => satietyValueTmp; set => satietyValueTmp = value; }
    public Slider EnergySlider { get => energySlider; set => energySlider = value; }
    public TextMeshProUGUI EnergyValueTmp { get => energyValueTmp; set => energyValueTmp = value; }
    public Slider SpeedSlider { get => speedSlider; set => speedSlider = value; }
    public TextMeshProUGUI SpeedValueTmp { get => speedValueTmp; set => speedValueTmp = value; }
    public Slider AgeSlider { get => ageSlider; set => ageSlider = value; }
    public TextMeshProUGUI AgeValueTmp { get => ageValueTmp; set => ageValueTmp = value; }
    public Slider ReproduceSlider { get => reproduceSlider; set => reproduceSlider = value; }
    public TextMeshProUGUI ReproduceValueTmp { get => reproduceValueTmp; set => reproduceValueTmp = value; }
    public TextMeshProUGUI FoodSightValueTmp { get => foodSightValueTmp; set => foodSightValueTmp = value; }
    public bool IsActive { get => isActive; set => isActive = value; }
}
