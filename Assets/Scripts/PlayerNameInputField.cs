using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(InputField))]

public class PlayerNameInputField : MonoBehaviour
{
    private const string playerNamePrefKey = "PlayerName";

    private void Start()
    {
        string defaultName = string.Empty;
        InputField inputField = this.GetComponent<InputField>();

        if(inputField != null)
        {
            if(PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    // InputField 에 타이핑할 때마다 인스펙터창에서 설정한 On Value Changed()에 의해 호출됨
    public void SetPlayerName(string value)
    {
        Debug.Log("value : " + value);

        if(string.IsNullOrEmpty(value))
        {
            Debug.Log("Player Name is null or empty.");
            return;
        }

        PhotonNetwork.NickName = value;

        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
