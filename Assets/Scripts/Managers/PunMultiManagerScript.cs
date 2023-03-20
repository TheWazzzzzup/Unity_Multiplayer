using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunMultiManagerScript : MonoBehaviourPunCallbacks
{
    [Header("Room Controls")]
    [SerializeField] private Button roomButton;
    [SerializeField] private string roomName;

    [Header("Debug Text")]
    [SerializeField] private TextMeshProUGUI m_tmpg_Master;
    [SerializeField] private TextMeshProUGUI m_tmpg_Room;
    [SerializeField] private TextMeshProUGUI m_tmpg_RoomName;
    [SerializeField] private TextMeshProUGUI m_tmpg_RoomPlayerCount;

    public void PhotonPunLogin()
    {
        PhotonNetwork.ConnectUsingSettings();
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

    private void Update()
    {
        m_tmpg_Master.text = PhotonNetwork.NetworkClientState.ToString();
    }

    void RefreshRoomUI()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            m_tmpg_RoomName.text = PhotonNetwork.CurrentRoom.Name;
            m_tmpg_RoomPlayerCount.text = string.Format($"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        }
    }

    public void CreateOrJoinRoom()
    {
        roomButton.interactable = false;
        PhotonNetwork.JoinOrCreateRoom(roomName,new RoomOptions() { MaxPlayers = 2},null);
    }
}
