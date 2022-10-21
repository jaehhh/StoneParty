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
    // 플레이어들의 움직임 가능 상태를 바꿔주는 이벤트함수
    [HideInInspector]
    public GameStateChangeEvent gameStateChangeEvent = new GameStateChangeEvent { };

    // 타 오브젝트
    [SerializeField]
    private FollowingCamera fCamera;

    // UI
    [SerializeField]
    private TextMeshProUGUI timeText;
    [SerializeField]
    private GameObject alertUI;
    [SerializeField]
    private TextMeshProUGUI alertMasage;

    // 설정 값
    private float maxTime = 180;
    float currentTime;

    // 타 스크립트에서 받는 값
    [HideInInspector]
    public Vector3 spawnPoint;
    [HideInInspector]
    public string winner;

    // 테스팅
    [SerializeField]
    private bool testing = false;

    private void Awake()
    {
        Application.runInBackground = true;

        // 로비 방목록에서 방 닫기 및 안보이기
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        gameStateChangeEvent.AddListener(StageState);
    }

    private void Start()
    {
        // 화면 전환

        Screen.orientation = ScreenOrientation.LandscapeLeft; // 왼쪽가로모드로 변경

        Screen.orientation = ScreenOrientation.AutoRotation; // 회전 허용
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        CreatePlayer();

        // 메인게임씬 입장 : true
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReadyInGame", true } });
        // 대기방씬 입장 체크 : false
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", false } });

        // 모두 입장을 완료했는지 확인
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

    // 마스터클라이언트가 모든 플레이어의 입장 상태를 확인하는 메소드
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
            alertMasage.text = i+"초 후에 게임이 시작됩니다";

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

        // 마스터클라이언트만이 진짜 타이머로써 작동하게 함
        // 나머지 클라이언트는 가짜 타이머
        // 타이머 끝나면 마스터클라이언트의 타이머를 전달해주는 방법으로 구현

        gameStateChangeEvent.Invoke(false);
    }

    // 스테이지가 진행되어야 하는지
    private void StageState(bool value)
    {
        // 게임이 끝났다고 판단
        if(value == false)
        {
            PhotonNetwork.OpRemoveCompleteCache();

            StopTimer();

            alertUI.SetActive(true);
         
            if (winner == "")
            {
                alertMasage.text = "비겼습니다!";
            }
            else
            {
                string team = "";

                if (winner == "blue")
                {
                    team = "블루팀";
                }
                else if (winner == "orange")
                {
                    team = "오렌지팀";
                }
                alertMasage.text = team + "이 승리하였습니다!";
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

        // 내가 이긴 팀
        if (winner == (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"])
        {
            int exp = Mathf.Min((int)(50 + (int)(currentTime / 10) * 10), 100);
            UserData.instance.Exp += exp;
        }
        // 내가 진 팀
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

    // 다른 플레이어 떠남
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("다른 플레이어 떠남");

        gameStateChangeEvent.Invoke(false);
    }

    public void ExitRoom()
    {
        PhotonNetwork.OpRemoveCompleteCache();

        StopTimer();

        // 팀컬러 초기화
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

        // 레디 상태 초기화
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReady", false } });

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
