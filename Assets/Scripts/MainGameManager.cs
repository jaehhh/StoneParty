using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
    private float maxTime = 270; // ���� �ð�

    // â�� ����
    private string plusMasage;
    private float currentTime; // ����� �ð�
    private float focusOn, focusOff; // ��׶��� �ð�
    private bool isStarted = false;

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
        Application.runInBackground = true; // ������� ���� �ȵ�

        // �κ� ���Ͽ��� �� �ݱ� �� �Ⱥ��̱�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        gameStateChangeEvent.AddListener(StageState);

        timeText.text = ((int)maxTime / 60) + " : " + ((int)(maxTime % 60)).ToString("D2");
    }

    // ȭ�� ��ȯ
    private void ScreenRotate()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft; // ���ʰ��θ��� ����

        Screen.orientation = ScreenOrientation.AutoRotation; // ȸ�� ���
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    private void Start()
    {
        ScreenRotate();

        CreatePlayer();

        // ���ΰ��Ӿ� ���� : true
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReadyInGame", true } });
        // ����� ���� üũ : false
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", false } });

        // ��� ������ �Ϸ��ߴ��� Ȯ��
        if (PhotonNetwork.IsMasterClient)
        {
            CheckReady();
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
    private void CheckReady()
    {
        if (isStarted) return;

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int readyCount = 0;

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            if ((bool)players[i].CustomProperties["isReadyInGame"])
            {
                readyCount++;
            }
        }

        if (readyCount >= playerCount) photonView.RPC("RPCStageStart", RpcTarget.AllBuffered);

        else Invoke("CheckReady", 0.5f);
    }

    [PunRPC]
    private void RPCStageStart()
    {
        isStarted = true;

        // ȭ�� ��ȯ �ȵǴ� ���� ������ �� �� �� ȣ��
        ScreenRotate();

        StartCoroutine("StageStart");
    }

    private IEnumerator StageStart()
    {
        yield return new WaitForSeconds(1f);

        alertUI.SetActive(true);

        for (int i = 3; i >= 1; i--)
        {
            alertMasage.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }

        alertUI.SetActive(false);
        gameStateChangeEvent.Invoke(true);

        StartCoroutine("Timer");
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

            int remainTime = (int)(maxTime - currentTime);

            timeText.text = ((remainTime)/60) + " : " + (remainTime%60).ToString("D2");

            if(maxTime - currentTime <= 0)
            {
                currentTime = maxTime;

                break;
            }

            yield return null;
        }

        // ������Ŭ���̾�Ʈ���� ��¥ Ÿ�̸ӷν� �۵��ϰ� ��
        // ������ Ŭ���̾�Ʈ�� ��¥ Ÿ�̸�
        // Ÿ�̸� ������ ������Ŭ���̾�Ʈ�� Ÿ�̸Ӹ� �������ִ� ������� ����

        if(PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPCGameDone", RpcTarget.AllBufferedViaServer);
        }
        
    }

    [PunRPC]
    private void RPCGameDone()
    {
        gameStateChangeEvent.Invoke(false);
    }

    // ���������� ����Ǿ�� �ϴ���
    private void StageState(bool value)
    {
        // ������ �����ٰ� �Ǵ�
        if (value == false)
        {
            StopCoroutine("StageStart");

            StopTimer();

            StopCoroutine("CloseAlert");
            alertUI.SetActive(true);
         
            if (winner == "")
            {
                alertMasage.text = plusMasage + "�����ϴ�!";
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
                alertMasage.text = plusMasage + team + "�� �¸��Ͽ����ϴ�!";
            }

            if(PhotonNetwork.IsMasterClient)
            {
                StartCoroutine("StageEnd", 3.5f);
            }

            GetExp(winner);
        }

        PhotonNetwork.OpRemoveCompleteCache();
    }

    private void GetExp(string winner)
    {
        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;

        if (currentTime < 30f) return; // 30�� �̸� �÷��̽� ����ġ X

        // ���� �̱� ��
        if (winner == (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"])
        {
            int exp = Mathf.Min((int)(30 + (int)(currentTime / 10) * 5), 100);
            UserData.instance.Exp += exp;
        }
        // ���� �� ��
        else
        {
            int exp = Mathf.Min((int)(20 + (int)(currentTime / 10) * 3), 60);
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
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", 0 } });
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "orangeTeamCount", 0 } });

        SceneManager.LoadScene("Room");
    }

    // �ٸ� �÷��̾� ����
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("�ٸ� �÷��̾� ����");

        // �� ȥ�� ������ ���� ����
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            plusMasage = "��� �÷��̾ �������ϴ�. ";
            gameStateChangeEvent.Invoke(false);
        }
        else if (PhotonNetwork.IsMasterClient && isStarted == false) // ���ӽ������� ���� ������ �̾ ���� ���
        {
            CheckReady();
        }
        else
        {
            StopCoroutine("CloseAlert");
            alertUI.SetActive(true);
            alertMasage.text = $"������ {PhotonNetwork.MasterClient.NickName}������ ����Ǿ����ϴ�";

            StartCoroutine(CloseAlert(2f));
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            focusOff = Time.realtimeSinceStartup;
            // focusOff = DateTime.Now;
            // focusOff = SystemClock.elapsedRealtime(); java.lang.Object
            Debug.Log(focusOff);
        }
        else
        {
            focusOn = Time.realtimeSinceStartup;
            Debug.Log(focusOff);
            currentTime += focusOn - focusOff;
        }
    }

    private IEnumerator CloseAlert(float disappearTime)
    {
        yield return new WaitForSecondsRealtime(disappearTime);

        alertUI.SetActive(false);
    }

    public void Alert(bool active, string masage)
    {
        photonView.RPC("RPCAlert", RpcTarget.OthersBuffered, active, masage);
    }

    [PunRPC]
    private void RPCAlert(bool active, string masage)
    {
        alertUI.SetActive(active);
        if(masage != "")
        alertMasage.text = masage;
    }

    // �� ������
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
