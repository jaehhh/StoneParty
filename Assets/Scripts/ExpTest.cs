using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpTest : MonoBehaviour
{
    int currentExp;
    int currentLevel;
    int maxExp;
    private LevelTable levelTable;
     // UserData UserData.instance;
    private UserDataController dataController;

    private ProfileUI profileUI;

    private void Start()
    {
        profileUI = FindObjectOfType<ProfileUI>();

        levelTable = Resources.Load("Table/LevelTable") as LevelTable;
        //UserData.instance = Resources.Load("Table/UserData") as UserData;
        dataController = FindObjectOfType<UserDataController>().GetComponent<UserDataController>();

        currentExp = UserData.instance.Exp;
        currentLevel = UserData.instance.Level;
        maxExp = levelTable.MaxExp[currentLevel];
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            currentExp += 50;

            UserData.instance.Exp = currentExp;

            if (currentExp >= maxExp)
            {
                UserData.instance.Level++;
                currentLevel++;

                currentExp -= maxExp;
                UserData.instance.Exp = currentExp;

                maxExp = levelTable.MaxExp[currentLevel];  
            }


            profileUI.ProfileUpdate();
            dataController.Save();
        }
    }
}
