using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button quitButton;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayHUD;

    [Header("Scene Groups")]
    [SerializeField] private GameObject menuEnvironment;
    [SerializeField] private GameObject gameplayRoot;

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
    }

    private void UnregisterButtonListeners()
    {
        if (playButton != null) playButton.onClick.RemoveListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(CloseSettings);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
    }

    public void StartGame()
    {
        SetActiveSafely(mainMenuPanel, false);
        SetActiveSafely(settingsPanel, false);
        SetActiveSafely(menuEnvironment, false);

        SetActiveSafely(gameplayRoot, true);
        SetActiveSafely(gameplayHUD, true);

        Time.timeScale = 1f;
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
        SetActiveSafely(mainMenuPanel, true);

        Time.timeScale = 1f;
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
