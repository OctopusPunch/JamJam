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
        CheckingScore,
        PerfectDisplay,
        Bust,
        NotEnough,
        ALittleMore
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
    private GameObject bustUI;
    [SerializeField]
    private GameObject notEnoughUI;
    [SerializeField]
    private GameObject aLittleMoreUI;
    [SerializeField]
    private GameObject waveUI;
    [SerializeField]
    private GameObject winUI;
    [SerializeField]
    private GameObject loseUI;
    [SerializeField]
    private GameObject perfectUI;
    [SerializeField]
    TextMeshProUGUI perfectUIBonusScoreText;
    [SerializeField]
    TextMeshProUGUI perfectUITimeBonusScoreText;

    // in game stuffs
    private float spawnSubWaveInSeconds;
    private bool waveNumberUIShowing = false;
    private bool godHandActive;

    private float timeScaleDelta = 0;
    private const float deltaDuration = .35f;
    private const float targetDeltaTime = .25f;

    private int perfectMatchCount = 0;
    private int chainedPerfects = 0;
    public int ChainedPerfects => chainedPerfects;
    private BigInteger score = 0;
    public BigInteger Score => score;
    BigInteger addToScore = 0;

    private bool isFirstWave = false;

    private float scoreDivider = 1f;
    private float showAsMultiplier;

    public bool perfectRun = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Application.targetFrameRate = 60;
        state = GameState.MainMenu;
        instance = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyUI();

        pauseUI.SetActive(false);
        settingsUI.SetActive(false);
        PauseButton.SetActive(false);
        mainMenuUI.SetActive(false);
        perfectUI.SetActive(false);
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
                perfectRun = false;
                return;

            case GameState.In_Game:
                InGameUpdate();
                return;

            case GameState.Lose:
                perfectRun = false;
                loseUI.SetActive(true);
                return;

            case GameState.Win:
                perfectRun = false;
                winUI.SetActive(true);
                return;
            case GameState.ShowNewWave:
                perfectRun = false;
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
                perfectRun = false;
                state = GameState.CheckingScore;
                break;
                case GameState.CheckingScore:
                CheckingScore();
                break;
            case GameState.PerfectDisplay:
                break;
            case GameState.Bust:
                StartCoroutine(ShowBustUI());
                waveNumberUIShowing = true;
                state = GameState.ShowingWave;
                break;
            case GameState.NotEnough:
                StartCoroutine(ShowNotEnoughUI());
                waveNumberUIShowing = true;
                state = GameState.ShowingWave;
                break;
            case GameState.ALittleMore:
                StartCoroutine(ShowALittleMoreUI());
                waveNumberUIShowing = true;
                state = GameState.ShowingWave;
                break;
        }
    }

    void InGameUpdate()
    {
        GodHandSlowDown();

        if (WaveManager.trackedNPCs.Count > 0)
        {
            if(perfectRun)
            {
                return;
            }
            scoreDivider -= Time.unscaledDeltaTime * .016f;
            return;
        }

        if (!isFirstWave)
        {
            if (!TownResourceBehaviour.Instance.CheckWinNoBonus())
            {
                TownResourceBehaviour.Instance.CheckWinOrFail();
                if (state == GameState.RunCheckScore || state == GameState.CheckingScore)
                {
                    return;
                }
                state = GameState.NotEnough;
                TownResourceBehaviour.Instance.AdjustHungerMeter(-1);
                chainedPerfects = 0;
                return;
            }
            else
            {
                state = GameState.ALittleMore;
                chainedPerfects = 0;
            }
        }
        isFirstWave = false;
        WaveManager.ReleaseNewWave();

    }

    public void ResetChain()
    {
        chainedPerfects = 0;
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
        DontDestroyOnLoad(perfectUI);
        DontDestroyOnLoad(bustUI);
        DontDestroyOnLoad(notEnoughUI);
        DontDestroyOnLoad(aLittleMoreUI);
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
        if(scoreDivider < 0.05f)
        {
            scoreDivider = 0.05f;
        }
        showAsMultiplier = 1 + scoreDivider;
        perfectUITimeBonusScoreText.text = "Time Bonus: " + showAsMultiplier.ToString("N2") + "%";
        addToScore = 0;
        int scoreMultiplier = 1;
        perfectMatchCount = 0;
        //water

        if (TownResourceBehaviour.Instance.CurrentWaterValue == TownResourceBehaviour.Instance.TargetWaterValue)
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
        if (TownResourceBehaviour.Instance.CurrentGoldValue == TownResourceBehaviour.Instance.TargetGoldValue)
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
        if (TownResourceBehaviour.Instance.CurretFoodValue == TownResourceBehaviour.Instance.TargetFoodValue)
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
            addToScore = (BigInteger)addToScore * (BigInteger)(chainedPerfects >= 1 ? BigInteger.Pow(scoreMultiplier, chainedPerfects + 1) : scoreMultiplier);
        }
        else
        {
            chainedPerfects = 0;
        }


        if (perfectMatchCount > 0)
        {
            addToScore += 5000 * perfectMatchCount;
        }


        if(perfectMatchCount == 3)
        {
            addToScore = PercentOf(addToScore, (decimal)scoreDivider);
            scoreDivider = 1f;
            StartCoroutine(ShowPerfectUI());
            state = GameState.PerfectDisplay;
            return;
        }
        scoreDivider = 1f;
        score += addToScore;
        state = GameState.In_Game;
        TownResourceBehaviour.Instance.ResetGoldMeter();
        TownResourceBehaviour.Instance.ResetFoodMeter();
        TownResourceBehaviour.Instance.ResetWaterMeter();
        WaveManager.ReleaseNewWave();

    }
    BigInteger PercentOf(BigInteger value, decimal multiplier, int precision = 10000)
    {
        // Convert multiplier to scaled integer
        BigInteger scaled = (BigInteger)(multiplier * precision);
        return (value * scaled) / precision;
    }

    public void SetToWin()
    {
        state = GameState.Win;
    }

    public void SetToLose()
    {
        chainedPerfects = 0;
        state = GameState.Lose;
    }
    public void RunCheckScore()
    {
        state = GameState.RunCheckScore;
    }
    public void SetToBust()
    {
        chainedPerfects = 0;
        state = GameState.Bust;
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
        waveUI.GetComponent<CanvasGroup>().alpha = 0;
        waveUI.SetActive(true);

        float waitForSeconds = 8f;
        float delta = 0;
        delta += Time.unscaledDeltaTime;
        while(delta < 0.65f)
        {
            waveUI.GetComponent<CanvasGroup>().alpha = delta/.65f;
            yield return null;
            delta += Time.unscaledDeltaTime;
        }
        waveUI.GetComponent<CanvasGroup>().alpha = 1;
        while (waitForSeconds > 0)
        {
            waitForSeconds -= Time.unscaledDeltaTime;
            yield return null;
        }
        delta = .65f;
        delta -= Time.unscaledDeltaTime;
        while (delta > 0)
        {
            waveUI.GetComponent<CanvasGroup>().alpha = delta / .65f;
            yield return null;
            delta -= Time.unscaledDeltaTime;
        }

        waveUI.GetComponent<CanvasGroup>().alpha = 0;
        waveNumberUIShowing = false;
        waveUI.SetActive(false);
    }
    IEnumerator ShowBustUI()
    {
        bustUI.GetComponent<CanvasGroup>().alpha = 0;
        bustUI.SetActive(true);

        float waitForSeconds = 3f;
        float delta = 0;
        delta += Time.unscaledDeltaTime;
        while(delta < 0.65f)
        {
            bustUI.GetComponent<CanvasGroup>().alpha = delta/.65f;
            yield return null;
            delta += Time.unscaledDeltaTime;
        }
        bustUI.GetComponent<CanvasGroup>().alpha = 1;
        while (waitForSeconds > 0)
        {
            waitForSeconds -= Time.unscaledDeltaTime;
            yield return null;
        }
        delta = .65f;
        delta -= Time.unscaledDeltaTime;
        while (delta > 0)
        {
            bustUI.GetComponent<CanvasGroup>().alpha = delta / .65f;
            yield return null;
            delta -= Time.unscaledDeltaTime;
        }

        bustUI.GetComponent<CanvasGroup>().alpha = 0;
        waveNumberUIShowing = false;
        bustUI.SetActive(false);
        TownResourceBehaviour.Instance.ResetGoldMeter();
        TownResourceBehaviour.Instance.ResetFoodMeter();
        TownResourceBehaviour.Instance.ResetWaterMeter();
    }
    IEnumerator ShowNotEnoughUI()
    {
        notEnoughUI.GetComponent<CanvasGroup>().alpha = 0;
        notEnoughUI.SetActive(true);

        float waitForSeconds = 3f;
        float delta = 0;
        delta += Time.unscaledDeltaTime;
        while(delta < 0.65f)
        {
            notEnoughUI.GetComponent<CanvasGroup>().alpha = delta/.65f;
            yield return null;
            delta += Time.unscaledDeltaTime;
        }
        notEnoughUI.GetComponent<CanvasGroup>().alpha = 1;
        while (waitForSeconds > 0)
        {
            waitForSeconds -= Time.unscaledDeltaTime;
            yield return null;
        }
        delta = .65f;
        delta -= Time.unscaledDeltaTime;
        while (delta > 0)
        {
            notEnoughUI.GetComponent<CanvasGroup>().alpha = delta / .65f;
            yield return null;
            delta -= Time.unscaledDeltaTime;
        }

        notEnoughUI.GetComponent<CanvasGroup>().alpha = 0;
        waveNumberUIShowing = false;
        notEnoughUI.SetActive(false);
        TownResourceBehaviour.Instance.ResetGoldMeter();
        TownResourceBehaviour.Instance.ResetFoodMeter();
        TownResourceBehaviour.Instance.ResetWaterMeter();
    }
    IEnumerator ShowALittleMoreUI()
    {
        aLittleMoreUI.GetComponent<CanvasGroup>().alpha = 0;
        aLittleMoreUI.SetActive(true);

        float waitForSeconds = 3f;
        float delta = 0;
        delta += Time.unscaledDeltaTime;
        while(delta < 0.65f)
        {
            aLittleMoreUI.GetComponent<CanvasGroup>().alpha = delta/.65f;
            yield return null;
            delta += Time.unscaledDeltaTime;
        }
        aLittleMoreUI.GetComponent<CanvasGroup>().alpha = 1;
        while (waitForSeconds > 0)
        {
            waitForSeconds -= Time.unscaledDeltaTime;
            yield return null;
        }
        delta = .65f;
        delta -= Time.unscaledDeltaTime;
        while (delta > 0)
        {
            aLittleMoreUI.GetComponent<CanvasGroup>().alpha = delta / .65f;
            yield return null;
            delta -= Time.unscaledDeltaTime;
        }

        aLittleMoreUI.GetComponent<CanvasGroup>().alpha = 0;
        waveNumberUIShowing = false;
        aLittleMoreUI.SetActive(false);
        TownResourceBehaviour.Instance.ResetGoldMeter();
        TownResourceBehaviour.Instance.ResetFoodMeter();
        TownResourceBehaviour.Instance.ResetWaterMeter();
    }
    IEnumerator ShowPerfectUI()
    {
        perfectUIBonusScoreText.text = "0";
        perfectUI.GetComponent<CanvasGroup>().alpha = 0;
        perfectUI.SetActive(true);

        float waitForSeconds = 3f;
        float delta = 0;
        delta += Time.unscaledDeltaTime;
        while(delta < 0.65f)
        {
            perfectUI.GetComponent<CanvasGroup>().alpha = delta/.65f;
            yield return null;
            delta += Time.unscaledDeltaTime;
        }
        perfectUI.GetComponent<CanvasGroup>().alpha = 1;


        yield return ShowScoreAdditionJuice();
        while (waitForSeconds > 0)
        {
            waitForSeconds -= Time.unscaledDeltaTime;
            yield return null;
        }






        delta = .65f;
        delta -= Time.unscaledDeltaTime;
        while (delta > 0)
        {
            perfectUI.GetComponent<CanvasGroup>().alpha = delta / .65f;
            yield return null;
            delta -= Time.unscaledDeltaTime;
        }

        perfectUI.GetComponent<CanvasGroup>().alpha = 0;
        perfectUI.SetActive(false);


        state = GameState.In_Game;
        TownResourceBehaviour.Instance.ResetGoldMeter();
        TownResourceBehaviour.Instance.ResetFoodMeter();
        TownResourceBehaviour.Instance.ResetWaterMeter();
        WaveManager.ReleaseNewWave();
    }

    IEnumerator ShowScoreAdditionJuice()
    {
        perfectUIBonusScoreText.transform.localScale = UnityEngine.Vector3.one;
        perfectUIBonusScoreText.text = "0";
        score += addToScore;
        BigInteger chaser = BigInteger.Zero;

        UnityEngine.Vector3 baseScale = UnityEngine.Vector3.one;
        UnityEngine.Vector3 punchScale = baseScale * 2f;

        BigInteger lastMilestone = 0;

        int growthCount = 1;

        while (chaser != addToScore)
        {
            BigInteger difference = addToScore - chaser;
            if (difference != 0)
            {
                BigInteger step = difference / 10 * growthCount;
                if (step == 0) step = difference > 0 ? 1 : -1;
                chaser += step;
            }

            BigInteger[] milestones = { 10_000, 100_000, 1_000_000, 10_000_000 };
            foreach (var m in milestones)
            {
                if (chaser >= m && lastMilestone < m)
                {
                    lastMilestone = m;

                    perfectUIBonusScoreText.text = chaser.ToString("N0");
                    // PlaySFX("ScorePop");
                    StartCoroutine(BounceText(perfectUIBonusScoreText.transform, punchScale, baseScale));
                    ++growthCount;

                }
            }

            perfectUIBonusScoreText.text = chaser.ToString("N0");
            yield return null;
        }

        perfectUIBonusScoreText.text = chaser.ToString("N0");
    }
    IEnumerator BounceText(Transform t, UnityEngine.Vector3 big, UnityEngine.Vector3 small)
    {
        UnityEngine.Vector3 baseScale = t.localScale;
        float nextBaseScale = Mathf.Min(baseScale.x * 1.05f, 3f);
        UnityEngine.Vector3 targetBaseScale = UnityEngine.Vector3.one * nextBaseScale;

        UnityEngine.Vector3 punchScale = targetBaseScale * 1.3f;

        float tLerp = 0f;
        while (tLerp < 1f)
        {
            tLerp += Time.unscaledDeltaTime * 3;
            float ease = 1f - Mathf.Pow(1f - tLerp, 3f);
            t.localScale = UnityEngine.Vector3.Lerp(punchScale, targetBaseScale, ease);
            yield return null;
        }

        t.localScale = targetBaseScale;
    }
    public static float CubicEaseOut(float delta, float start, float end)
    {
        delta--;
        return Mathf.LerpUnclamped(start, end, delta * delta * delta + 1);
    }
}
