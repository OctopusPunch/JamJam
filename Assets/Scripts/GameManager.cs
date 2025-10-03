using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(0)]
public class GameManager : MonoBehaviour
{
    public WaveManager WaveManager;

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
        ShowingWave
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
        }
    }

    void InGameUpdate()
    {
        if (WaveManager.beginNewWave)
        {
            if (WaveManager.currentWave >= WaveManager.Waves.Count)
            {
                state = GameState.Win;
                return;
            }
            if (WaveManager.trackedNPCs.Count == 0)
            {
                state = GameState.ShowNewWave;
                spawnSubWaveInSeconds = WaveManager.GetCurrentSubWaveSpawnTime();
                WaveManager.beginNewWave = false;
            }
            return;
        }
        spawnSubWaveInSeconds -= Time.deltaTime;

        if(spawnSubWaveInSeconds > 0)
        {
            return;
        }

        WaveManager.ReleaseNewSubwave();

        spawnSubWaveInSeconds = WaveManager.GetCurrentSubWaveSpawnTime();
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


    public void StartGame()
    {
        waveNumberUIShowing = true;
        mainMenuUI.SetActive(false);

        state = GameState.ShowingWave;
        TownResourceBehaviour.Instance.InitialiseResources();
        WaveManager.ResetWaves();
        spawnSubWaveInSeconds = WaveManager.GetCurrentSubWaveSpawnTime();
        StartCoroutine(ShowWaveUI());
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
}
