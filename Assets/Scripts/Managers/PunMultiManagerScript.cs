using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunMultiManagerScript : MonoBehaviourPunCallbacks
{
    [Header("Welcome Panel")]
    [SerializeField] private GameObject WelcomePanel;
    [SerializeField] private TMP_InputField playerNickname;
    [SerializeField] private TMP_Text welcomePrompt;
    [SerializeField] private Button selectNickName;
    // Second Screen
    [SerializeField] private TMP_Text welcomePrompt2;
    [SerializeField] private Button joinServer;

    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;


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

    private bool isMasterClient => PhotonNetwork.IsMasterClient;


    public void NickNameCreated()
    {
        if (playerNickname.text.Length > 3)
        {
            Debug.Log("not Null");
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
        WelcomePanel.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);

    }

    private void Start()
    {
        m_tmpg_Room.text = "No";
        m_tmpg_RoomName.text = "";
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("<color=#00ff00>Master Is Connected</color>");
        roomButton.interactable = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Inside a room");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        m_tmpg_Room.text = "Connected To Room";
        RefreshRoomUI();
        Debug.Log("Joined Room");
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

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RefreshRoomUI();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Failed To Connect To Room");
        m_tmpg_Room.text = "Failed To Connect To Room";
    }

    [ContextMenu("RoomCount")]
    public void GetRoomCount() => Debug.Log(PhotonNetwork.CountOfRooms);

    private void Update()
    {
        m_tmpg_Master.text = PhotonNetwork.NetworkClientState.ToString();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("OnRoomListUpdateCalled");
        base.OnRoomListUpdate(roomList);
        var x = 0;
        foreach (var roominfo in roomList)
        {
            x++;
            Debug.Log($"{x}.{roominfo.Name}");
        }
    }

    void RefreshRoomUI()
    {
        Debug.Log(PhotonNetwork.CountOfRooms);
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

    public void CreateOrJoinRoom()
    {
        roomButton.interactable = false;
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions() { MaxPlayers = 20 },null);
    }
}
