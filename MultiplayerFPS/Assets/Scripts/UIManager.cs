using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UIManager : MonoBehaviour{

    public static UIManager instance;
    public TextMeshProUGUI ammoText;
    public GameObject deathPanel;
    public TextMeshProUGUI deathMessageText;
    public TextMeshProUGUI deathTimerText;
    public Image healthImage;
    public Color goodHealthColor;
    public Color badHealthColor;
    public TextMeshProUGUI playerNameText;
    public GameObject joinGamePanel;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI deathsText;
    public GameObject leaderboardPanel;
    public LeaderboardPlayer leaderboardPlayerPrefab;
    public List<GameObject> allLeaderboardPlayers = new List<GameObject>();

    private void Awake() {
        instance = this;
    }
    private void Start() {
        ShowJoinGamePanel();
    }

    private void Update() {
        CheckShowLeaderboard();
    }

    public void ShowDeathPanel(string damager,float duration) {
        deathMessageText.text = "You were killed by " + damager;
        deathPanel.SetActive(true);
        StartCoroutine(StartDeathTimer(duration));
    }

    IEnumerator StartDeathTimer(float duration) {
        for (float t = 0; t < duration; t+=Time.deltaTime) {
            deathTimerText.text = ((int)(Mathf.Lerp(duration,0,t/duration))).ToString();
            yield return null;
        }
        deathTimerText.text = 0.ToString();
        HideDeathPanel();
    }

    private void HideDeathPanel() {
        deathPanel.SetActive(false);
    }

    public void ShowJoinGamePanel() {
        joinGamePanel.SetActive(true);
    }
    public void HideJoinGamePanel() {
        joinGamePanel.SetActive(false);
    }

    #region Leaderboard UI
    private void CheckShowLeaderboard() {
        if (Input.GetKeyDown(KeyCode.CapsLock)) {
            ShowLeaderboardPanel();
        }
        if (Input.GetKeyUp(KeyCode.CapsLock)) {
            HideLeaderboardPanel();
        }
    }

    private void HideLeaderboardPanel() {
        leaderboardPanel.SetActive(false);
    }
    public void ShowLeaderboardPanel() {
        foreach (GameObject player in allLeaderboardPlayers) {
            Destroy(player);
        }
        allLeaderboardPlayers.Clear();

        List<PlayerInfo> sortedPlayers = GetSortedPlayers(MatchManager.instance.allPlayers);

        leaderboardPlayerPrefab.gameObject.SetActive(true);
        for (int i = 0; i < sortedPlayers.Count; i++) {
            LeaderboardPlayer newplayer = Instantiate(leaderboardPlayerPrefab,leaderboardPlayerPrefab.transform.parent);
            PlayerInfo pInfo = sortedPlayers[i];
            newplayer.SetLeaderboardPlayer(pInfo.nickName, pInfo.kills, pInfo.deaths);
            allLeaderboardPlayers.Add(newplayer.gameObject);
        }
        leaderboardPlayerPrefab.gameObject.SetActive(false);

        leaderboardPanel.SetActive(true);
    }
    #endregion

    public void UpdateHealth(int current, int total) {
        float percentage = (float)current / (float)total;
        if (percentage < .3f) {
            healthImage.color = badHealthColor;
        }
        else {
            healthImage.color = goodHealthColor;
        }
        healthImage.fillAmount = percentage;
    }
    public void UpdatePlayerName(string name) {
        playerNameText.text = name;
    }

    private List<PlayerInfo> GetSortedPlayers(List<PlayerInfo> players) {
        List<PlayerInfo> sortedPlayers = new List<PlayerInfo>();

        while(sortedPlayers.Count < players.Count) {
            int highest = -1;
            PlayerInfo selection = players[0];
            foreach(PlayerInfo p in players) {
                if (!sortedPlayers.Contains(p)) {
                    if (p.kills > highest) {
                        highest = p.kills;
                        selection = p;
                    }
                }
            }
            sortedPlayers.Add(selection);
        }

        return sortedPlayers;
    }
}
