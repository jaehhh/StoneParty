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
    // 타 오브젝트의 컴포넌트 참조
    [HideInInspector]
    public ParticleManager particleManager;
    private MainGameManager mainGameManager;
    public MainGameManager MainGameManager
    {
        get { return mainGameManager; }
    }

    // 내 오브젝트 컴포넌트 참조
    [SerializeField]
    private TextMeshProUGUI playerNameTagText;

    // 수치값 내 오브젝트에 메모리로 보유
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

        // 메인게임매니져의 이벤트함수에 플레이어자신의 움직임가능상태전환 메소드를 등록
        mainGameManager = GameObject.FindObjectOfType<MainGameManager>().GetComponent<MainGameManager>();
        particleManager = mainGameManager.GetComponent<ParticleManager>();
        mainGameManager.gameStateChangeEvent.AddListener(MovePossibleState);

        string name = PhotonNetwork.NickName;
        int colorIndex = (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue" ? 0 : 1;
        photonView.RPC("PlayerSetup", RpcTarget.AllBuffered, name, colorIndex);

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
    private void PlayerSetup(string name, int color)
    {
        playerNameTagText.text = name;
        playerNameTagText.color = colors[color];
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

        oreMesh.material = mats[matNum];
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

    // 자식 오브젝트 "Ball"에서 사망처리 되었을 때 해당 메소드가 불리고 메소드에서 RPC호출한다음 RPC에서 ParticleManager호출
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
