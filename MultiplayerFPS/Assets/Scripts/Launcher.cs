using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks{

    public static Launcher instance;
    [Header("Loading Panel")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [Header("Error Panel")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorText;
    [Header("Main Menu")]
    [SerializeField] private GameObject menuPanel;
    [Header("Create Room Menu")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private TMP_InputField createRoomInputField;
    [Header("Room Menu")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private TextMeshProUGUI roomNameText;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        HideAllMenus();
        ConnectToNetwork();
    }

    #region Connect
    private void ConnectToNetwork() {
        ShowLoadingPanel();
        SetLoadingText("Connecting To Network...");
        PhotonNetwork.ConnectUsingSettings();
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnectedToMaster() {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
        SetLoadingText("Joining Lobby...");
    }

    public override void OnJoinedLobby() {
        base.OnJoinedLobby();
        ShowMainMenu();
    }
    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        SetRoomNameText(PhotonNetwork.CurrentRoom.Name);
        ShowRoomPanel();
    }
    public override void OnCreateRoomFailed(short returnCode, string message) {
        base.OnCreateRoomFailed(returnCode, message);
        SetErrorText("Failed To Create Room: "+message);
        ShowErrorPanel();
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
        ShowMainMenu();
    }

    #endregion

    #region UI    

    #region On Click Events
    public void CloseErrorPanel() {
        ShowMainMenu();
    }

    public void OpenCreateRoom() {
        ShowCreateRoomPanel();
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        SetLoadingText("Leaving Room...");
        ShowLoadingPanel();
    }

    public void CreateRoom() {
        if (string.IsNullOrEmpty(createRoomInputField.text)) { return; }
        RoomOptions rOptions = new RoomOptions();
        rOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(createRoomInputField.text, rOptions);
        ShowLoadingPanel();
        SetLoadingText("Creating Room...");
    }

    #endregion

    #region Show
    private void ShowLoadingPanel() {
        HideAllMenus();
        loadingPanel.SetActive(true);
    }
    private void ShowErrorPanel() {
        HideAllMenus();
        errorPanel.SetActive(true);
    }

    private void ShowMainMenu() {
        HideAllMenus();
        menuPanel.SetActive(true);
    }

    public void ShowCreateRoomPanel() {
        HideAllMenus();
        createRoomPanel.SetActive(true);
    }

    public void ShowRoomPanel() {
        HideAllMenus();
        roomPanel.SetActive(true);
    }
    #endregion

    #region Hide
    private void HideAllMenus() {
        loadingPanel.SetActive(false);
        menuPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
    }
    #endregion

    #region Texts
    private void SetLoadingText(string message) {
        loadingText.text = message;
    }
    private void SetErrorText(string message) {
        errorText.text = message;
    }
    private void SetRoomNameText(string name) {
        roomNameText.text = name;
    }
    #endregion

    #endregion

}
