using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

// ���� ���� Player

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

            // ����.Ŀ����������Ƽ �ʱ�ȭ
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
        // ù ���忡�� ����. ������ ������ ���ƿ��� �������� ����
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
        // ���� ������ ������� ȣ��
        else if((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            photonView.RPC("RPCChangeMat", RpcTarget.AllBuffered, 0);
        }
        else if((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "orange")
        {
            photonView.RPC("RPCChangeMat", RpcTarget.AllBuffered, 1);
        }
    }

    // ���׸��� �迭�� �����տ� ���� ������
    // ����� ������� �ʴ� �޼ҵ�
    [PunRPC]
    private void RPCSetMat()
    {
        Material[] tempMats = new Material[2];
        tempMats = roomManager.mats;
        mats = tempMats;
    }

    // ��ǳ��RPCȣ��, ��ǳ�� ����� �ð� ���
    public void ChattingBoxOn(string msg)
    {
        photonView.RPC("RPCChattingBoxOn", RpcTarget.All, msg);

        StopCoroutine("ChattingBoxDisappear");
        StartCoroutine("ChattingBoxDisappear");
    }

    // RPC ��ǳ�� ����
    [PunRPC]
    private void RPCChattingBoxOn(string msg)
    {
        chattingBox.SetActive(true);
        chattingText.text = msg;
    }

    // ��ǳ�� ����� �ڷ�ƾ
    private IEnumerator ChattingBoxDisappear()
    {
        yield return new WaitForSeconds(disappearTime);

        photonView.RPC("RPCChattingBoxDisappear", RpcTarget.All);
    }

    // RPC ��ǳ�� �����
    [PunRPC]
    private void RPCChattingBoxDisappear()
    {
        chattingBox.SetActive(false);
    }

    // ���� UI
    public void Ready(bool value)
    {
        photonView.RPC("RPCReady", RpcTarget.AllBuffered, value);
    }

    // ����UI RPC
    [PunRPC]
    private void RPCReady(bool value)
    {
        readyUI.SetActive(value);  
    }

    // ��ư ���� �� ȣ��Ǵ� ������ �޼ҵ�
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

    // RPC ���׸��� ����
    [PunRPC]
    private IEnumerator RPCChangeMat(int matNum)
    {
        yield return new WaitForSeconds(0.07f);

        this.GetComponentInChildren<MeshRenderer>().material = mats[matNum];
    }

    public void EquipItem(int itemCode = 0)
    {
        // �Ű����� �� = 0 : ���� ������ �������� �Ծ��, != 0 : ������ �������� �Ծ��
        if (itemCode == 0)
        {
            // ������ ������ ������
            itemCode = UserData.instance.EquippedItemCode;
        }

        photonView.RPC("RPCEquipItem", RpcTarget.AllBufferedViaServer, itemCode);
    }

    [PunRPC]
    private void RPCEquipItem(int itemCode)
    {
        // �ƹ��͵� �������� ����
        if(itemCode == 0)
        {
            return;
        }

        string itemPath = "Item/Customizing Item" + itemCode.ToString();

        // ������ ������
        CustomizingItem customizingItem = Resources.Load(itemPath) as CustomizingItem;

        Instantiate(customizingItem.object3D, this.gameObject.transform.Find("Ball"));
    }
}