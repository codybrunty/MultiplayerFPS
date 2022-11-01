using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardPlayer : MonoBehaviour{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerKillsText;
    public TextMeshProUGUI playerDeathsText;

    public void SetLeaderboardPlayer(string name,int kills, int deaths) {
        playerNameText.text = name;
        playerKillsText.text = kills.ToString();
        playerDeathsText.text = deaths.ToString();
    }
}
