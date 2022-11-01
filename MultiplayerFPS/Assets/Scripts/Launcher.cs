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
    [SerializeField] private GameObject testRoomButton;
    public static bool onStart = true;
    [Header("Find Room Menu")]
    [SerializeField] private GameObject joinRoomPanel;
    [SerializeField] private RoomButton roomButtonPrefab;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();
    [Header("Create Room Menu")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private TMP_InputField createRoomInputField;
    [Header("Room Menu")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerNameTextPrefab;
    private List<TextMeshProUGUI> allPlayerNametexts = new List<TextMeshProUGUI>();
    [SerializeField] private GameObject startGameButton;
    [Header("Create Name Menu")]
    [SerializeField] private GameObject createNamePanel;
    [SerializeField] private TMP_InputField createNameInputField;
    [Header("Level Settings")]
    public string levelToPlay;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        HideAllMenus();
        ConnectToNetwork();
#if UNITY_EDITOR
        testRoomButton.SetActive(true);
#endif
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
        PhotonNetwork.AutomaticallySyncScene = true;
        SetLoadingText("Joining Lobby...");
    }

    public override void OnJoinedLobby() {
        base.OnJoinedLobby();
        if (!onStart) {
            ShowMainMenu();
        }
        else {
            onStart = false;
            if (PlayerPrefs.GetString("PlayerNickname", "") != "") {
                createNameInputField.text = PlayerPrefs.GetString("PlayerNickname", "");
            }
            ShowCreateNamePanel();
        }
    }
    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        SetRoomNameText(PhotonNetwork.CurrentRoom.Name);
        ListAllPlayers();
        ShowRoomPanel();
    }

    private void ListAllPlayers() {
        foreach (TextMeshProUGUI text in allPlayerNametexts) {
            Destroy(text.gameObject);
        }
        allPlayerNametexts.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        playerNameTextPrefab.gameObject.SetActive(true);
        for (int i = 0; i < players.Length; i++) {
            TextMeshProUGUI newPlayerNameText = Instantiate(playerNameTextPrefab, playerNameTextPrefab.transform.parent);
            newPlayerNameText.text = players[i].NickName;
            allPlayerNametexts.Add(newPlayerNameText);
        }
        playerNameTextPrefab.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient) {
            startGameButton.SetActive(true);
        }
        else {
            startGameButton.SetActive(false);
        }
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

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        base.OnRoomListUpdate(roomList);
        ClearRoomButtons();
        CreateAllRoomButtons(roomList);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer) {
        base.OnPlayerEnteredRoom(newPlayer);
        ListAllPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);
        ListAllPlayers();
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

    public void OpenJoinRoom() {
        ShowJoinRoomPanel();
    }
    public void CloseJoinRoom() {
        ShowMainMenu();
    }
    public void CloseCreateRoom() {
        ShowMainMenu();
    }

    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        ShowLoadingPanel();
        SetLoadingText("Joining Room...");
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void SetName() {
        if (string.IsNullOrEmpty(createNameInputField.text)) { return; }
        PlayerPrefs.SetString("PlayerNickname", createNameInputField.text);
        PhotonNetwork.NickName = createNameInputField.text;
        ShowMainMenu();
    }

    public void StartGame() {
        PhotonNetwork.LoadLevel(levelToPlay);
    }
    public void TestRoom() {
        RoomOptions rOptions = new RoomOptions();
        rOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom("Test", rOptions);
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

    public void ShowJoinRoomPanel() {
        HideAllMenus();
        joinRoomPanel.SetActive(true);
    }
    public void ShowCreateNamePanel() {
        HideAllMenus();
        createNamePanel.SetActive(true);
    }

    private void ClearRoomButtons() {
        foreach(RoomButton button in allRoomButtons) {
            Destroy(button.gameObject);
        }
        allRoomButtons.Clear();
    }

    private void CreateAllRoomButtons(List<RoomInfo> roomList) {
        roomButtonPrefab.gameObject.SetActive(true);
        for (int i = 0; i < roomList.Count; i++) {
            if (roomList[i].PlayerCount < roomList[i].MaxPlayers && !roomList[i].RemovedFromList) {
                RoomButton newButton = Instantiate(roomButtonPrefab,roomButtonPrefab.transform.parent);
                newButton.SetupRoomButton(roomList[i]);
                allRoomButtons.Add(newButton);
            }
        }
        roomButtonPrefab.gameObject.SetActive(false);
    }

    #endregion

    #region Hide
    private void HideAllMenus() {
        loadingPanel.SetActive(false);
        menuPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        joinRoomPanel.SetActive(false);
        createNamePanel.SetActive(false);
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
