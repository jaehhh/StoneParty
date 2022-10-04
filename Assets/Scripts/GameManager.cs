using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

// 안쓰는 스크립트
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public GameObject playerPrefab;

    private void Start()
    {
        Instance = this;

        if(playerPrefab == null)
        {
            Debug.Log("playerPrefab null");
        }
        else
        {
            if(PlayerController.LocalPlayerInstance == null)
            {
                PhotonNetwork.Instantiate(this.playerPrefab.name, Vector3.zero, Quaternion.identity);
            }
            else
            {
                Debug.Log("LocalPlayerInstance == null");
            } 
        }
    }

    // 룸에 입장했을 시 호출되는 Pun 콜백함수
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("OnPlayerEnteredRoom()");
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("OnPlayerEnteredRoom()/PhotonNetwork.isMasterClient");
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);

            LoadArena();
        }
    }

    // 룸에 입장 후 호출되는 씬 변경 함수
    private void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("LoadArena()/!PhotonNetwork.isMasterClient");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    // 룸을 퇴장할 때 호출되는 Pun콜백함수
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Main");
    }

    // 화면에서 "방 나가기" 버튼 누르면 호출
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // 다른 플레이어가 방을 나가면 호출되는 Pun 콜백함수
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName);

        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("OnPlayerLeftRoom()/PhotonNetwork.isMasterClient");
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);

            LoadArena(); // 플레이어 수에 따라 씬을 재변경하기 위함
        }
    }
}
