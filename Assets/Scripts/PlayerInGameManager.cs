using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

// 인게임 씬의 플레이어를 제어하는 스크립트

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

        // 메인게임매니져의 이벤트함수에 플레이어자신의 움직임가능상태전환 메소드를 등록
        mainGameManager = GameObject.FindObjectOfType<MainGameManager>().GetComponent<MainGameManager>();
        mainGameManager.gameStateChangeEvent.AddListener(MovePossibleState);

        string name = PhotonNetwork.NickName;
        photonView.RPC("PlayerSetup", RpcTarget.AllBuffered, name);

        // 포톤.커스텀프로퍼티 초기화
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

    // 플레이어 네임태그
    [PunRPC]
    private void PlayerSetup(string name)
    {
        playerNameTagText.text = name;
    }

    // RPC 머테리얼 변경
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

    // MoveController로부터 호출되는 하위 오브젝트 "Ball"의 Renderer 끄고 키기
    public void RendererEnable(bool value)
    {
        photonView.RPC("RPCRendererSetActive", RpcTarget.AllBuffered, value);
    }

    // RPC 외형 감추기
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

    // 자식 오브젝트 "Ball"에서 사망처리 되었을 때 부모의 PhotonView를 이용하여 PhotonNetwork.Instantiate()사용
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
        if (itemCode == 0)
        {
            return;
        }

        string itemPath = "Item/Customizing Item" + itemCode.ToString();

        // 장착될 아이템
        CustomizingItem customizingItem = Resources.Load(itemPath) as CustomizingItem;

        Instantiate(customizingItem.object3D, this.gameObject.transform.Find("Ball"));
    }

}
