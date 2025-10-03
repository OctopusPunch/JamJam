using UnityEngine;
using UnityEngine.UI;

public class TownResourceBehaviour : MonoBehaviour
{
    public float CurretFoodValue => currentFoodValue;
    public float CurrentHappinessValue => currentHappinessValue;
    public float CurrentHungerValue => currentHungerValue;


    public static TownResourceBehaviour Instance => _instance;
    private static TownResourceBehaviour _instance;

    [Header("Food Values")]
    [SerializeField]
    private Image foodMeter;
    [SerializeField]
    private TMPro.TMP_Text foodValueDisplay;
    [SerializeField]
    private float maxFoodValue = 99;
    [SerializeField]
    private float startFoodValue = 70;
    private float currentFoodValue;

    private float foodDrainSpeed;


    [Space(10)]
    [Header("Happiness Values")]
    [SerializeField]
    private Image happinessMeter;
    [SerializeField]
    private TMPro.TMP_Text happinessValueDisplay;
    [SerializeField]
    private float maxHappinessValue = 99;
    [SerializeField]
    private float startHappinessValue = 70;
    private float currentHappinessValue;

    private float happinessDrainSpeed;


    [Space(10)]
    [Header("Monster Hunger Values")]
    [SerializeField]
    private Image hungerMeter;
    [SerializeField]
    private TMPro.TMP_Text hungerValueDisplay;
    [SerializeField]
    private float maxHungerValue = 99;
    [SerializeField]
    private float startHungerValue = 50;
    private float currentHungerValue;

    private float hungerDrainSpeed;

    [Space(10)]
    [Header("Resource Drain Values")]
    [SerializeField]
    private float slowResourceDrain = .4f;

    [SerializeField]
    private float normalResourceDrain = .8f;

    [SerializeField]
    private float fastResourceDrain = 1.5f;

    [SerializeField]
    CanvasGroup canvas;

    void Awake()
    {
        canvas.alpha = 0;
        _instance = this;
        InitialiseResources();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void InitialiseResources()
    {
        currentHappinessValue = startHappinessValue;
        happinessDrainSpeed = normalResourceDrain;

        currentFoodValue = startFoodValue;
        foodDrainSpeed = normalResourceDrain;

        currentHungerValue = startHungerValue;
        hungerDrainSpeed = normalResourceDrain;
    }

    void Update()
    {
        if(GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            canvas.alpha = 0;
            return;
        }

        canvas.alpha = 1.0f;

        DrainFoodOverTime();
        DrainHappinessOverTime();
        DrainHungerOverTime();
    }


    void DrainFoodOverTime()
    {
        float happinessRatio = currentHappinessValue / maxHappinessValue;
        float speedMultiplier = 1f + Mathf.Pow(happinessRatio, 2);       
        float adjustedFoodDrain = foodDrainSpeed * speedMultiplier;

        currentFoodValue -= adjustedFoodDrain * Time.deltaTime;
        if (currentFoodValue < 0)
        {
            currentFoodValue = 0;
        }
        foodMeter.fillAmount = currentFoodValue / maxFoodValue;
        foodValueDisplay.text = currentFoodValue.ToString("0");
    }

    void DrainHappinessOverTime()
    {
        float hungerRatio = currentHungerValue / maxHungerValue;
        float speedMultiplier = 1f + Mathf.Pow(hungerRatio, 2);
        float adjustedHappinessDrain = happinessDrainSpeed * speedMultiplier;
        Debug.Log("adjustedHappinessDrain: " + adjustedHappinessDrain);
        currentHappinessValue -= adjustedHappinessDrain * Time.deltaTime;
        if (currentHappinessValue < 0)
        {
            currentHappinessValue = 0;
        }
        happinessMeter.fillAmount = currentHappinessValue / maxHappinessValue;
        happinessValueDisplay.text = currentHappinessValue.ToString("0");
    }

    void DrainHungerOverTime()
    {
        float foodRatio = currentFoodValue / maxFoodValue;
        float happinessRatio = currentHappinessValue / maxHappinessValue;
        float speedMultiplier = Mathf.Pow(foodRatio, 2) + Mathf.Pow(happinessRatio, 2);
        float adjustedHungerDrain = hungerDrainSpeed * speedMultiplier;

        currentHungerValue -= adjustedHungerDrain * Time.deltaTime;
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