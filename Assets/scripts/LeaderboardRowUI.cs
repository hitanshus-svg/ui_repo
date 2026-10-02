using TMPro;
using UnityEngine;

// Attach this to the root of your "leaderboard prefab".
// Drag each child's TMP_InputField component into the matching slot below.
public class LeaderboardRowUI : MonoBehaviour
{
    [Header("Drag the matching TMP_InputFields from this prefab's children")]
    public TMP_InputField rankField;
    public TMP_InputField teamNameField;
    public TMP_InputField pointsField;

    public void SetData(LeaderboardEntry entry)
    {
        if (rankField != null)
        {
            rankField.text = entry.rank.ToString("00");
            rankField.interactable = false; // display-only, not editable by players
        }

        if (teamNameField != null)
        {
            teamNameField.text = entry.name;
            teamNameField.interactable = false;
        }

        if (pointsField != null)
        {
            pointsField.text = $"{entry.points} POINTS";
            pointsField.interactable = false;
        }
    }
}
