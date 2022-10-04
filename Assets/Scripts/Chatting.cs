using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class Chatting : MonoBehaviourPunCallbacks
{
    // UI ������Ʈ
    [SerializeField]
    private TextMeshProUGUI chattingBoxText; // ��ȭ �α�â
    private TMP_InputField chattingInputField; // �Է����� ��ǲ�ʵ�
    [SerializeField]
    private Scrollbar chattingScrollbar;

    public GameObject player;


    private void Awake()
    {
        chattingInputField = GetComponent<TMP_InputField>();
    }

    // ��ǲ�ʵ忡�� �Է� ��ġ�� ȣ��
    private void AddText()
    {
        if (chattingInputField.text != "")
        {
            string msg = $"[{PhotonNetwork.NickName}] {chattingInputField.text}";

            photonView.RPC("ReceiveChat", RpcTarget.All, msg); // RPC ����

            msg = chattingInputField.text;
            player.GetComponent<PlayerManager>().ChattingBoxOn(msg); // �÷��̾� ��ǳ�� ǥ�� �Լ�

            chattingInputField.text = ""; // ��ǲ�ʵ� �ʱ�ȭ

            StartCoroutine(ScrollbarSet()); // ��ũ�ѹ� �� ���� �ڷ�ƾ
        }
    }

    public IEnumerator ScrollbarSet()
    {
        yield return null;
        yield return null;

        chattingScrollbar.value = 0f;
    }

    [PunRPC]
    private void ReceiveChat(string chat)
    {
        if(chattingBoxText.text == "")
        {
            chattingBoxText.text += chat;
        }
        else
        {
            chattingBoxText.text += "\n" + chat;
        }  
    }
}
