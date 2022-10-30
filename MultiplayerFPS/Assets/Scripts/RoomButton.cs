using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour{

    [SerializeField] TextMeshProUGUI buttonText;
    private RoomInfo roomInfo;

    public void SetupRoomButton(RoomInfo info) {
        roomInfo = info;
        SetButtonText(roomInfo.Name);
    }

    private void SetButtonText(string name) {
        buttonText.text = name;
    }

    public void JoinRoom() {
        Launcher.instance.JoinRoom(roomInfo);
    }

}
