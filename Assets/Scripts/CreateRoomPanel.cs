using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CreateRoomPanel : MonoBehaviourPunCallbacks
{
    // 하이어라키 오브젝트
    [SerializeField]
    private TMP_InputField roomNameInputField;
    [SerializeField]
    private Image mapImage;
    [SerializeField]
    private TextMeshProUGUI mapNameText;
    [SerializeField]
    private TMP_Dropdown modDropdown;
    [SerializeField]
    private TMP_Dropdown maxPlayerDropdown;
    [SerializeField]
    private TMP_InputField passwordInputField;

    [SerializeField]
    private TableMod[] mods; // 설정가능한 모드
    private TableMod selectedMod; // 현재 선택된 모드
    private TableMap[] maps; // 모드에 따라 설정가능한 맵
    private int mapOrder; // 맵 순서. 좌우 화살표를 조작해 순서대로 맵을 선택
    private TableMap selectedMap; // 현재 선택된 맵
    private int selectedMaxPlayer; // 현재 선택된 최대플레이어수

    // 입력중인 값을 저장하는 공간
    private string roomName;
    private string password;

    // UserData UserData.instance;

    private void Awake()
    {
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        SetupModDropDown();
    }

    // 모드 드롭다운 데이터 할당
    private void SetupModDropDown()
    {
        modDropdown.options.Clear();

        for (int i = 0; i < mods.Length; i++)
        {
            TMP_Dropdown.OptionData dropdownData = new TMP_Dropdown.OptionData();
            dropdownData.text = mods[i].modName;

            modDropdown.options.Add(dropdownData);
        }

        selectedMod = mods[modDropdown.value];

        SetupMap();
    }

    // 선택된 모드에 따라 선택가능한 맵 셋업
    public void SetupMap()
    {
        maps = new TableMap[selectedMod.canUseMapsInTeamMod.Length];

        for(int i = 0; i < maps.Length; i++)
        {
            maps[i] = selectedMod.canUseMapsInTeamMod[i];
        }

        selectedMap = maps[0];
        mapOrder = 0;

        mapImage.sprite = selectedMap.mapSprite;
        mapNameText.text = selectedMap.mapName;

        SetupMaxPlayerDropdown();
    }

    // 선택된 맵에 따라 선택가능한 최대플레이어수 셋업
    public void SetupMaxPlayerDropdown()
    {
        // 최대플레이어수 드롭다운 데이터 할당
        //
        maxPlayerDropdown.options.Clear();

        for (int i = 0; i < selectedMap.maxPlayerInTeamMod.Length; i++)
        {
            TMP_Dropdown.OptionData dropdownData = new TMP_Dropdown.OptionData();
            dropdownData.text = selectedMap.maxPlayerInTeamMod[i].ToString();

            maxPlayerDropdown.options.Add(dropdownData);
        }
        maxPlayerDropdown.value = 0;

        selectedMaxPlayer = int.Parse(maxPlayerDropdown.options[maxPlayerDropdown.value].text);

        maxPlayerDropdown.captionText.text = selectedMaxPlayer.ToString();
    }

 
    // 맵 변경
    public void MapChange(int value)
    {
        // 매개변수가 -1이면 좌측 화살표, 1이면 우측화살표

        mapOrder += value;

        if(mapOrder >= maps.Length)
        {
            mapOrder = 0;
        }
        else if(mapOrder < 0)
        {
            mapOrder = maps.Length - 1;
        }

        selectedMap = maps[mapOrder];

        Debug.Log("maps.length : " + maps.Length);
        Debug.Log("mapOrder :" + mapOrder);

        mapImage.sprite = selectedMap.mapSprite;
        mapNameText.text = selectedMap.mapName;
    }

    // 모드 드롭다운 조작시 선택중인 모드 변경
    public void ModDropdownOnValueChange(TMP_Dropdown dropdown)
    {
        string selectedModString = dropdown.options[dropdown.value].text;

        for (int i = 0; i < mods.Length; i++)
        {
            if (selectedModString == mods[i].modName)
            {
                selectedMod = mods[i];

                // 선택모드가 바뀌면 이후 맵과 최대플레이어수 다시 셋업
                SetupMap();

                break;
            }
        }
    }

    // 최대플레이어수 드롭다운 조작시 선택중인 값 변경
    public void MaxPlayerDropdownOnValueChange(TMP_Dropdown dropdown)
    {
        string selectedmaxPlayerString = dropdown.options[dropdown.value].text;

        selectedMaxPlayer = int.Parse(selectedmaxPlayerString);
    }
 
    // 방이름 변경
    public void RoomNameOnValueChanged()
    {
        roomName = roomNameInputField.text;
    }

    // 비번 설정
    public void PasswordOnValueChanged()
    {
        password = passwordInputField.text;
    }

    // 방만들기 버튼
    public void CreateRoom()
    {
        PhotonNetwork.NickName = UserData.instance.PlayerName;

        // 방 이름 세팅
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room{Random.Range(0, 100)}";
        }

        RoomOptions roomOptions = new RoomOptions();
        Hashtable roomCustomProperties = new Hashtable();

        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)selectedMaxPlayer;
        roomOptions.CleanupCacheOnLeave = true;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable {
            { "RoomName", roomName },{"blueTeamCount",0 },{"orangeTeamCount",0 },
            {"ModName", selectedMod.modName }, {"MapName",selectedMap.mapName}, {"BGMIndex" , -1} };

        // 비번 입력 유무에 따라 방 잠금 설정
        if (string.IsNullOrEmpty(password))
        {
            roomOptions.CustomRoomProperties.Add("isLocked", (byte)0);
            roomOptions.CustomRoomProperties.Add("password", null);
        }
        else
        {
            roomOptions.CustomRoomProperties.Add("isLocked", (byte)1);
            roomOptions.CustomRoomProperties.Add("password", password);
        }

        // 룸 말고 로비에서 커스텀프로퍼티 읽을 수 있도록
        roomOptions.CustomRoomPropertiesForLobby = new string[4] { "ModName", "MapName","isLocked","password" };

        // 대기방에 입장해서 첫 프레임을 시작했는지 확인할 때 사용
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", false } });

        // 다음 씬으로 데이터를 전달하는 GiveDataToRoom 스크립트에 데이터 전달
        // giveDataToRoom.roomOption = roomOptions;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Room");
    }
}
