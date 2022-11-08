using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileUI : MonoBehaviour
{
    [Header("Drag Objects in Hierarchy for Refresh your profile which is leftup-side.")]
    [SerializeField]
    private Image profileImage;
    [SerializeField]
    private TextMeshProUGUI profileName;
    [SerializeField]
    private TextMeshProUGUI profileLevel;
    [SerializeField]
    private Slider profileEXP;

    private LevelTable levelTable;
    // UserData UserData.instance;

    private void Start()
    {
        levelTable = Resources.Load("Table/LevelTable") as LevelTable;
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        if (UserData.instance == null)
        {
            return; 
        }

        ProfileUpdate();
    }

    public void ProfileUpdate()
    {
        profileImage.sprite = levelTable.ProfileImages[UserData.instance.ProfileImageCode];
        profileName.text = UserData.instance.PlayerName;
        profileLevel.text = UserData.instance.Level.ToString();
        profileEXP.value = (float)UserData.instance.Exp / levelTable.MaxExp[UserData.instance.Level];
    }
}
