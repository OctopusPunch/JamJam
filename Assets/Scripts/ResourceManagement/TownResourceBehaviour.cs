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
    [Header("Happiness Values")]
    [SerializeField]
    private Image happinessMeter;
    [SerializeField]
    private TMPro.TMP_Text happinessValueDisplay;
    [SerializeField]
    private float maxHappinessValue = 50;
    private float currentHappinessValue;

    private float happinessDrainSpeed;


    [Space(10)]
    [Header("Monster Hunger Values")]
    [SerializeField]
    private Image hungerMeter;
    [SerializeField]
    private TMPro.TMP_Text hungerValueDisplay;
    [SerializeField]
    private float maxHungerValue = 50;
    private float currentHungerValue;

    private float hungerDrainSpeed;

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
        currentHappinessValue = maxHappinessValue;
        happinessDrainSpeed = normalResourceDrain;

        currentFoodValue = maxFoodValue;
        foodDrainSpeed = normalResourceDrain;

        currentHungerValue = maxHungerValue;
        hungerDrainSpeed = normalResourceDrain;
    }

    void Update()
    {
        // ToDo Check Game is not paused or on main menu

        DrainFoodOverTime();
        DrainHappinessOverTime();
        DrainHungerOverTime();
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

    void DrainHappinessOverTime()
    {
        currentHappinessValue -= happinessDrainSpeed * Time.deltaTime;
        if (currentHappinessValue < 0)
        {
            currentHappinessValue = 0;
        }
        happinessMeter.fillAmount = currentHappinessValue / maxHappinessValue;
        happinessValueDisplay.text = currentHappinessValue.ToString("0");
    }

    void DrainHungerOverTime()
    {
        currentHungerValue -= hungerDrainSpeed * Time.deltaTime;
        if (currentHungerValue < 0)
        {
            currentHungerValue = 0;
        }
        hungerMeter.fillAmount = currentHungerValue / maxHungerValue;
        hungerValueDisplay.text = currentHungerValue.ToString("0");
    }

    public bool UseHappinessResource(float value)
    {
        if (value <= 0)
        {
            return false;
        }

        if (currentHappinessValue <= 0)
        {
            return false;
        }

        if (currentHappinessValue < value)
        {
            return false;
        }

        currentHappinessValue -= value;
        if (currentHappinessValue < 0)
        {
            currentHappinessValue = 0;
        }
        return true;
    }

    public void AddHappinessResource(float value)
    {
        currentHappinessValue += value;
        if(currentHappinessValue > maxHappinessValue)
        {
            currentHappinessValue = maxHappinessValue;
        }
    }

    public bool UseFoodResource(float value)
    {
        if (value <= 0)
        {
            return false;
        }

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


    public bool UseHungerMeter(float value)
    {
        if(value <= 0)
        {
            return false;
        }

        if (currentHungerValue <= 0)
        {
            return false;
        }

        if (currentHungerValue < value)
        {
            return false;
        }

        currentHungerValue -= value;
        if (currentHungerValue < 0)
        {
            currentHungerValue = 0;
        }
        return true;
    }

    public void AddToHungerMeter(float value)
    {
        currentHungerValue += value;
        if (currentHungerValue > maxHungerValue)
        {
            currentHungerValue = maxHungerValue;
        }
    }
}