using System;
using System.Collections;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(0)]
public class GameManager : MonoBehaviour
{
    public bool GodHandActive => godHandActive;
    public DeckManager WaveManager;

    public GameObject PauseUI => pauseUI;
    public GameObject MainMenuUI => mainMenuUI;
    public GameObject SettingsUI => settingsUI;


    public static GameManager Instance => instance;
    private static GameManager instance;

    public enum GameState
    {
        Paused,
        MainMenu,
        In_Game,
        Lose,
        Win,
        ShowNewWave,
        ShowingWave,
        RunCheckScore,
        CheckingScore
    }

    public GameState State => state;

    [SerializeField]
    private GameState state;

    [Space(10)]
    [Header("UI")]
    [SerializeField]
    private GameObject pauseUI;
    [SerializeField]
    private GameObject mainMenuUI;
    [SerializeField]
    private GameObject settingsUI;
    [SerializeField]
    private GameObject PauseButton;
    [SerializeField]
    private GameObject waveUI;
    [SerializeField]
    private GameObject winUI;
    [SerializeField]
    private GameObject loseUI;

    [Header("Wave UI")]
    [SerializeField]
    private TextMeshProUGUI waveUIWaveNumber;


    // in game stuffs
    private float spawnSubWaveInSeconds;
    private bool waveNumberUIShowing = false;
    private bool godHandActive;

    private float timeScaleDelta = 0;
    private const float deltaDuration = .35f;
    private const float targetDeltaTime = .25f;

    private int perfectMatchCount = 0;
    private int chainedPerfects = 0;
    private BigInteger score = 0;
    public BigInteger Score => score;

    private bool isFirstWave = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        state = GameState.MainMenu;
        instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyUI();

        pauseUI.SetActive(false);
        settingsUI.SetActive(false);
        PauseButton.SetActive(false);
        mainMenuUI.SetActive(false);
        WaveManager.Initialise();

        StartCoroutine(WaitForSceneFinishLoadingAndShowMainMenuAsync("InGameScene"));
    }


    public void Update()
    {

        if(waveNumberUIShowing)
        {
            return;
        }

        switch (state)
        {
            case GameState.Paused:
                return;

            case GameState.MainMenu:
                return;

            case GameState.In_Game:
                InGameUpdate();
                return;

            case GameState.Lose:
                loseUI.SetActive(true);
                return;

            case GameState.Win:
                winUI.SetActive(true);
                return;
            case GameState.ShowNewWave:
                StartCoroutine(ShowWaveUI());
                waveNumberUIShowing = true;

                state = GameState.ShowingWave;
                break;
                case GameState.ShowingWave:
                if(waveNumberUIShowing)
                {
                    return;
                }
                state = GameState.In_Game;
                break;
                case GameState.RunCheckScore:

                state = GameState.CheckingScore;
                break;
                case GameState.CheckingScore:
                CheckingScore();
                break;
        }
    }

    void InGameUpdate()
    {
        GodHandSlowDown();


        if (WaveManager.trackedNPCs.Count > 0)
        {
            return;
        }

        if (!isFirstWave)
        {
            state = GameState.RunCheckScore;
        }
        isFirstWave = false;
        WaveManager.ReleaseNewWave();

    }

    void GodHandSlowDown()
    {
        if (godHandActive)
        {
            timeScaleDelta += Time.unscaledDeltaTime;
            if (timeScaleDelta >= deltaDuration)
            {
                Time.timeScale = targetDeltaTime;
                timeScaleDelta = deltaDuration;
                return;
            }
            Time.timeScale = CubicEaseOut(timeScaleDelta / deltaDuration, 1, targetDeltaTime);
            return;
        }

        timeScaleDelta -= Time.unscaledDeltaTime;
        if (timeScaleDelta <= 0)
        {
            Time.timeScale = 1f;
            timeScaleDelta = 0;
            return;
        }

        Time.timeScale = CubicEaseOut(timeScaleDelta / deltaDuration, 1, targetDeltaTime);
    }

    void DontDestroyUI()
    {
        DontDestroyOnLoad(pauseUI);
        DontDestroyOnLoad(mainMenuUI);
        DontDestroyOnLoad(settingsUI);
        DontDestroyOnLoad(PauseButton);
        DontDestroyOnLoad(waveUI);
        DontDestroyOnLoad(winUI);
        DontDestroyOnLoad(loseUI);
    }

    public void SetGodHandActive(bool value)
    {
        godHandActive = value;
    }


    public void StartGame()
    {
        isFirstWave = true;
        waveNumberUIShowing = true;
        mainMenuUI.SetActive(false);

        state = GameState.ShowingWave;
        score = 0;
        perfectMatchCount = 0;
        chainedPerfects = 0;

        TownResourceBehaviour.Instance.ResetResources();
        WaveManager.ResetWaves();
        StartCoroutine(ShowWaveUI());
    }

    void CheckingScore()
    {
        int applyDamage = 0;
        int scoreMultiplier = 1;
        perfectMatchCount = 0;
        int addToScore = 0;
        //water
        if (TownResourceBehaviour.Instance.CurrentWaterValue > TownResourceBehaviour.Instance.TargetWaterValue)
        {
            ++applyDamage;
        }
        else if (TownResourceBehaviour.Instance.CurrentWaterValue < TownResourceBehaviour.Instance.TargetWaterValue * .8f)
        {
            ++applyDamage;
        }
        else if (TownResourceBehaviour.Instance.CurrentWaterValue == TownResourceBehaviour.Instance.TargetWaterValue)
        {
            ++perfectMatchCount;
            scoreMultiplier += perfectMatchCount;
            addToScore += TownResourceBehaviour.Instance.CurrentWaterValue;
        }
        else
        {
            addToScore += TownResourceBehaviour.Instance.CurrentWaterValue;
        }
        //end water

        // Gold
        if (TownResourceBehaviour.Instance.CurrentGoldValue > TownResourceBehaviour.Instance.TargetGoldValue)
        {
            ++applyDamage;
        }
        else if (TownResourceBehaviour.Instance.CurrentGoldValue < TownResourceBehaviour.Instance.TargetGoldValue * .8f)
        {
            ++applyDamage;
        }
        else if (TownResourceBehaviour.Instance.CurrentGoldValue == TownResourceBehaviour.Instance.TargetGoldValue)
        {
            ++perfectMatchCount;
            scoreMultiplier += perfectMatchCount;

            addToScore += TownResourceBehaviour.Instance.CurrentGoldValue;
        }
        else
        {
            addToScore += TownResourceBehaviour.Instance.CurrentGoldValue;
        }
        // End Gold

        // Food
        if (TownResourceBehaviour.Instance.CurretFoodValue > TownResourceBehaviour.Instance.TargetFoodValue)
        {
            ++applyDamage;
        }
        else if (TownResourceBehaviour.Instance.CurretFoodValue < TownResourceBehaviour.Instance.TargetFoodValue * .8f)
        {
            ++applyDamage;
        }
        else if (TownResourceBehaviour.Instance.CurretFoodValue == TownResourceBehaviour.Instance.TargetFoodValue)
        {
            ++perfectMatchCount;
            scoreMultiplier += perfectMatchCount;
            addToScore += TownResourceBehaviour.Instance.CurretFoodValue;
        }
        else
        {
            addToScore += TownResourceBehaviour.Instance.CurretFoodValue;
        }
        // End Food

        // Tally score

        if (perfectMatchCount == 3)
        {
            ++chainedPerfects;
            score += (BigInteger)addToScore * (BigInteger)(chainedPerfects > 1 ? BigInteger.Pow(scoreMultiplier, chainedPerfects) : scoreMultiplier);
        }
        else
        {
            chainedPerfects = 0;
        }


        if (perfectMatchCount > 0)
        {
            score += 5000 * perfectMatchCount;
        }

        Debug.Log("Score: " + score);
        // Check damage
        TownResourceBehaviour.Instance.AdjustHungerMeter(-applyDamage);

        if(TownResourceBehaviour.Instance.CurrentHungerValue <= 0)
        {
            state = GameState.Lose;
            return;
        }
        state = GameState.In_Game;
        TownResourceBehaviour.Instance.ResetGoldMeter();
        TownResourceBehaviour.Instance.ResetFoodMeter();
        TownResourceBehaviour.Instance.ResetWaterMeter();
    }

    public void SetToWin()
    {
        state = GameState.Win;
    }

    public void SetToLose()
    {
        state = GameState.Lose;
    }

    public void SetToInGame()
    {
        state = GameState.In_Game;
        pauseUI.SetActive(false);
        mainMenuUI.SetActive(false);
        PauseButton.SetActive(true);
    }
    public void SetToMain()
    {
        state = GameState.MainMenu;
        pauseUI.SetActive(false);
        mainMenuUI.SetActive(true);
        winUI.SetActive(false);
        loseUI.SetActive(false);
    }
    public void SetToPaused()
    {
        state = GameState.Paused;
        pauseUI.SetActive(true);
        PauseButton.SetActive(false);
    }

    public void SetToSettings() 
    {
        pauseUI.SetActive(false);
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    public void ReturnFromSettings() 
    {
        settingsUI.SetActive(false);

        if (state == GameState.Paused) 
        {
            pauseUI.SetActive(true);
        }
        else if(state == GameState.MainMenu) 
        {
            mainMenuUI.SetActive(true);
        }
    }
    public void VolumeSlider(float volume) 
    {
        AudioListener.volume = volume;
    }

    public void QuitGame() 
    {
        Application.Quit();
    }

    IEnumerator WaitForSceneFinishLoadingAndShowMainMenuAsync(string sceneToLoad)
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        sceneLoadOperation.allowSceneActivation = false;
        while (!sceneLoadOperation.isDone)
        {
            if (sceneLoadOperation.progress >= 0.9f)
            {
                yield return null;
                sceneLoadOperation.allowSceneActivation = true;
            }
            yield return null;
        }
        mainMenuUI.SetActive(true);
    }

    IEnumerator ShowWaveUI()
    {
        waveUIWaveNumber.text = (1 + WaveManager.currentWave).ToString();

        waveUI.SetActive(true);

        float waitForSeconds = 3;


        while(waitForSeconds > 0)
        {
            waitForSeconds -= Time.unscaledDeltaTime;
            yield return null;
        }

        waveNumberUIShowing = false;
        waveUI.SetActive(false);
    }

    public static float CubicEaseOut(float delta, float start, float end)
    {
        delta--;
        return Mathf.LerpUnclamped(start, end, delta * delta * delta + 1);
    }
}
