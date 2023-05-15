using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineGameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;

    const string PLAYER_PREFAB_NAME = "PlayerCapsule";

    private const string GAME_STATRTED_RPC = nameof(GameStarted);

    Vector3 StartLoc = new Vector3(0,1,0);

    bool hasGameStarted = false;

    PlayerController localPlayerController;

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView.RPC(GAME_STATRTED_RPC,RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    public void GameStarted()
    {
        hasGameStarted = true;
        localPlayerController.canControl = true;
        Debug.Log("You can now control the character");
    }

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Ready and connected");
            localPlayerController = PhotonNetwork.Instantiate(PLAYER_PREFAB_NAME, StartLoc, PlayerPrefab.transform.rotation).GetComponent<PlayerController>();
            GameStarted();
        }
    }
}
