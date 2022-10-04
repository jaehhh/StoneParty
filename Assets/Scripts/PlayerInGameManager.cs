using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

// �ΰ��� ���� �÷��̾ �����ϴ� ��ũ��Ʈ

public class PlayerInGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject[] particleDeath;

    private MainGameManager mainGameManager;
    public MainGameManager MainGameManager
    {
        get { return mainGameManager; }
    }

    [SerializeField]
    private TextMeshProUGUI playerNameTagText;

    [SerializeField]
    private Material[] mats;

    // UserData UserData.instance;

    public void Awake()
    {
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        Setup();
    }

    private void Setup()
    {
        if (!photonView.IsMine) return;

        // ���ΰ��ӸŴ����� �̺�Ʈ�Լ��� �÷��̾��ڽ��� �����Ӱ��ɻ�����ȯ �޼ҵ带 ���
        mainGameManager = GameObject.FindObjectOfType<MainGameManager>().GetComponent<MainGameManager>();
        mainGameManager.gameStateChangeEvent.AddListener(MovePossibleState);

        string name = PhotonNetwork.NickName;
        photonView.RPC("PlayerSetup", RpcTarget.AllBuffered, name);

        // ����.Ŀ����������Ƽ �ʱ�ȭ
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReady", false } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReadyInGame", false } });
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        if((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            photonView.RPC("RPCMaterialChange", RpcTarget.AllBuffered, 0);
            this.transform.Find("Ball").tag = "PlayerBlue";
        }
        else
        {
            photonView.RPC("RPCMaterialChange", RpcTarget.AllBuffered, 1);
            this.transform.Find("Ball").tag = "PlayerOrange";
        }
        
    }

    // �÷��̾� �����±�
    [PunRPC]
    private void PlayerSetup(string name)
    {
        playerNameTagText.text = name;
    }

    // RPC ���׸��� ����
    [PunRPC]
    private void RPCMaterialChange(int matNum)
    {
        if (matNum == 0)
        {
            this.transform.Find("Ball").tag = "PlayerBlue";
        }
        else
        {
            this.transform.Find("Ball").tag = "PlayerOrange";
        }

        this.GetComponentInChildren<MeshRenderer>().material = mats[matNum];
    }

    // MoveController�κ��� ȣ��Ǵ� ���� ������Ʈ "Ball"�� Renderer ���� Ű��
    public void RendererEnable(bool value)
    {
        photonView.RPC("RPCRendererSetActive", RpcTarget.AllBuffered, value);
    }

    // RPC ���� ���߱�
    [PunRPC]
    private void RPCRendererSetActive(bool value)
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        for(int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = value;
        }
    }

    private void MovePossibleState(bool value)
    {
        gameObject.GetComponentInChildren<MoveController>().canMove = value;
    }

    // �ڽ� ������Ʈ "Ball"���� ���ó�� �Ǿ��� �� �θ��� PhotonView�� �̿��Ͽ� PhotonNetwork.Instantiate()���
    public void CreateParticleDeath(Vector3 pos)
    {

        int num;

        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            num = 0;
        }
        else
        {
            num = 1;
        }

        photonView.RPC("RPCCreateParticleDeath", RpcTarget.AllBufferedViaServer, pos, num);
    }

    [PunRPC]
    private void RPCCreateParticleDeath(Vector3 pos, int num)
    {
        Instantiate(particleDeath[num], pos, Quaternion.identity);
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
        if (itemCode == 0)
        {
            return;
        }

        string itemPath = "Item/Customizing Item" + itemCode.ToString();

        // ������ ������
        CustomizingItem customizingItem = Resources.Load(itemPath) as CustomizingItem;

        Instantiate(customizingItem.object3D, this.gameObject.transform.Find("Ball"));
    }

}
