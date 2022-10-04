using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ClickOtherRoomPanel : MonoBehaviourPunCallbacks
{
    // my child GameObjects
    [SerializeField]
    private TextMeshProUGUI roomNameText;
    [SerializeField]
    private TextMeshProUGUI manCountText;
    [SerializeField]
    private Image mapImage;
    [SerializeField]
    private Image gameModeImage;
    [SerializeField]
    private TextMeshProUGUI gameModeText;
    [SerializeField]
    private TMP_InputField passwordInputField;
    [SerializeField]
    private Button enterRoomButton;

    private bool isLocked; // 현재 팝업된 방이 잠겨있는가
    private string roomPassword; // 방의 패스워드
    private string textingPassword; // 입력중인 패스워드

    public void Setup(string roomNameText, string manCountText, Sprite sprite = null , string gameModText= "", RoomInfo roomInfo = null)
    {
        this.roomNameText.text = roomNameText;
        this.manCountText.text = manCountText;
        this.mapImage.sprite = sprite;
        this.gameModeText.text = gameModText;

        enterRoomButton.onClick.RemoveAllListeners();
        enterRoomButton.onClick.AddListener(() => OnEnterRoom(roomNameText));

        byte temp = (byte)roomInfo.CustomProperties["isLocked"];
        isLocked = temp == (byte)1 ? true : false;

        if(isLocked)
        {
            roomPassword = (string)roomInfo.CustomProperties["password"];

            passwordInputField.gameObject.SetActive(true);
            passwordInputField.text = "";
            textingPassword = "";
        }
        else
        {
            passwordInputField.gameObject.SetActive(false);
        }
    }

    public void PasswordInputFieldOnValueChange()
    {
        textingPassword = passwordInputField.text;

        Debug.Log("textingPassword : " + textingPassword + "\nroomPassword : " + roomPassword);
    }

    public void CloseTap()
    {
        gameObject.SetActive(false);
    }

    // 버튼 클릭하면 들어가지도록 하는 이벤트
    private void OnEnterRoom(string roomName)
    {
        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;

        if (isLocked)
        {
            // 비밀번호 일치하면 방에 입장
            if(textingPassword == roomPassword)
            {
                PhotonNetwork.NickName = UserData.instance.PlayerName;
                PhotonNetwork.JoinRoom(roomNameText.text, null);
            }
            else
            {
                AlertBarUI.instance.AlertWithText("비밀번호가 틀렸습니다");
            }
        }
        else
        {
            PhotonNetwork.NickName = UserData.instance.PlayerName;
            PhotonNetwork.JoinRoom(roomNameText.text, null);
        }   
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("Room");
    }
}
