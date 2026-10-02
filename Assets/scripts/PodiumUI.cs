using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Fields for one podium position (a team name + points pair).
[System.Serializable]
public class PodiumSlot
{
    public TMP_InputField teamNameField;
    public TMP_InputField pointsField;
}

// Attach to your "Rankers" section (the 3-column podium row).
public class PodiumUI : MonoBehaviour
{
    [Header("Assign in visual left-to-right order shown in your UI")]
    public PodiumSlot secondPlaceSlot; // left column, badge "2"
    public PodiumSlot firstPlaceSlot;  // middle column, badge "1"
    public PodiumSlot thirdPlaceSlot;  // right column, badge "3"

    [Header("Polling")]
    public float pollIntervalSeconds = 4f;

    void Start()
    {
        InvokeRepeating(nameof(Refresh), 0f, pollIntervalSeconds);
    }

    void Refresh()
    {
        if (ApiClient.Instance == null) return;
        ApiClient.Instance.GetLeaderboard(OnLeaderboardReceived, limit: 3);
    }

    void OnLeaderboardReceived(List<LeaderboardEntry> entries)
    {
        if (entries == null || entries.Count == 0) return;

        // entries[0] is rank 1, entries[1] is rank 2, entries[2] is rank 3
        if (entries.Count >= 1) SetSlot(firstPlaceSlot, entries[0]);
        if (entries.Count >= 2) SetSlot(secondPlaceSlot, entries[1]);
        if (entries.Count >= 3) SetSlot(thirdPlaceSlot, entries[2]);
    }

    void SetSlot(PodiumSlot slot, LeaderboardEntry entry)
    {
        if (slot == null || entry == null) return;

        if (slot.teamNameField != null)
        {
            slot.teamNameField.text = entry.name;
            slot.teamNameField.interactable = false;
        }
        if (slot.pointsField != null)
        {
            slot.pointsField.text = $"{entry.points} POINTS";
            slot.pointsField.interactable = false;
        }
    }
}
