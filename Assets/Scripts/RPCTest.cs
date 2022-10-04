using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class RPCTest : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private KeyCode RPCKey = KeyCode.R;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if(Input.GetKeyDown(RPCKey))
        {
            PhotonView pv = GameObject.Find("Obstacle").GetComponent<PhotonView>();
            pv.RPC("ChangeImage", RpcTarget.All);
        }
    }
}
