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
    public enum EventCodes : byte { NewPlayer, ListPlayers, UpdateStat }

    private void Awake() {
        instance = this;
    }

    private void Start() {
        if (!PhotonNetwork.IsConnected) {
            SceneManager.LoadScene(0);
        }
        else {
            NewPlayerSend();
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
        PlayerInfo newPlayer = new PlayerInfo(data[0].ToString(),(int)data[1]);
        allPlayers.Add(newPlayer);
        ListPlayersSend();
    }
    public void ListPlayersSend() {
        object[] data = new object[allPlayers.Count];
        for (int i = 0; i < allPlayers.Count; i++) {
            object[] player = new object[4];
            player[0] = allPlayers[i].nickName;
            player[1] = allPlayers[i].actor;
            player[2] = allPlayers[i].deaths;
            player[3] = allPlayers[i].kills;
            data[i] = player;
        }
        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers,data,new RaiseEventOptions { Receivers = ReceiverGroup.All}, new SendOptions { Reliability = true });
    }
    public void ListPlayersReceive(object[] data) {
        allPlayers.Clear();
        for (int i = 0;i < data.Length;i++) {
            object[] playerData = (object[])data[i];
            PlayerInfo player = new PlayerInfo(playerData[0].ToString(), (int)playerData[1], (int)playerData[2], (int)playerData[3]);
            allPlayers.Add(player);
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor) {
                index = i;
            }
        }
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
                    UpdateStatsDisplay();
                }
                if (UIManager.instance.leaderboardPanel.activeInHierarchy) {
                    UIManager.instance.ShowLeaderboardPanel();
                }
                break;
            }
        }

    }

    private void UpdateStatsDisplay() {
        if (allPlayers.Count>index) {
            UIManager.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIManager.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
    }



}

[System.Serializable]
public class PlayerInfo{
    public string nickName;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(string nickName, int actor) {
        this.nickName = nickName;
        this.actor = actor;
        deaths = 0;
        kills = 0;
    }
    public PlayerInfo(string nickName, int actor, int deaths, int kills) {
        this.nickName = nickName;
        this.actor = actor;
        this.deaths = deaths;
        this.kills = kills;
    }
}
