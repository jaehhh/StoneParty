using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MyInfoUI : MonoBehaviour
{
    [SerializeField]
    private Image userImage;
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI levelExpText;

    private void OnEnable()
    {
        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;
        LevelTable levelTable = Resources.Load("Table/LevelTable") as LevelTable;

        userImage.sprite = levelTable.ProfileImages[UserData.instance.ProfileImageCode];
        playerNameText.text = UserData.instance.PlayerName;
        levelExpText.text = "Level " + UserData.instance.Level + "\nExp " + UserData.instance.Exp + " / " + levelTable.MaxExp[UserData.instance.Level];
    }
}
