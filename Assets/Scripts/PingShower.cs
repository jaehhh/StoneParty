using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PingShower : MonoBehaviourPunCallbacks
{
    private int ping;
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        UpdatePing();
    }

    private void UpdatePing()
    {
        ping = PhotonNetwork.GetPing();

        text.text = $"Ping : {ping}";

        Invoke("UpdatePing", 0.7f);
    }
}