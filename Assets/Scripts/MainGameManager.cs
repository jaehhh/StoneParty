using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class GameStateChangeEvent : UnityEvent<bool> { }

public class MainGameManager : MonoBehaviourPunCallbacks
{
    // �÷��̾���� ������ ���� ���¸� �ٲ��ִ� �̺�Ʈ�Լ�
    [HideInInspector]
    public GameStateChangeEvent gameStateChangeEvent = new GameStateChangeEvent { };

    // Ÿ ������Ʈ
    [SerializeField]
    private FollowingCamera fCamera;

    // UI
    [SerializeField]
    private TextMeshProUGUI timeText;
    [SerializeField]
    private GameObject alertUI;
    [SerializeField]
    private TextMeshProUGUI alertMasage;

    // ���� ��
    private float maxTime = 180;
    float currentTime;

    // Ÿ ��ũ��Ʈ���� �޴� ��
    [HideInInspector]
    public Vector3 spawnPoint;
    [HideInInspector]
    public string winner;

    // �׽���
    [SerializeField]
    private bool testing = false;

    private void Awake()
    {
        Application.runInBackground = true;

        // �κ� ���Ͽ��� �� �ݱ� �� �Ⱥ��̱�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        gameStateChangeEvent.AddListener(StageState);
    }

    private void Start()
    {
        // ȭ�� ��ȯ

        Screen.orientation = ScreenOrientation.LandscapeLeft; // ���ʰ��θ��� ����

        Screen.orientation = ScreenOrientation.AutoRotation; // ȸ�� ���
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        CreatePlayer();

        // ���ΰ��Ӿ� ���� : true
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReadyInGame", true } });
        // ����� ���� üũ : false
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", false } });

        // ��� ������ �Ϸ��ߴ��� Ȯ��
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine("CheckReady");
        }
    }

    private void CreatePlayer()
    {
        GameObject clone = PhotonNetwork.Instantiate("PlayerInGame", spawnPoint, Quaternion.identity);

        fCamera.target = clone.transform.Find("Ball").gameObject;

        clone.GetComponent<PlayerInGameManager>().EquipItem();
    }

    public Vector3 RespawnPlayer()
    { 
        return spawnPoint;
    }

    // ������Ŭ���̾�Ʈ�� ��� �÷��̾��� ���� ���¸� Ȯ���ϴ� �޼ҵ�
    private IEnumerator CheckReady()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int readyCount;

        while (true)
        {
            readyCount = 0;

            Player[] players = PhotonNetwork.PlayerList;

            for (int i = 0; i < players.Length; i++)
            {
                if((bool)players[i].CustomProperties["isReadyInGame"])
                {
                    readyCount++;
                }
            }

            if (readyCount >= playerCount) break;

            yield return new WaitForSeconds(0.5f);
        }

        photonView.RPC("RPCStageStart", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPCStageStart()
    {
        StartCoroutine("StageStart");
    }

    private IEnumerator StageStart()
    {
        yield return new WaitForSeconds(2f);

        for(int i = 3; i >= 1; i--)
        {
            alertUI.SetActive(true);
            alertMasage.text = i+"�� �Ŀ� ������ ���۵˴ϴ�";

            yield return new WaitForSeconds(1f);
        }

        alertUI.SetActive(false);
        gameStateChangeEvent.Invoke(true);

        if (testing)
        {
            StartCoroutine("Timer");
        }
        else
        {
            photonView.RPC("RPCTimer", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPCTimer()
    {
        StartCoroutine("Timer");
    }

    private void StopTimer()
    {
        StopCoroutine("Timer");
    }

    private IEnumerator Timer()
    {
        currentTime = 0;

        while(true)
        {
            currentTime += Time.deltaTime;

            timeText.text = (maxTime - currentTime).ToString("N1");

            if(maxTime - currentTime <= 0)
            {
                break;
            }

            yield return null;
        }

        // ������Ŭ���̾�Ʈ���� ��¥ Ÿ�̸ӷν� �۵��ϰ� ��
        // ������ Ŭ���̾�Ʈ�� ��¥ Ÿ�̸�
        // Ÿ�̸� ������ ������Ŭ���̾�Ʈ�� Ÿ�̸Ӹ� �������ִ� ������� ����

        gameStateChangeEvent.Invoke(false);
    }

    // ���������� ����Ǿ�� �ϴ���
    private void StageState(bool value)
    {
        // ������ �����ٰ� �Ǵ�
        if(value == false)
        {
            PhotonNetwork.OpRemoveCompleteCache();

            StopTimer();

            alertUI.SetActive(true);
         
            if (winner == "")
            {
                alertMasage.text = "�����ϴ�!";
            }
            else
            {
                string team = "";

                if (winner == "blue")
                {
                    team = "�����";
                }
                else if (winner == "orange")
                {
                    team = "��������";
                }
                alertMasage.text = team + "�� �¸��Ͽ����ϴ�!";
            }

            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine("StageEnd", 5f);
            }

            GetExp(winner);
        }
    }

    private void GetExp(string winner)
    {
        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;

        // ���� �̱� ��
        if (winner == (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"])
        {
            int exp = Mathf.Min((int)(50 + (int)(currentTime / 10) * 10), 100);
            UserData.instance.Exp += exp;
        }
        // ���� �� ��
        else
        {
            int exp = Mathf.Min((int)(30 + (int)(currentTime / 10) * 6), 60);
            UserData.instance.Exp += exp;
        }

        LevelTable levelTable = Resources.Load("Table/LevelTable") as LevelTable;

        int currentExp = UserData.instance.Exp;
        int currentLevel = UserData.instance.Level;
        int maxExp = levelTable.MaxExp[currentLevel];

        if (currentExp >= maxExp)
        {
            UserData.instance.Level++;
            currentLevel++;

            currentExp -= maxExp;
            UserData.instance.Exp = currentExp;

            maxExp = levelTable.MaxExp[currentLevel];
        }

        FindObjectOfType<UserDataController>().Save();
        }

    private IEnumerator StageEnd(float time = 0)
    {
        yield return new WaitForSeconds(time);

        SceneChange();
    }

    private void SceneChange()
    {
        SceneManager.LoadScene("Room");
    }

    // �ٸ� �÷��̾� ����
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("�ٸ� �÷��̾� ����");

        gameStateChangeEvent.Invoke(false);
    }

    public void ExitRoom()
    {
        PhotonNetwork.OpRemoveCompleteCache();

        StopTimer();

        // ���÷� �ʱ�ȭ
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            int count = (int)PhotonNetwork.CurrentRoom.CustomProperties["blueTeamCount"];
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", count - 1 } });
        }
        else
        {
            int count = (int)PhotonNetwork.CurrentRoom.CustomProperties["orangeTeamCount"];
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "orangeTeamCount", count - 1 } });
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "teamColor", null } });

        // ���� ���� �ʱ�ȭ
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReady", false } });

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
