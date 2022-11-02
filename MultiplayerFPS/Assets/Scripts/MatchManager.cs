using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback{

    public static MatchManager instance;
    public List<PlayerInfo> allPlayers = new List<PlayerInfo> ();
    public int index;
    public enum EventCodes : byte { NewPlayer, ListPlayers, UpdateStat, NextMatch }
    public enum GameState { Waiting, Playing, Ending }
    public int killsToWin;
    public GameState gameState = GameState.Waiting;
    public float waitAfterEnding;
    public bool perpetual;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        if (!PhotonNetwork.IsConnected) {
            SceneManager.LoadScene(0);
        }
        else {
            NewPlayerSend();
            gameState = GameState.Playing;
        }
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code < 200) {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            switch (theEvent) {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatReceive(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchReceived();
                    break;
            }
        }
    }

    public override void OnEnable() {
        base.OnEnable();
    }

    public override void OnDisable() {
        base.OnDisable();
    }

    public void NewPlayerSend() {
        object[] data = new object[2];
        data[0] = PhotonNetwork.NickName;
        data[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer,data,new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, new SendOptions { Reliability = true });
    }

    public void NewPlayerReceive(object[] data) {
        PlayerInfo newPlayer = new PlayerInfo(data[0].ToString(),(int)data[1],0,0);
        allPlayers.Add(newPlayer);
        //Debug.Log("Count: " + allPlayers.Count);
        ListPlayersSend();
    }
    public void ListPlayersSend() {
        object[] data = new object[allPlayers.Count+1];
        data[0] = gameState;
        for (int i = 0; i < allPlayers.Count; i++) {
            object[] player = new object[5];
            player[0] = allPlayers[i].nickName;
            player[1] = allPlayers[i].actor;
            player[2] = allPlayers[i].deaths;
            player[3] = allPlayers[i].kills;
            data[i+1] = player;
        }
        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers,data,new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }
    public void ListPlayersReceive(object[] data) {
        gameState = (GameState)data[0];
        if (!PhotonNetwork.IsMasterClient) {
            allPlayers.Clear();
        }
        for (int i = 1; i < data.Length; i++) {
            object[] playerData = (object[])data[i];
            PlayerInfo player = new PlayerInfo(playerData[0].ToString(), (int)playerData[1], (int)playerData[2], (int)playerData[3]);
            if (!PhotonNetwork.IsMasterClient) {
                allPlayers.Add(player);
            }
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor) {
                index = i - 1;
                //Debug.Log(PhotonNetwork.LocalPlayer.NickName+": "+index);
            }
        }
        StateCheck();
    }
    public void UpdateStatSend(int actor, int stat, int amt) {
        //stat 0deaths 1kills
        object[] data = new object[3];
        data[0] = actor;
        data[1] = stat;
        data[2] = amt;
        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStat, data, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }

    public void UpdateStatReceive(object[] data) {
        int actor = (int)data[0];
        //stat 0deaths 1kills
        int stat = (int)data[1];
        int amt = (int)data[2];
        for (int i = 0; i < allPlayers.Count; i++) {
            if(allPlayers[i].actor == actor) {
                switch (stat) {
                    case 0:
                        allPlayers[i].deaths += amt;
                        break;
                    case 1:
                        allPlayers[i].kills += amt;
                        break;
                }
                if (i == index) {
                    Debug.Log(PhotonNetwork.LocalPlayer.NickName + ": " + index);
                    UpdateStatsDisplay();
                }
                if (UIManager.instance.leaderboardPanel.activeInHierarchy) {
                    UIManager.instance.ShowLeaderboardPanel();
                }
                break;
            }
        }
        ScoreCheck();
    }

    private void UpdateStatsDisplay() {
        if (allPlayers.Count>index) {
            UIManager.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIManager.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }

    private void ScoreCheck() {
        bool results = false;
        foreach (PlayerInfo p in allPlayers) { 
            if(p.kills >= killsToWin) {
                results = true;
                break;
            }
        }
        if (results) {
            if(PhotonNetwork.IsMasterClient && gameState != GameState.Ending) {
                gameState = GameState.Ending;
                ListPlayersSend();
            }
        }
    }

    private void StateCheck() {
        if (gameState == GameState.Ending) {
            EndGame();
        }
    }

    private void EndGame() {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.DestroyAll();
        }
        UIManager.instance.ShowMatchOverPanel();
        UIManager.instance.ShowLeaderboardPanel();
        UIManager.instance.HideDeathPanel();

        Camera cam = Camera.main;
        cam.transform.position = UIManager.instance.matchOverCameraPoint.position;
        cam.transform.rotation = UIManager.instance.matchOverCameraPoint.rotation;
        
        StartCoroutine(ReturnToMainMenu());
    }

    IEnumerator ReturnToMainMenu() {
        yield return new WaitForSeconds(waitAfterEnding);
        if (perpetual) {
            if (PhotonNetwork.IsMasterClient) {
                if (!Launcher.instance.changeMapBetweenRounds) {
                    NextMatchSend();
                }
                else {
                    int randomMapIndex = Random.Range(0, Launcher.instance.allMaps.Length);
                    if (Launcher.instance.allMaps[randomMapIndex] == SceneManager.GetActiveScene().name) {
                        NextMatchSend();
                    }
                    else {
                        PhotonNetwork.LoadLevel(Launcher.instance.allMaps[randomMapIndex]);
                    }
                }
            }
        }
        else {
            PhotonNetwork.AutomaticallySyncScene = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PhotonNetwork.LeaveRoom();
        }
    }

    public void NextMatchSend() {
        PhotonNetwork.RaiseEvent((byte)EventCodes.NextMatch, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, new SendOptions { Reliability = true });
    }

    public void NextMatchReceived() {
        gameState = GameState.Playing;
        UIManager.instance.HideMatchOverPanel();
        UIManager.instance.HideLeaderboardPanel();
        UIManager.instance.HideDeathPanel();
        foreach (PlayerInfo p in allPlayers) {
            p.kills = 0;
            p.deaths = 0;
        }
        UpdateStatsDisplay();
        SpawnManager.instance.SpawnPlayer();
    }

}

[System.Serializable]
public class PlayerInfo{
    public string nickName;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(string nickName, int actor, int deaths, int kills) {
        this.nickName = nickName;
        this.actor = actor;
        this.deaths = deaths;
        this.kills = kills;
    }
}
