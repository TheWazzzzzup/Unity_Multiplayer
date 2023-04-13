using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Realtime;

public class MyRoomInfo : MonoBehaviour , IDeselectHandler
{
    public static RoomInfo RoomInfo { get; private set; }

    PunMultiManagerScript m_Script;

    Button m_Button;

    UnityAction m_Action;

    private void Start()
    {
        m_Script = GetComponentInParent<PunMultiManagerScript>();
        m_Button = GetComponent<Button>();
        if (m_Script != null) Debug.Log("ParentScriptFound");
    }

    private void FixedUpdate()
    {
        m_Button.onClick.AddListener(SendMe);
    }

    public void SetRoomInfo(RoomInfo roominfo)
    {
        RoomInfo = roominfo;
    }

    void SendMe()
    {
        m_Script.RoomPicked(RoomInfo);
        m_Script.SelectedRoomsCount(1);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        m_Script.SelectedRoomsCount(-1);
    }
}
