using UnityEngine;
using UnityEngine.UI;

public class TownResourceBehaviour : MonoBehaviour
{

    public static TownResourceBehaviour Instance => _instance;
    private static TownResourceBehaviour _instance;

    [Header("Food Values")]
    [SerializeField]
    private Image foodMeter;
    [SerializeField]
    private TMPro.TMP_Text foodValueDisplay;
    [SerializeField]
    private float maxFoodValue = 50;
    private float currentFoodValue;

    private float foodDrainSpeed;


    [Space(10)]
    [Header("Water Values")]
    [SerializeField]
    private Image waterMeter;
    [SerializeField]
    private TMPro.TMP_Text waterValueDisplay;
    [SerializeField]
    private float maxWaterValue = 50;
    private float currentWaterValue;

    private float waterDrainSpeed;

    [Space(10)]
    [Header("Resource Drain Values")]
    [SerializeField]
    private float slowResourceDrain = .1f;

    [SerializeField]
    private float normalResourceDrain = .3f;

    [SerializeField]
    private float fastResourceDrain = 1;

    void Awake()
    {
        _instance = this;
        InitialiseResources();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void InitialiseResources()
    {
        currentWaterValue = maxWaterValue;
        waterDrainSpeed = normalResourceDrain;

        currentFoodValue = maxFoodValue;
        foodDrainSpeed = normalResourceDrain;
    }

    void Update()
    {
        // ToDo Check Game is not paused or on main menu

        DrainFoodOverTime();
        DrainWaterOverTime();
    }


    void DrainFoodOverTime()
    {
        currentFoodValue -= foodDrainSpeed * Time.deltaTime;
        if (currentFoodValue < 0)
        {
            currentFoodValue = 0;
        }
        foodMeter.fillAmount = currentFoodValue / maxFoodValue;
        foodValueDisplay.text = currentFoodValue.ToString("0");
    }

    void DrainWaterOverTime()
    {
        currentWaterValue -= waterDrainSpeed * Time.deltaTime;
        if (currentWaterValue < 0)
        {
            currentWaterValue = 0;
        }
        waterMeter.fillAmount = currentWaterValue / maxWaterValue;
        waterValueDisplay.text = currentWaterValue.ToString("0");
    }

    public bool UseWaterResource(float value)
    {
        if (currentWaterValue <= 0)
        {
            return false;
        }

        if (currentWaterValue < value)
        {
            return false;
        }

        currentWaterValue -= value;
        if (currentWaterValue < 0)
        {
            currentWaterValue = 0;
        }
        return true;
    }

    public void AddWaterResource(float value)
    {
        currentWaterValue += value;
        if(currentWaterValue > maxWaterValue)
        {
            currentWaterValue = maxWaterValue;
        }
    }

    public bool UseFoodResource(float value)
    {
        if (currentFoodValue <= 0)
        {
            return false;
        }

        if (currentFoodValue < value)
        {
            return false;
        }

        currentFoodValue -= value;
        if (currentFoodValue < 0)
        {
            currentFoodValue = 0;
        }
        return true;
    }

    public void AddFoodResource(float value)
    {
        currentFoodValue += value;
        if (currentFoodValue > maxFoodValue)
        {
            currentFoodValue = maxFoodValue;
        }
    }
}