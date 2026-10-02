using System;
using Newtonsoft.Json;

[Serializable]
public class LeaderboardEntry
{
    public int rank;
    public int team_id;

    [JsonProperty("team_name")]
    public string name;

    public int points;
    [JsonProperty("stories_completed")]
    public int stories;
}

[Serializable]
public class TokenResponse
{
    public string access_token;
    public string token_type;
}