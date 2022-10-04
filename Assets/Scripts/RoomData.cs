using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 방찾기탭의 왼쪽 UI Info 창에 등장하는 방 목록 오브젝트 프리팹의 컴포넌트로 활용되는 스크립트

public class RoomData : MonoBehaviour
{
    [Header("Room UI in myself")]
    [SerializeField]
    private Image modeImage;
    [SerializeField]
    private TextMeshProUGUI roomNameText;
    [SerializeField]
    private TextMeshProUGUI manCountText;

    private Sprite mapImage;
    private string modName;
    private Sprite modSprite2D;

    private RoomInfo roomInfo;
    [HideInInspector]
    public RoomInfo RoomInfo
    {
        get
        {
            return roomInfo;
        }
        set
        {
            roomInfo = value;

            // roomInfo가 변경되면 변경점을 UI에 업데이트함
            RoomUIUpdate();
        }
    }

    [HideInInspector]
    public GameObject clickOtherRoomPanel; // 이 게임오브젝트 프리팹이 생성될 때 LobbyNetwork 스크립트에서 설정
 
    // 홈에서 방찾기 버튼으로 로비에 입장 -> 방목록 UI 변경
    private void RoomUIUpdate()
    {
        roomNameText.text = roomInfo.Name;
        manCountText.text = roomInfo.PlayerCount + " / " + roomInfo.MaxPlayers;
    }

    // 방목록에서 방을 선택 -> 방 세부정보 UI 팝업
    public void ShowRoomInfo()
    {
        string tempString;
        TableMod tempTableMod;
        TableMap tempTableMap;

        // 모드 이미지, 이름 테이블에서 추출
        tempString = roomInfo.CustomProperties["ModName"].ToString();
        tempTableMod = Resources.Load("Table/Mod_" + tempString) as TableMod;

        modName = tempTableMod.modName;

        // 2D맵 이미지 테이블에서 추출
        tempString = roomInfo.CustomProperties["MapName"].ToString();
        tempTableMap = Resources.Load("Table/Map_" + tempString) as TableMap;

        mapImage = tempTableMap.mapSprite;


        clickOtherRoomPanel.SetActive(true);

        clickOtherRoomPanel.GetComponent<ClickOtherRoomPanel>().Setup(roomNameText.text, manCountText.text, mapImage, modName, roomInfo);
    }
}
