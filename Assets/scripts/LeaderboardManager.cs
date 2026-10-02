using System.Collections.Generic;
using UnityEngine;

// Attach to an empty GameObject in your leaderboard scene.
public class LeaderboardManager : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The Scroll View's Content object where rows get spawned")]
    public Transform contentParent;
    [Tooltip("Your 'leaderboard prefab' asset")]
    public GameObject rowPrefab;

    [Header("Polling")]
    [Tooltip("How often to refresh, in seconds")]
    public float pollIntervalSeconds = 4f;

    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    void Start()
    {
        InvokeRepeating(nameof(RefreshLeaderboard), 0f, pollIntervalSeconds);
    }

    void RefreshLeaderboard()
    {
        ApiClient.Instance.GetLeaderboard(OnLeaderboardReceived);
    }

    void OnLeaderboardReceived(List<LeaderboardEntry> entries)
    {
        // If the fetch failed, entries will be null — keep showing the
        // last successfully loaded leaderboard instead of clearing it.
        if (entries == null) return;

        foreach (GameObject row in spawnedRows) Destroy(row);
        spawnedRows.Clear();

        foreach (LeaderboardEntry entry in entries)
        {
            GameObject rowObj = Instantiate(rowPrefab, contentParent);
            LeaderboardRowUI rowUI = rowObj.GetComponent<LeaderboardRowUI>();
            if (rowUI != null) rowUI.SetData(entry);
            spawnedRows.Add(rowObj);
        }
    }
}
