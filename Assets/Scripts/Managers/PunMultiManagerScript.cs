using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

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
    [SerializeField] private TMP_Text selctedRoomPlayerCount;
    [SerializeField] private Button joinRoomButton;
    [Space]
    [Header("Room Scroll View")]
    [SerializeField] GameObject scrollViewContext;
    [SerializeField] GameObject scrollbarVertical;
    [SerializeField] GameObject roomUIPrefab;
    [SerializeField] TMP_Text defualtPrompt;

    #region old
    [Header("Room Controls")]
    [SerializeField] private Button roomButton;
    [SerializeField] private string roomName;
    [SerializeField] private Button startGameButton;

    [SerializeField] TMP_InputField nicknameInputField;
    [Header("Debug Text")]
    [SerializeField] private TextMeshProUGUI m_tmpg_Master;
    [SerializeField] private TextMeshProUGUI m_tmpg_Room;
    [SerializeField] private TextMeshProUGUI m_tmpg_RoomName;
    [SerializeField] private TextMeshProUGUI m_tmpg_RoomPlayerCount;
    [SerializeField] private TextMeshProUGUI m_tmpg_PlayerListText;

    [Header("UI's")]
    [SerializeField] private CanvasRenderer panel_SecondUI;
    [SerializeField] private CanvasRenderer panel_FirstUI;
    [SerializeField] private TextMeshProUGUI m_tmpg_2ndUIPrompt;
    #endregion

    private bool isMasterClient => PhotonNetwork.IsMasterClient;

    Dictionary<int, GameObject> Rooms;
    int dicCount = 0;

    List<RoomInfo> ManagerRoomList => new();

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

    #endregion


    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        CreateRoomSwitch(false);
    }

        
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        m_tmpg_Room.text = "Connected To Room";
        RefreshRoomUI();
        Debug.Log("Joined Room");
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RefreshRoomUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"Player name is {newPlayer.NickName}");
        RefreshRoomUI();

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            startGameButton.interactable = true;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        RefreshRoomUI();
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Failed To Connect To Room");
        m_tmpg_Room.text = "Failed To Connect To Room";
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        Debug.Log("OnRoomListUpdate Override Called");

        if (PhotonNetwork.CountOfRooms > 0)
        {
            Debug.Log("Rooms Created");
            foreach (var roominfo in roomList)
            {
                if (roominfo.PlayerCount > 0)
                {
                    ManagerRoomList.Add(roominfo);
                    UIRoomCreation(roominfo);
                }
                else
                {
                    foreach (var localRoom in ManagerRoomList)
                    {
                        if (localRoom.Name == roominfo.Name)
                        {
                            ManagerRoomList.Remove(localRoom);
                            UIRoomDestruction(localRoom);
                        }
                    }
                }
            }
        }

        else
        {
            Debug.Log("Room List Updated But No Rooms Was Found");
        }
    }

    void UIRoomCreation(RoomInfo roominfo)
    {
        var currentBotton = Instantiate<GameObject>(roomUIPrefab, scrollViewContext.transform);
        Rooms.Add(dicCount, currentBotton);
        dicCount++;
        var tmpTempList = currentBotton.GetComponentsInChildren<TMP_Text>();
        for (int i = 0; i < tmpTempList.Length; i++)
        {
            var tmp = tmpTempList[i];
            tmpTempList[0].text = roominfo.Name;
            tmpTempList[1].text = $"{roominfo.PlayerCount}/{roominfo.MaxPlayers}";
        }
    }

    void UIRoomDestruction(RoomInfo roomToDestroy)
    {
        foreach(var roominfo in Rooms)
        {
            if (roominfo.Value.name == roomToDestroy.Name)
            {
                int loc = roominfo.Key;
                Destroy(roominfo.Value);
                Rooms.Remove(roominfo.Key);
                for (int i = loc + 1;i < Rooms.Count; i++)
                {

                }
            }
        }
    }

    void RefreshRoomUI()
    {
        m_tmpg_PlayerListText.text = null;
        if (PhotonNetwork.CurrentRoom != null)
        {
            m_tmpg_RoomName.text = PhotonNetwork.CurrentRoom.Name;
            m_tmpg_RoomPlayerCount.text = string.Format($"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
            //panel_SecondUI.gameObject.SetActive(true);
            //panel_FirstUI.gameObject.SetActive(false);
            foreach (var player in PhotonNetwork.PlayerList)
            {
                m_tmpg_PlayerListText.text += player.NickName + "\n";
            }
        }
        //if (PhotonNetwork.CurrentRoom == null)
        //{
        //    panel_SecondUI.gameObject.SetActive(false);
        //    panel_FirstUI.gameObject.SetActive(true);
        //}
    }

    public void CreateRoomSwitch(bool createRoom)
    {
        if (createRoom)
        {
            chooseRoomInputField.gameObject.SetActive(true);
            chooseRoomPrompt.gameObject.SetActive(true);
            createRoomButton.gameObject.SetActive(true);
            //
            selctedRoomName.gameObject.SetActive(false);
            selctedRoomPlayerCount.gameObject.SetActive(false);
            selectedRoomListPrompt.gameObject.SetActive(false);
            joinRoomButton.gameObject.SetActive(false);
        }
        else
        {
            chooseRoomInputField.gameObject.SetActive(false);
            chooseRoomPrompt.gameObject.SetActive(false);
            createRoomButton.gameObject.SetActive(false);
            //
            selctedRoomName.gameObject.SetActive(true);
            selctedRoomPlayerCount.gameObject.SetActive(true);
            selectedRoomListPrompt.gameObject.SetActive(true);
            joinRoomButton.gameObject.SetActive(true);
        }
    }
}
