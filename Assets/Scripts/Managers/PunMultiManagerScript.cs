using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PunMultiManagerScript : MonoBehaviourPunCallbacks
{
    [Header("Welcome Panel")]
    [SerializeField] private GameObject welcomePanel;
    [Space]
    [SerializeField] private TMP_InputField playerNickname;
    [SerializeField] private TMP_Text welcomePrompt;
    [SerializeField] private Button selectNickName;
    // Second Screen
    [SerializeField] private TMP_Text welcomePrompt2;
    [SerializeField] private Button joinServer;
    [Space]
    
    [Header("Lobby Panel")]
    [SerializeField] private GameObject lobbyPanel;
    [Space]

    [Header("Room Info Panel")]
    [SerializeField] TMP_Text masterStatus;
    [SerializeField] TMP_Text lobbyStatus;
    [Space]
    [SerializeField] private TMP_Text chooseRoomPrompt;
    [SerializeField] private TMP_InputField chooseRoomInputField;
    [SerializeField] private Button createRoomButton;
    [Space]
    [SerializeField] private TMP_Text selctedRoomName;
    [SerializeField] private TMP_Text selectedRoomListPrompt;
    [SerializeField] private TMP_Text selctedRoomPlayerList;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button exitButton;
    [Space]

    [Header("Room Scroll View")]
    [SerializeField] GameObject scrollViewContext;
    [SerializeField] GameObject scrollbarVertical;
    [SerializeField] GameObject roomUIPrefab;
    [SerializeField] TMP_Text defualtScrollPrompt;
    [Space]

    [Header("Room Panel")]
    [SerializeField] private GameObject roomPanel;
    [Space]

    [SerializeField] private TMP_Text roomPanelRoomName;
    [SerializeField] private TMP_Text roomPanelRoomNumberOfPlayer;
    [SerializeField] private Button startOrJoinGameButton;
    [SerializeField] private TMP_Text startOrJoinGameText;
    [Space]

    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private GameObject playerUIContext;

    [Header("Player")]
    [SerializeField] private GameObject PlayerPrefab;
    const string PLAYER_PREFAB_NAME = "PlayerCapsule";


    private bool isMasterClient => PhotonNetwork.IsMasterClient;

    private string currentSelctedRoom;

    List<GameObject> UIRoomList => new();

    #region Event Methods
    public void NickNameCreated()
    {
        if (playerNickname.text.Length >= 3)
        {
            PhotonNetwork.NickName = playerNickname.text;

            welcomePrompt.gameObject.SetActive(false);
            selectNickName.gameObject.SetActive(false);
            playerNickname.gameObject.SetActive(false);



            welcomePrompt2.text = $"Welcome {PhotonNetwork.NickName} press the button to join the server";
            welcomePrompt2.gameObject.SetActive(true);
            joinServer.gameObject.SetActive(true);
        }
        else
        {
            welcomePrompt.color = Color.red;
        }
    }

    public void PhotonPunLogin()
    {
        PhotonNetwork.ConnectUsingSettings();
        if (lobbyPanel != null)
        {
            welcomePanel.gameObject.SetActive(false);
            lobbyPanel.gameObject.SetActive(true);
            createRoomButton.interactable = false;
        }
    }
    
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(chooseRoomInputField.text, new RoomOptions() { MaxPlayers = 20 },null);
        createRoomButton.interactable = false;
    }

    #endregion

    #region Unity Methods
    private void Start()
    {
        if (welcomePanel != null && lobbyPanel != null)
        {
            welcomePanel.gameObject.SetActive(true);
            lobbyPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("One of the panels was not found");
        }
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.InLobby && createRoomButton.gameObject.activeInHierarchy)
        {
            if (chooseRoomInputField.text.Length > 3) createRoomButton.interactable = true;
            else createRoomButton.interactable = false;
        }
    }


    #endregion

    #region PhotonNetwork Overrides
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        if (masterStatus != null)
        {
            masterStatus.color = Color.green;
            masterStatus.text = "Connected to Master";
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        if (masterStatus != null)
        {
            masterStatus.color = Color.red;
            masterStatus.text = "Disconnected from Master";
        }
    }
    
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        if (lobbyStatus != null)
        {
            lobbyStatus.color = Color.green;
            lobbyStatus.text = "Connected to Lobby";
        }
    }
    
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        if (lobbyStatus != null)
        {
            lobbyStatus.color = Color.red;
            lobbyStatus.text = "Disconnted from Lobby";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        UIRoomClear();

        Debug.Log("OnRoomListUpdate Override Called");

        if (PhotonNetwork.CountOfRooms > 0)
        {
            defualtScrollPrompt.gameObject.SetActive(false);
            Debug.Log("Rooms Created");
            foreach (var roominfo in roomList)
            {
                if (roominfo.PlayerCount > 0)
                {
                    UIRoomInstantion(roominfo);
                }
            }
        }
        else
        {
            defualtScrollPrompt.gameObject.SetActive(true);
            Debug.Log("Room List Updated But No Rooms Was Found");
        }
    }
    
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        joinRoomButton.interactable = false;
        selctedRoomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            selctedRoomPlayerList.text = $"{player.NickName}\n";
        }
        CreateRoomSwitch(false);
        LobbyToRoomSwitch(true);
        RoomHandler();
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        createRoomButton.interactable = true;
        chooseRoomPrompt.text = "Failed to Create Room";
    }
    
    
    #endregion

    #region Debug

    [ContextMenu("DebugServerDisconnect")]
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    [ContextMenu("DebugNumberOfRooms")]
    public void NumberOfRooms() => Debug.Log(PhotonNetwork.CountOfRooms);

    [ContextMenu("DebugLobbyDisconnect")]
    public void LobbyDisconnect() => PhotonNetwork.LeaveLobby();

    [ContextMenu("DebugLobbyConnect")]
    public void LobbyConnect() => PhotonNetwork.JoinLobby();
    #endregion


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        joinRoomButton.interactable = false;
        CreateRoomSwitch(false);
        LobbyToRoomSwitch(true);
        RoomHandler();
        Debug.Log("JoinedRoom");

    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Failed To Join");
    }
    
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        LobbyToRoomSwitch(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // refresh room ui! like on room list update

        base.OnPlayerEnteredRoom(newPlayer);
        RoomHandler();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RoomHandler();
    }


    [ContextMenu("LoadLevel")]
    public void StartAndLoadLevel()
    {
        PhotonNetwork.LoadLevel(1);
        PhotonNetwork.Instantiate(PLAYER_PREFAB_NAME, Vector3.zero,PlayerPrefab.transform.rotation);
    }
    



    public void RoomPicked(RoomInfo roominfo)
    {
        if (roominfo != null)
        {
            Debug.Log("HasInfo");
            selctedRoomName.text = "Room: " + roominfo.Name;
            selctedRoomPlayerList.text = $"{roominfo.PlayerCount}/{roominfo.MaxPlayers}";
            CreateRoomSwitch(false);
            joinRoomButton.interactable = true;
            currentSelctedRoom = roominfo.Name;
            joinRoomButton.onClick.AddListener(JoinRoom);
        }
        Debug.Log("!HasInfo");
    }

    void JoinRoom()
    {
        PhotonNetwork.JoinRoom(currentSelctedRoom,null);
    }

    void UIRoomInstantion(RoomInfo roominfo)
    {
        var tmpTempList = roomUIPrefab.GetComponentsInChildren<TMP_Text>();
        tmpTempList[0].text = roominfo.Name;
        tmpTempList[1].text = $"{roominfo.PlayerCount}/{roominfo.MaxPlayers}";
        roomUIPrefab.GetComponentInChildren<MyRoomInfo>().SetRoomInfo(roominfo);
        UIRoomList.Add(Instantiate<GameObject>(roomUIPrefab, scrollViewContext.transform));
    }

    public void SelectedRoomsCount(int index) => currentSelctedRoom += index;


    public void RoomHandler()
    {
        // destroy player list on every update
        var childcount = playerUIContext.transform.childCount;
        for (int i = 0; i < childcount; i++)
        {
            if (playerUIContext.transform.GetChild(i).tag == "Destructable")
            {
                Debug.Log("PlayerListCleard");
                Destroy(playerUIContext.transform.GetChild(i).gameObject);
            }
        }


        if (roomPanel.activeInHierarchy)
        {
            roomPanelRoomName.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
            roomPanelRoomNumberOfPlayer.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

            if (isMasterClient)
            {
                startOrJoinGameButton.interactable = false;
                startOrJoinGameText.text = "Start Game";
            }
            else
            {
                startOrJoinGameButton.interactable = false;
                startOrJoinGameText.text = "Join Game";
            }
        }

        Debug.Log("Players:");
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            Debug.Log($"{player.NickName}");
            var playerUI = Instantiate(playerUIPrefab,playerUIContext.transform);
            TMP_Text[] playerListUI= playerUI.GetComponentsInChildren<TMP_Text>();
            playerListUI[0].text = player.NickName;
            playerListUI[1].text = "N/A";
        }

        if (isMasterClient)
        {
            int children = playerUIContext.transform.childCount;
            for (int i = 0; i < children; i++)
            {
                var child = playerUIContext.transform.GetChild(i);
                var button = child.GetComponentInChildren<Button>();
            }
        }
    }

    public void LobbyToRoomSwitch(bool switchToRoom)
    {
        if (lobbyPanel != null && roomPanel != null) 
        if (switchToRoom)
        {
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(true);
        }
        else
        {
                lobbyPanel.SetActive(true);
                roomPanel.SetActive(false);
            }
    }

    public void CreateRoomSwitch(bool createRoom)
    {
        Debug.Log($"Room Switch is called with {createRoom} boolean");
        if (createRoom)
        {
            chooseRoomInputField.gameObject.SetActive(true);
            chooseRoomPrompt.gameObject.SetActive(true);
            createRoomButton.gameObject.SetActive(true);
            //
            selctedRoomName.gameObject.SetActive(false);
            selctedRoomPlayerList.gameObject.SetActive(false);
            selectedRoomListPrompt.gameObject.SetActive(false);
            joinRoomButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
        }
        else
        {
            chooseRoomInputField.gameObject.SetActive(false);
            chooseRoomPrompt.gameObject.SetActive(false);
            createRoomButton.gameObject.SetActive(false);
            //
            selctedRoomName.gameObject.SetActive(true);
            selctedRoomPlayerList.gameObject.SetActive(true);
            selectedRoomListPrompt.gameObject.SetActive(true);
            joinRoomButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
        }
    }



    #region Handlers
    void UIRoomClear()
    {
        var childcount = scrollViewContext.transform.childCount;
        for (int i = 0; i < childcount; i++)
        {
            if (scrollViewContext.transform.GetChild(i).tag == "Destructable")
            {
                Destroy(scrollViewContext.transform.GetChild(i).gameObject);
            }
        }
    }
    
    
    
    
    #endregion

}
