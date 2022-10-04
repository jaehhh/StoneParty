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

    private bool isLocked; // ���� �˾��� ���� ����ִ°�
    private string roomPassword; // ���� �н�����
    private string textingPassword; // �Է����� �н�����

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

    // ��ư Ŭ���ϸ� �������� �ϴ� �̺�Ʈ
    private void OnEnterRoom(string roomName)
    {
        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;

        if (isLocked)
        {
            // ��й�ȣ ��ġ�ϸ� �濡 ����
            if(textingPassword == roomPassword)
            {
                PhotonNetwork.NickName = UserData.instance.PlayerName;
                PhotonNetwork.JoinRoom(roomNameText.text, null);
            }
            else
            {
                AlertBarUI.instance.AlertWithText("��й�ȣ�� Ʋ�Ƚ��ϴ�");
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
