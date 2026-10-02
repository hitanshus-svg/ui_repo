using UnityEngine;

/// <summary>
/// Handles navigation between the app's main screens (Map, Quest, Leaderboard).
/// Attach this to a single GameObject in your scene (e.g. an empty "UIManager" object,
/// or your root Canvas), then assign the panel references in the Inspector.
///
/// Wiring buttons:
///   - Map page's "Quest" button      -> UIManager.ShowQuest()
///   - Map page's "Leaderboard" button-> UIManager.ShowLeaderboard()
///   - Quest page's "Map" button      -> UIManager.ShowMap()
///   - Leaderboard page's "Map" button-> UIManager.ShowMap()
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen Panels")]
    [Tooltip("Root GameObject for the Map page")]
    public GameObject mapPanel;

    [Tooltip("Root GameObject for the Quest / Mission page")]
    public GameObject questPanel;

    [Tooltip("Root GameObject for the Leaderboard page")]
    public GameObject leaderboardPanel;

    [Header("Behavior")]
    [Tooltip("Which screen should be active when the app starts")]
    public Screen startScreen = Screen.Map;

    public enum Screen { Map, Quest, Leaderboard }

    private Screen currentScreen;

    private void Awake()
    {
        // Simple singleton so buttons anywhere can call UIManager.Instance.ShowX()
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ShowScreen(startScreen);
    }

    // ---- Public methods to hook up to Button OnClick() events ----

    public void ShowMap() => ShowScreen(Screen.Map);
    public void ShowQuest() => ShowScreen(Screen.Quest);
    public void ShowLeaderboard() => ShowScreen(Screen.Leaderboard);

    // ---- Core logic ----

    public void ShowScreen(Screen screen)
    {
        if (mapPanel != null) mapPanel.SetActive(screen == Screen.Map);
        if (questPanel != null) questPanel.SetActive(screen == Screen.Quest);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(screen == Screen.Leaderboard);

        currentScreen = screen;
    }

    public Screen GetCurrentScreen() => currentScreen;
}