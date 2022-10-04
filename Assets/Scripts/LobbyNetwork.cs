using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyNetwork : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string gameVersion = "1";

    // UI 관련
    [SerializeField]
    private GameObject roomPrefab; // 방정보창(방목록에 뜨는) 
    [SerializeField]
    private Transform parent; // 방목록 창
    [SerializeField]
    private Button findRoomButton; // Home탭의 방찾기 버튼

    // 방목록 데이터
    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();

    [SerializeField]
    private GameObject roomFindTap_PanelClickOtherRoom; // 프리팹 스크립트에서 하이라키 오브젝트를 참조할 수 있도록 데이터를 전달

    private void Awake()
    {
        // 마스터클라이언트가 씬전환을 호출하고 연결된 모든 플레이어들이 자동적으로 씬전환을 할 수 있도록 하는 함수
        PhotonNetwork.AutomaticallySyncScene = true;

        Connect();
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings(); // 포톤 클라우드에 연결
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogWarning("OnConnectedToMaster");

        findRoomButton.interactable = true;  
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"OnDisconnected. Cause : {cause}");

        Connect();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room List 업데이트");

        GameObject tempRoom = null;

        foreach(var room in roomList)
        {
            if (room.RemovedFromList == true) // 룸 삭제
            {
                roomDict.TryGetValue(room.Name, out tempRoom); // out은 단순히 값만 전이되는 게 아니고 주소값을 넣는 것
                Destroy(tempRoom);
                roomDict.Remove(room.Name);
            }
            else
            {
                if (roomDict.ContainsKey(room.Name) == false) // 룸 생성
                {
                    GameObject roomUI = Instantiate(roomPrefab, parent);
                    roomUI.GetComponent<RoomData>().RoomInfo = room;
                    roomDict.Add(room.Name, roomUI);

                    roomUI.GetComponent<RoomData>().clickOtherRoomPanel = roomFindTap_PanelClickOtherRoom;
                }
                else // 룸 정보 갱신
                {
                    roomDict.TryGetValue(room.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = room;
                }
            }
        }
    }

    // 내정보창에서 방찾기 버튼 누르면 호출
    public void FindRoom()
    {
        Debug.LogWarning("로비 입장 버튼");

        Application.runInBackground = true;

        PhotonNetwork.JoinLobby();
    }

    public void LeaveFindRoom()
    {
        Debug.LogWarning("로비 떠남 버튼");

        Application.runInBackground = false;

        PhotonNetwork.LeaveLobby();
    }


    /*
    public void UIChange(string value)
    {
        if (value == "myInfo")
        {
            myInfoMenu.SetActive(true);
            lobbyManu.SetActive(false);
        }
        else if (value == "lobby")
        {
            myInfoMenu.SetActive(false);
            lobbyManu.SetActive(true);
        }
    }*/
}
