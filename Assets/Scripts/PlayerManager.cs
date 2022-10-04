using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

// 대기방 씬의 Player

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject chattingBox;
    [SerializeField]
    private TextMeshProUGUI chattingText;
    [SerializeField]
    private TextMeshProUGUI playerNameTagText;
    [SerializeField]
    private GameObject readyUI;

    public Material[] mats;

    [HideInInspector]
    public RoomManager roomManager;

    private float disappearTime = 5;

    // UserData UserData.instance;

    private void Awake()
    {
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        if (photonView.IsMine)
        {
            Chatting chatting = GameObject.FindObjectOfType<Chatting>().GetComponent<Chatting>();
            GameObject.FindObjectOfType<RoomManager>().GetComponent<RoomManager>().player = this.gameObject;

            chatting.player = this.gameObject;

            string name = PhotonNetwork.NickName;
            photonView.RPC("PlayerSetup", RpcTarget.AllBuffered, name);

            // 포톤.커스텀프로퍼티 초기화
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReady", false } });
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReadyInGame", false } });
        }
    }

    private void Start()
    {
        if(photonView.IsMine)
        {
            TeamSetting();
        }
    }

    [PunRPC]
    private void PlayerSetup(string name)
    {
        playerNameTagText.text = name;

        chattingBox.SetActive(false);
    }

    private void TeamSetting()
    {
        // 첫 입장에만 세팅. 게임을 끝내고 돌아오면 세팅하지 않음
        if (PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == null)
        {
            int blueCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["blueTeamCount"];
            int orangeCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["orangeTeamCount"];

            if (blueCount - orangeCount <= 0)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "teamColor", "blue" } });
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", blueCount + 1 } });

                photonView.RPC("RPCChangeMat", RpcTarget.AllBuffered, 0);
            }
            else
            {  
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "teamColor", "orange" } });
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "orangeTeamCount", orangeCount + 1 } });

                photonView.RPC("RPCChangeMat", RpcTarget.AllBuffered, 1);
            }
        }
        // 게임 끝나고 재입장시 호출
        else if((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            photonView.RPC("RPCChangeMat", RpcTarget.AllBuffered, 0);
        }
        else if((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "orange")
        {
            photonView.RPC("RPCChangeMat", RpcTarget.AllBuffered, 1);
        }
    }

    // 머테리얼 배열은 프리팹에 직접 설정중
    // 현재는 사용하지 않는 메소드
    [PunRPC]
    private void RPCSetMat()
    {
        Material[] tempMats = new Material[2];
        tempMats = roomManager.mats;
        mats = tempMats;
    }

    // 말풍선RPC호출, 말풍선 사라짐 시간 계산
    public void ChattingBoxOn(string msg)
    {
        photonView.RPC("RPCChattingBoxOn", RpcTarget.All, msg);

        StopCoroutine("ChattingBoxDisappear");
        StartCoroutine("ChattingBoxDisappear");
    }

    // RPC 말풍선 생성
    [PunRPC]
    private void RPCChattingBoxOn(string msg)
    {
        chattingBox.SetActive(true);
        chattingText.text = msg;
    }

    // 말풍선 사라짐 코루틴
    private IEnumerator ChattingBoxDisappear()
    {
        yield return new WaitForSeconds(disappearTime);

        photonView.RPC("RPCChattingBoxDisappear", RpcTarget.All);
    }

    // RPC 말풍선 사라짐
    [PunRPC]
    private void RPCChattingBoxDisappear()
    {
        chattingBox.SetActive(false);
    }

    // 레디 UI
    public void Ready(bool value)
    {
        photonView.RPC("RPCReady", RpcTarget.AllBuffered, value);
    }

    // 레디UI RPC
    [PunRPC]
    private void RPCReady(bool value)
    {
        readyUI.SetActive(value);  
    }

    // 버튼 누를 때 호출되는 팀변경 메소드
    public void ChangeTeam()
    {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"]) return;

        int blueCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["blueTeamCount"];
        int orangeCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["orangeTeamCount"];

        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "orange")
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "teamColor", "blue" } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", blueCount + 1 } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "orangeTeamCount", orangeCount - 1 } });

            photonView.RPC("RPCChangeMat", RpcTarget.AllBufferedViaServer, 0);
        }
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "teamColor", "orange" } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "orangeTeamCount", orangeCount + 1 } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", blueCount - 1 } });

            photonView.RPC("RPCChangeMat", RpcTarget.AllBufferedViaServer, 1);
        }
    }

    // RPC 머테리얼 변경
    [PunRPC]
    private IEnumerator RPCChangeMat(int matNum)
    {
        yield return new WaitForSeconds(0.07f);

        this.GetComponentInChildren<MeshRenderer>().material = mats[matNum];
    }

    public void EquipItem(int itemCode = 0)
    {
        // 매개변수 값 = 0 : 현재 장착된 아이템을 입어라, != 0 : 지정한 아이템을 입어라
        if (itemCode == 0)
        {
            // 장착된 아이템 데이터
            itemCode = UserData.instance.EquippedItemCode;
        }

        photonView.RPC("RPCEquipItem", RpcTarget.AllBufferedViaServer, itemCode);
    }

    [PunRPC]
    private void RPCEquipItem(int itemCode)
    {
        // 아무것도 장착하지 않음
        if(itemCode == 0)
        {
            return;
        }

        string itemPath = "Item/Customizing Item" + itemCode.ToString();

        // 장착될 아이템
        CustomizingItem customizingItem = Resources.Load(itemPath) as CustomizingItem;

        Instantiate(customizingItem.object3D, this.gameObject.transform.Find("Ball"));
    }
}