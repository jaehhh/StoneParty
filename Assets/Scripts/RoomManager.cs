using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

// room 씬에서 작동되는 함수 모음
public class RoomManager : MonoBehaviourPunCallbacks
{
    // UI들
    [SerializeField]
    private TextMeshProUGUI roomNameText;
    [SerializeField]
    private TextMeshProUGUI playerCountText;
    [SerializeField]
    private TextMeshProUGUI chattingText;
    [SerializeField]
    private Image roomLockImage;

    // 하이어라키 드래그앤드랍 설정
    public Material[] mats; // 머테리얼
    [SerializeField]
    private Sprite[] lockSprite;

    [HideInInspector]
    public GameObject player; // UI버튼 눌렀을 때 플레이어의 상태에 영향을 주기 위한 


    public void Awake()
    {
        Application.runInBackground = true;

        // 로비 방목록에서 방 열기 및 보이기
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }

        // 커스텀프로퍼티 : 로컬플레이어 레디상태
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "isReady", false } });

        #region Set Room Info
        // 방제목
        string roomName = PhotonNetwork.CurrentRoom.Name;
        // 플레이어수 텍스트 변경
        string playerCount = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
        // 잠금 유무
        byte isLocked = (byte)PhotonNetwork.CurrentRoom.CustomProperties["isLocked"];

        RoomSetup(roomName, playerCount, isLocked);
        #endregion

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", 0 } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "orangeTeamCount", 0 } });
        }
    }

    private void Start()
    {
        // 화면 세로 전환
        Screen.orientation = ScreenOrientation.Portrait;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", true } });

        CreatePlayer();
    }

    private void CreatePlayer()
    {  
        int ran = Random.Range(0, 2);
        ran = ran == 0 ? -1 : 1;

        Vector3 pos = new Vector3(ran * 6f, 3.5f, 0);

        GameObject clone = PhotonNetwork.Instantiate("PlayerInRoom", Vector3.zero, Quaternion.identity);
        clone.transform.Find("Ball").transform.position = pos;

        clone.GetComponent<PlayerManager>().roomManager = this;

        clone.GetComponent<PlayerManager>().EquipItem();
    }

    [PunRPC]
    private void RoomSetup(string roomName = null, string playerCount = null, byte isLocked = 2)
    {
        if(roomName != null)
        roomNameText.text = roomName;
        if(playerCount != null)
        playerCountText.text = playerCount;
        if(isLocked != 2)
        {
            int i = isLocked == 1 ? 1 : 0;
            roomLockImage.sprite = lockSprite[i];
        } 
    }

    // 나가기 버튼 클릭시 호출
    public void ExitRoom()
    {
        // 팀컬러 초기화
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue")
        {
            int count = (int)PhotonNetwork.CurrentRoom.CustomProperties["blueTeamCount"];
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "blueTeamCount", count-1 } });
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

    // 방 나가기 완료시 로비 씬으로 전환
    public override void OnLeftRoom()
    {
        //roomData.DestroyRoomData();

        SceneManager.LoadScene("Lobby");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogWarning("OnPlayerEnteredRoom()");

        // 현재 방정보 세팅은 각자가 알아서 함
        /*
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine("WaitOtherPlayerJoin", newPlayer); 
        }*/

        // 방제목
        string roomName = PhotonNetwork.CurrentRoom.Name;
        // 플레이어수 텍스트 변경
        string playerCount = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

        RoomSetup(roomName, playerCount);
    }

    private IEnumerator WaitOtherPlayerJoin(Player newPlayer)
    {
        Debug.LogWarning("WaitOtherPlayerJoin Coroutine");

        bool isJoinedRoom = false;

        while (isJoinedRoom != true)
        {
            isJoinedRoom = (bool)newPlayer.CustomProperties["isJoinRoom"];

            yield return null;
        }

        // 방제목
        string roomName = PhotonNetwork.CurrentRoom.Name;
        // 플레이어수 텍스트 변경
        string playerCount = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

        photonView.RPC("RoomSetup", RpcTarget.All, roomName, playerCount);
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning($"1 Frame : {Time.deltaTime}");

        Invoke("CheckMasterClient", Time.deltaTime * 2f);
    }
    
    // 방장 바뀌면 레디 해제 및 방정보 업데이트
    private void CheckMasterClient()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning($"다른 플레이어 나갔고, 나는 방장임");

            // 레디해제
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReady", false } });
            player.GetComponent<PlayerManager>().Ready(false);

            // 방정보 업데이트
            string playerCount = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
            photonView.RPC("RoomSetup", RpcTarget.All, null, playerCount, (byte)2);
        }
        else
        {
            Debug.Log("방장교체 실패");
        }
    }

    // 준비 게임시작 버튼
    public void ReadyStartButton()
    {
        // 방장이면 레디인원 체크 후 게임시작
        if(PhotonNetwork.IsMasterClient)
        {
            int readyCount = 0;

            for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if((bool)PhotonNetwork.PlayerList[i].CustomProperties["isReady"] &&
                    PhotonNetwork.PlayerList[i].NickName != PhotonNetwork.NickName)
                {
                    readyCount++;
                }
            }

            if(readyCount >= PhotonNetwork.CurrentRoom.PlayerCount - 1)
            {
                if((int)PhotonNetwork.CurrentRoom.CustomProperties["blueTeamCount"] == (int)PhotonNetwork.CurrentRoom.CustomProperties["orangeTeamCount"])
                {
                    string sceneName = PhotonNetwork.CurrentRoom.CustomProperties["ModName"].ToString() +
                        PhotonNetwork.CurrentRoom.CustomProperties["MapName"].ToString();

                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Debug.LogWarning("인원수가 맞지 않음");

                    string sceneName = PhotonNetwork.CurrentRoom.CustomProperties["ModName"].ToString() +
                        PhotonNetwork.CurrentRoom.CustomProperties["MapName"].ToString();

                    SceneManager.LoadScene(sceneName);  // 테스트
                }     
            }
            else
            {
                Debug.LogWarning("준비하지 않은 인원 존재 : "+ readyCount +" / " + (PhotonNetwork.CurrentRoom.PlayerCount - 1));
            }
        }
        // 방장 아니면 레디/레디헤제
        else
        {
            if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"])
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { {"isReady", false} });
                player.GetComponent<PlayerManager>().Ready(false);
            }
            else
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isReady", true } });
                player.GetComponent<PlayerManager>().Ready(true);
            }
        }
    }
    
    // 팀변경 버튼
    public void ChangeTeamButton()
    {
        player.GetComponent<PlayerManager>().ChangeTeam();
    }
}
