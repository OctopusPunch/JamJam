using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class TownResourceBehaviour : MonoBehaviour
{
    public int CurretFoodValue => currentFoodValue;
    public int CurrentWaterValue => currentWaterValue;
    public int CurrentHungerValue => currentHungerValue;
    public int CurrentGoldValue => currentGoldValue;

    public int TargetFoodValue => targetFoodValue;
    public int TargetWaterValue => targetWaterValue;
    public int TargetGoldValue => targetGoldValue;


    public static TownResourceBehaviour Instance => _instance;
    private static TownResourceBehaviour _instance;

    [Header("Food Values")]
    [SerializeField]
    private Image foodMeter;
    [SerializeField]
    private TMPro.TMP_Text foodValueDisplay;
    [SerializeField]
    private TMPro.TMP_Text foodTargetDisplay;
    [SerializeField]
    private int targetFoodValue = 99;
    private int currentFoodValue;


    [Space(10)]
    [Header("Water Values")]
    [SerializeField]
    private Image waterMeter;
    [SerializeField]
    private TMPro.TMP_Text waterValueDisplay;
    [SerializeField]
    private TMPro.TMP_Text waterTargetDisplay;
    [SerializeField]
    private int targetWaterValue = 99;
    private int currentWaterValue;


    [Space(10)]
    [Header("Gold Values")]
    [SerializeField]
    private Image goldMeter;
    [SerializeField]
    private TMPro.TMP_Text goldValueDisplay;
    [SerializeField]
    private TMPro.TMP_Text goldTargetDisplay;
    [SerializeField]
    private int targetGoldValue = 99;
    private int currentGoldValue;


    [Space(10)]
    [Header("Monster Hunger Values")]
    [SerializeField]
    private Image hungerMeter;
    [SerializeField]
    private TMPro.TMP_Text hungerValueDisplay;
    private int currentHungerValue;


    [SerializeField]
    CanvasGroup canvas;

    public SimpleAnimationEnumerator bellringerAnimator;
    public SimpleAnimationEnumerator chestAnimator;

    void Awake()
    {
        canvas.alpha = 0;
        _instance = this;
        ResetResources();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public void ResetResources()
    {
        ResetWaterMeter();

        ResetFoodMeter();

        ResetHungerMeter();

        ResetGoldMeter();



        targetFoodValue = 5;
        targetGoldValue = 5;
        targetWaterValue = 5;

        SetTargetText();
        hungerValueDisplay.text = "3";
        goldValueDisplay.text = "0";
        waterValueDisplay.text = "0";
        foodValueDisplay.text = "0";
    }

    void Update()
    {
        if(GameManager.Instance == null)
        {
            return;
        }
        if(GameManager.Instance.State == GameManager.GameState.ShowingWave || GameManager.Instance.State == GameManager.GameState.ShowNewWave)
        {
            canvas.alpha = .35f;
            SetDisplayText();
            return;
        }
        if(GameManager.Instance.State != GameManager.GameState.In_Game)
        {
            canvas.alpha = 0;
            return;
        }

        canvas.alpha = 1.0f;
        SetDisplayText();
    }

    public void SetTargetText()
    {
        foodTargetDisplay.text = TargetFoodValue.ToString();
        goldTargetDisplay.text = TargetGoldValue.ToString();
        waterTargetDisplay.text = TargetWaterValue.ToString();
    }

    void SetDisplayText()
    {
        hungerValueDisplay.text = CurrentHungerValue.ToString();
        goldValueDisplay.text = CurrentGoldValue.ToString();
        waterValueDisplay.text = CurrentWaterValue.ToString();
        foodValueDisplay.text = CurretFoodValue.ToString();
    }

    public void SetTargetWaterValue(int value)
    {
        targetWaterValue = value;
    }

    public void AdjustWaterResource(int value)
    {
        currentWaterValue += value;
        if(currentWaterValue < 0)
        {
            currentWaterValue = 0;
        }
        waterMeter.fillAmount = (float)currentWaterValue / (float)targetWaterValue;
    }

    public void ResetWaterMeter()
    {
        currentWaterValue = 0;
        waterMeter.fillAmount = 0;
    }


    public void SetTargetFoodValue(int value)
    {
        targetFoodValue = value;
    }
    public void AdjustFoodResource(int value)
    {
        currentFoodValue += value;
        if (currentFoodValue < 0)
        {
            currentFoodValue = 0;
        }
        foodMeter.fillAmount = (float)currentFoodValue / (float)targetFoodValue;
    }

    public void ResetFoodMeter()
    {
        currentFoodValue = 0;
        foodMeter.fillAmount = 0;
    }

    public void AdjustHungerMeter(int value)
    {
        currentHungerValue += value;
        if(currentHungerValue < 0)
        {
            currentHungerValue = 0;
        }
        GameManager.Instance.WaveManager.ForceRun();
        hungerMeter.fillAmount = (float)currentHungerValue / 3;
        if (TownResourceBehaviour.Instance.CurrentHungerValue <= 0)
        {
            GameManager.Instance.SetToLose();
            return;
        }
        GameManager.Instance.WaveManager.ForceRun();
        if (GameManager.Instance.State != GameState.Bust && GameManager.Instance.State != GameState.NotEnough)
        {
            GameManager.Instance.RunCheckScore();
        }
        else
        {
            GameManager.Instance.WaveManager.ReleaseNewWave();
        }
    }

    public void CheckWinOrFail()
    {
        if(currentFoodValue == targetFoodValue && currentGoldValue == targetGoldValue && currentWaterValue == targetWaterValue)
        {
            GameManager.Instance.WaveManager.PerfectRun();
            GameManager.Instance.RunCheckScore();
            return;
        }
        if(currentFoodValue > targetFoodValue)
        {
            GameManager.Instance.SetToBust();
            TownResourceBehaviour.Instance.AdjustHungerMeter(-1);
            Debug.Log("B");
            return;
        }
        if(currentGoldValue > targetGoldValue)
        {
            GameManager.Instance.SetToBust();
            TownResourceBehaviour.Instance.AdjustHungerMeter(-1);
            Debug.Log("C");
            // fail
            return;
        }
        if(currentWaterValue > targetWaterValue)
        {
            GameManager.Instance.SetToBust();
            TownResourceBehaviour.Instance.AdjustHungerMeter(-1);
            Debug.Log("D");
            // fail
            return;
        }

        Debug.Log("F");

    }

    public bool CheckWinNoBonus()
    {

        if (currentFoodValue >= targetFoodValue - 1 && currentFoodValue <= targetFoodValue &&
            currentGoldValue >= targetGoldValue - 1 && currentGoldValue <= targetGoldValue &&
            currentWaterValue >= targetWaterValue - 1 && currentWaterValue <= targetWaterValue)
        {

            GameManager.Instance.RunCheckScore();
            return true;
        }
        return false;
    }

    public void ResetHungerMeter()
    {
        currentHungerValue = 3;
        hungerMeter.fillAmount = 1;
    }

    public void SetTargetGoldValue(int value)
    {
        targetGoldValue = value;
    }

    public void AdjustGoldMeter(int value)
    {
        currentGoldValue += value;
        if(currentGoldValue < 0)
        {
            currentGoldValue = 0;
        }
        goldMeter.fillAmount = (float)currentGoldValue / (float)targetGoldValue;
    }

    public void ResetGoldMeter()
    {
        currentGoldValue = 0;
        goldMeter.fillAmount = 0;
    }
}