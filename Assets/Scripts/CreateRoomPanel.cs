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
    // ���̾��Ű ������Ʈ
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
    private TableMod[] mods; // ���������� ���
    private TableMod selectedMod; // ���� ���õ� ���
    private TableMap[] maps; // ��忡 ���� ���������� ��
    private int mapOrder; // �� ����. �¿� ȭ��ǥ�� ������ ������� ���� ����
    private TableMap selectedMap; // ���� ���õ� ��
    private int selectedMaxPlayer; // ���� ���õ� �ִ��÷��̾��

    // �Է����� ���� �����ϴ� ����
    private string roomName;
    private string password;

    // UserData UserData.instance;

    private void Awake()
    {
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        SetupModDropDown();
    }

    // ��� ��Ӵٿ� ������ �Ҵ�
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

    // ���õ� ��忡 ���� ���ð����� �� �¾�
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

    // ���õ� �ʿ� ���� ���ð����� �ִ��÷��̾�� �¾�
    public void SetupMaxPlayerDropdown()
    {
        // �ִ��÷��̾�� ��Ӵٿ� ������ �Ҵ�
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

 
    // �� ����
    public void MapChange(int value)
    {
        // �Ű������� -1�̸� ���� ȭ��ǥ, 1�̸� ����ȭ��ǥ

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

    // ��� ��Ӵٿ� ���۽� �������� ��� ����
    public void ModDropdownOnValueChange(TMP_Dropdown dropdown)
    {
        string selectedModString = dropdown.options[dropdown.value].text;

        for (int i = 0; i < mods.Length; i++)
        {
            if (selectedModString == mods[i].modName)
            {
                selectedMod = mods[i];

                // ���ø�尡 �ٲ�� ���� �ʰ� �ִ��÷��̾�� �ٽ� �¾�
                SetupMap();

                break;
            }
        }
    }

    // �ִ��÷��̾�� ��Ӵٿ� ���۽� �������� �� ����
    public void MaxPlayerDropdownOnValueChange(TMP_Dropdown dropdown)
    {
        string selectedmaxPlayerString = dropdown.options[dropdown.value].text;

        selectedMaxPlayer = int.Parse(selectedmaxPlayerString);
    }
 
    // ���̸� ����
    public void RoomNameOnValueChanged()
    {
        roomName = roomNameInputField.text;
    }

    // ��� ����
    public void PasswordOnValueChanged()
    {
        password = passwordInputField.text;
    }

    // �游��� ��ư
    public void CreateRoom()
    {
        PhotonNetwork.NickName = UserData.instance.PlayerName;

        // �� �̸� ����
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

        // ��� �Է� ������ ���� �� ��� ����
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

        // �� ���� �κ񿡼� Ŀ����������Ƽ ���� �� �ֵ���
        roomOptions.CustomRoomPropertiesForLobby = new string[4] { "ModName", "MapName","isLocked","password" };

        // ���濡 �����ؼ� ù �������� �����ߴ��� Ȯ���� �� ���
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "isJoinRoom", false } });

        // ���� ������ �����͸� �����ϴ� GiveDataToRoom ��ũ��Ʈ�� ������ ����
        // giveDataToRoom.roomOption = roomOptions;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Room");
    }
}
