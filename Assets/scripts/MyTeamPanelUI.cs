using TMPro;
using UnityEngine;

// Attach to your "Rank" panel (the purple card showing your own team's stats).
public class MyTeamPanelUI : MonoBehaviour
{
    [Header("Drag the matching TMP_InputFields from your Rank panel")]
    public TMP_InputField teamNameField;
    public TMP_InputField storiesField;
    public TMP_InputField pointsField;
    public TMP_InputField rankField;

    [Header("Polling")]
    public float pollIntervalSeconds = 4f;

    void Start()
    {
        InvokeRepeating(nameof(Refresh), 0f, pollIntervalSeconds);
    }

    void Refresh()
    {
        // Nothing to show until a team is logged in.
        if (ApiClient.Instance == null || !ApiClient.Instance.IsLoggedIn) return;
        ApiClient.Instance.GetMyTeam(OnMyTeamReceived);
    }

    void OnMyTeamReceived(LeaderboardEntry entry)
    {
        if (entry == null) return;

        if (teamNameField != null)
        {
            teamNameField.text = entry.name;
            teamNameField.interactable = false;
        }
        if (storiesField != null)
        {
            storiesField.text = $"STORIES : {entry.stories}";
            storiesField.interactable = false;
        }
        if (pointsField != null)
        {
            pointsField.text = $"POINTS : {entry.points}";
            pointsField.interactable = false;
        }
        if (rankField != null)
        {
            rankField.text = $"RANK : {entry.rank:00}";
            rankField.interactable = false;
        }
    }
}
