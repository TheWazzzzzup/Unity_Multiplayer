using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class PunMultiManagerScript : MonoBehaviourPunCallbacks
{
    [Header("Room Controls")]
    [SerializeField] private Button RoomButton;

    [Header("Debug Text")]
    [SerializeField] private TextMeshProUGUI m_tmpg_Master;
    [SerializeField] private TextMeshProUGUI m_tmpg_Room;

    public void PhotonPunLogin()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Start()
    {
        m_tmpg_Room.text = "No";
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("<color=#00ff00>Master Is Connected</color>");
        RoomButton.interactable = true;

    }

    private void Update()
    {
        m_tmpg_Master.text = PhotonNetwork.NetworkClientState.ToString();
    }
}
