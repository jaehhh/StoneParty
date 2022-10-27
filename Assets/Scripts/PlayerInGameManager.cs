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
    // Ÿ ������Ʈ�� ������Ʈ ����
    [HideInInspector]
    public ParticleManager particleManager;
    private MainGameManager mainGameManager;
    public MainGameManager MainGameManager
    {
        get { return mainGameManager; }
    }

    // �� ������Ʈ ������Ʈ ����
    [SerializeField]
    private TextMeshProUGUI playerNameTagText;

    // ��ġ�� �� ������Ʈ�� �޸𸮷� ����
    [SerializeField]
    private MeshRenderer oreMesh;
    [SerializeField]
    private Material[] mats;
    [SerializeField]
    private Color[] colors;

    public void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        if (!photonView.IsMine) return;

        // ���ΰ��ӸŴ����� �̺�Ʈ�Լ��� �÷��̾��ڽ��� �����Ӱ��ɻ�����ȯ �޼ҵ带 ���
        mainGameManager = GameObject.FindObjectOfType<MainGameManager>().GetComponent<MainGameManager>();
        particleManager = mainGameManager.GetComponent<ParticleManager>();
        mainGameManager.gameStateChangeEvent.AddListener(MovePossibleState);

        string name = PhotonNetwork.NickName;
        int colorIndex = (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue" ? 0 : 1;
        photonView.RPC("PlayerSetup", RpcTarget.AllBuffered, name, colorIndex);

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
    private void PlayerSetup(string name, int color)
    {
        playerNameTagText.text = name;
        playerNameTagText.color = colors[color];
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

        oreMesh.material = mats[matNum];
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

    // �ڽ� ������Ʈ "Ball"���� ���ó�� �Ǿ��� �� �ش� �޼ҵ尡 �Ҹ��� �޼ҵ忡�� RPCȣ���Ѵ��� RPC���� ParticleManagerȣ��
    public void CreateDeathParticle(Vector3 pos)
    {
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            photonView.RPC("RPCCreateDeathParticle", RpcTarget.AllBufferedViaServer, 0, pos);
        }
        else
        {
            photonView.RPC("RPCCreateDeathParticle", RpcTarget.AllBufferedViaServer, 1, pos);
        }
    }

    [PunRPC]
    private void RPCCreateDeathParticle(int num, Vector3 pos)
    {
        particleManager.ActiveDeathParticle(num, pos);
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
