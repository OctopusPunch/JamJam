using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

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
        Win
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
        mainMenuUI.SetActive(true);

        StartCoroutine(WaitForSceneFinishLoadingAndShowMainMenuAsync("InGameScene"));
    }

    void DontDestroyUI()
    {
        DontDestroyOnLoad(pauseUI);
        DontDestroyOnLoad(mainMenuUI);
        DontDestroyOnLoad(settingsUI);
        DontDestroyOnLoad(PauseButton);
    }


    public void StartGame()
    {
        Debug.Log("Start New Game or Wave from here");
        SetToInGame();
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

    IEnumerator WaitForSceneFinishLoadingAsync(string sceneToLoad)
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

}
