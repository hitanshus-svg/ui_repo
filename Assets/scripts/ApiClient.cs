using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// Handles all HTTP calls to the FastAPI backend.
// Attach this to a single persistent GameObject in your scene (e.g. an
// empty object called "Backend"). Access it anywhere via ApiClient.Instance.
public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }

    [Header("Backend Config")]
    [Tooltip("Local: http://127.0.0.1:8000  |  Deployed: your Railway/Render URL")]
    public string baseUrl = "http://127.0.0.1:8000";

    [Header("TEMPORARY - remove once a real login screen exists")]
    [Tooltip("If checked, logs in automatically on start using the fields below. Delete this block once you build a real login UI.")]
    public bool autoLoginForTesting = false;
    public string testTeamName = "TEAM - A";
    public string testPassword = "test12345";

    public string AccessToken { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (autoLoginForTesting)
        {
            Login(testTeamName, testPassword, (success, error) =>
            {
                if (success) Debug.Log($"Auto-login succeeded for {testTeamName}");
                else Debug.LogError($"Auto-login failed: {error}");
            });
        }
    }

    // ---------- LOGIN ----------
    // Your /auth/login endpoint uses OAuth2PasswordBearer, which expects
    // form-encoded username/password (NOT a JSON body).
    public void Login(string teamName, string password, System.Action<bool, string> onComplete)
    {
        StartCoroutine(LoginRoutine(teamName, password, onComplete));
    }

    private IEnumerator LoginRoutine(string teamName, string password, System.Action<bool, string> onComplete)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", teamName);
        form.AddField("password", password);

        using (UnityWebRequest req = UnityWebRequest.Post($"{baseUrl}/auth/login", form))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Login failed: {req.error} - {req.downloadHandler.text}");
                onComplete?.Invoke(false, req.downloadHandler.text);
                yield break;
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(req.downloadHandler.text);
            AccessToken = token.access_token;
            onComplete?.Invoke(true, null);
        }
    }

    // ---------- REGISTER ----------
    public void Register(string teamName, string password, System.Action<bool, string> onComplete)
    {
        StartCoroutine(RegisterRoutine(teamName, password, onComplete));
    }

    private IEnumerator RegisterRoutine(string teamName, string password, System.Action<bool, string> onComplete)
    {
        string json = JsonConvert.SerializeObject(new { name = teamName, password = password });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/auth/register", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Register failed: {req.error} - {req.downloadHandler.text}");
                onComplete?.Invoke(false, req.downloadHandler.text);
                yield break;
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(req.downloadHandler.text);
            AccessToken = token.access_token;
            onComplete?.Invoke(true, null);
        }
    }

    // ---------- LEADERBOARD (public, no auth needed) ----------
    public void GetLeaderboard(System.Action<List<LeaderboardEntry>> onComplete, int limit = 50)
    {
        StartCoroutine(GetLeaderboardRoutine(onComplete, limit));
    }

    private IEnumerator GetLeaderboardRoutine(System.Action<List<LeaderboardEntry>> onComplete, int limit)
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}/leaderboard?limit={limit}"))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Leaderboard fetch failed: {req.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            var entries = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(req.downloadHandler.text);
            onComplete?.Invoke(entries);
        }
    }

    // ---------- MY TEAM PROFILE (auth required) ----------
    public void GetMyTeam(System.Action<LeaderboardEntry> onComplete)
    {
        StartCoroutine(GetMyTeamRoutine(onComplete));
    }

    private IEnumerator GetMyTeamRoutine(System.Action<LeaderboardEntry> onComplete)
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("Cannot fetch team profile: not logged in yet.");
            onComplete?.Invoke(null);
            yield break;
        }

        using (UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}/teams/me"))
        {
            req.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GetMyTeam failed: {req.error} - {req.downloadHandler.text}");
                onComplete?.Invoke(null);
                yield break;
            }

            var entry = JsonConvert.DeserializeObject<LeaderboardEntry>(req.downloadHandler.text);
            onComplete?.Invoke(entry);
        }
    }

    // ---------- SUBMIT SCORE (auth required) ----------
    public void PatchScore(int pointsDelta, int storiesDelta, System.Action<bool> onComplete)
    {
        StartCoroutine(PatchScoreRoutine(pointsDelta, storiesDelta, onComplete));
    }

    private IEnumerator PatchScoreRoutine(int pointsDelta, int storiesDelta, System.Action<bool> onComplete)
    {
        if (!IsLoggedIn)
        {
            Debug.LogError("Cannot submit score: not logged in yet.");
            onComplete?.Invoke(false);
            yield break;
        }

        string json = JsonConvert.SerializeObject(new { points_delta = pointsDelta, stories_delta = storiesDelta });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/teams/me/score", "PATCH"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {AccessToken}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Score update failed: {req.error} - {req.downloadHandler.text}");
            }

            onComplete?.Invoke(req.result == UnityWebRequest.Result.Success);
        }
    }
}