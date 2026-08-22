using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Scene Groups")]
    [SerializeField] private GameObject menuEnvironment;
    [SerializeField] private GameObject gameplayRoot;

    [Header("Level Skyboxes")]
    [SerializeField] private Material level1Skybox;
    [SerializeField] private Material level2Skybox;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayHUD;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button quitButton;

    [Header("Level Select Buttons")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button backButton;





    private void Awake()
    {
        RegisterButtonListeners();
    }

    private void Start()
    {
        ShowMainMenu();
        //StartGame();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
    }

    private void RegisterButtonListeners()
    {
        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (level1Button != null) level1Button.onClick.AddListener(PlayLevel1);
        if (level2Button != null) level2Button.onClick.AddListener(PlayLevel2);
        if (backButton != null) backButton.onClick.AddListener(BackToMenu);
    }

    private void UnregisterButtonListeners()
    {
        if (playButton != null) playButton.onClick.RemoveListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(CloseSettings);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        if (level1Button != null) level1Button.onClick.RemoveListener(PlayLevel1);
        if (level2Button != null) level2Button.onClick.RemoveListener(PlayLevel2);
        if (backButton != null) backButton.onClick.RemoveListener(BackToMenu);
    }

    public void StartGame()
    {
        SetActiveSafely(mainMenuPanel, false);
        SetActiveSafely(levelSelectPanel, true);
    }

    public void OpenSettings()
    {
        SetActiveSafely(mainMenuPanel, false);
        SetActiveSafely(settingsPanel, true);
    }

    private void CloseSettings()
    {
        SetActiveSafely(settingsPanel, false);
        SetActiveSafely(mainMenuPanel, true);
    }

    public void ShowMainMenu()
    {
        SetActiveSafely(gameplayRoot, false);
        SetActiveSafely(gameplayHUD, false);

        SetActiveSafely(menuEnvironment, true);
        SetActiveSafely(settingsPanel, false);
        SetActiveSafely(levelSelectPanel, false);
        SetActiveSafely(mainMenuPanel, true);

        Time.timeScale = 1f;
    }

    private void PlayLevel1()
    {
        SetActiveSafely(mainMenuPanel, false);
        SetActiveSafely(settingsPanel, false);
        SetActiveSafely(levelSelectPanel, false);
        SetActiveSafely(menuEnvironment, false);

        SetActiveSafely(gameplayRoot, true);
        SetActiveSafely(gameplayHUD, true);

        RenderSettings.skybox = level1Skybox;

        Time.timeScale = 1f;
    }

    private void PlayLevel2()
    {
        SetActiveSafely(mainMenuPanel, false);
        SetActiveSafely(settingsPanel, false);
        SetActiveSafely(levelSelectPanel, false);
        SetActiveSafely(menuEnvironment, false);

        SetActiveSafely(gameplayRoot, true);
        SetActiveSafely(gameplayHUD, true);

        RenderSettings.skybox = level2Skybox;

        Time.timeScale = 1f;
    }

    private void BackToMenu()
    {
        SetActiveSafely(mainMenuPanel, true);
        SetActiveSafely(levelSelectPanel, false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetActiveSafely(GameObject target, bool isActive)
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }


}
