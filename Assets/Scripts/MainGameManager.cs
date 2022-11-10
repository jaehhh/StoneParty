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
    private float maxTime = 270; // 게임 시간

    // 창고 변수
    private string plusMasage;
    private float currentTime; // 진행된 시간
    private float focusOn, focusOff; // 백그라운드 시간
    private bool isStarted = false;

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
        Application.runInBackground = true; // 모버일은 적용 안됨

        // 로비 방목록에서 방 닫기 및 안보이기
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        gameStateChangeEvent.AddListener(StageState);

        timeText.text = ((int)maxTime / 60) + " : " + ((int)(maxTime % 60)).ToString("D2");
    }

    // 화면 전환
    private void ScreenRotate()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft; // 왼쪽가로모드로 변경

        Screen.orientation = ScreenOrientation.AutoRotation; // 회전 허용
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    private void Start()
    {
        ScreenRotate();

        CreatePlayer();

        // 메인게임씬 입장 : true
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReadyInGame", true } });
        // 대기방씬 입장 체크 : false
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", false } });

        // 모두 입장을 완료했는지 확인
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

    // 마스터클라이언트가 모든 플레이어의 입장 상태를 확인하는 메소드
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

        // 화면 전환 안되는 버그 때문에 한 번 더 호출
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

        // 마스터클라이언트만이 진짜 타이머로써 작동하게 함
        // 나머지 클라이언트는 가짜 타이머
        // 타이머 끝나면 마스터클라이언트의 타이머를 전달해주는 방법으로 구현

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

    // 스테이지가 진행되어야 하는지
    private void StageState(bool value)
    {
        // 게임이 끝났다고 판단
        if (value == false)
        {
            StopCoroutine("StageStart");

            StopTimer();

            StopCoroutine("CloseAlert");
            alertUI.SetActive(true);
         
            if (winner == "")
            {
                alertMasage.text = plusMasage + "비겼습니다!";
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
                alertMasage.text = plusMasage + team + "이 승리하였습니다!";
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

        if (currentTime < 30f) return; // 30초 미만 플레이시 경험치 X

        // 내가 이긴 팀
        if (winner == (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"])
        {
            int exp = Mathf.Min((int)(30 + (int)(currentTime / 10) * 5), 100);
            UserData.instance.Exp += exp;
        }
        // 내가 진 팀
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

    // 다른 플레이어 떠남
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("다른 플레이어 떠남");

        // 나 혼자 남으면 게임 종료
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            plusMasage = "모든 플레이어가 나갔습니다. ";
            gameStateChangeEvent.Invoke(false);
        }
        else if (PhotonNetwork.IsMasterClient && isStarted == false) // 게임시작전에 방장 나가면 이어서 시작 대기
        {
            CheckReady();
        }
        else
        {
            StopCoroutine("CloseAlert");
            alertUI.SetActive(true);
            alertMasage.text = $"방장이 {PhotonNetwork.MasterClient.NickName}님으로 변경되었습니다";

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

    // 방 나가기
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
