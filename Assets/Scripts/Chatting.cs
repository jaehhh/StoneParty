using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class Chatting : MonoBehaviourPunCallbacks
{
    // UI 오브젝트
    [SerializeField]
    private TextMeshProUGUI chattingBoxText; // 대화 로그창
    private TMP_InputField chattingInputField; // 입력중인 인풋필드
    [SerializeField]
    private Scrollbar chattingScrollbar;

    public GameObject player;


    private void Awake()
    {
        chattingInputField = GetComponent<TMP_InputField>();
    }

    // 인풋필드에서 입력 마치면 호출
    private void AddText()
    {
        if (chattingInputField.text != "")
        {
            string msg = $"[{PhotonNetwork.NickName}] {chattingInputField.text}";

            photonView.RPC("ReceiveChat", RpcTarget.All, msg); // RPC 전달

            msg = chattingInputField.text;
            player.GetComponent<PlayerManager>().ChattingBoxOn(msg); // 플레이어 말풍선 표시 함수

            chattingInputField.text = ""; // 인풋필드 초기화

            StartCoroutine(ScrollbarSet()); // 스크롤바 값 변경 코루틴
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
