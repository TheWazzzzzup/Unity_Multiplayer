using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunMultiManagerScript : MonoBehaviourPunCallbacks
{

    public void PhotonPunLogin()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("MasterConnected - Yarok");
    }
}
