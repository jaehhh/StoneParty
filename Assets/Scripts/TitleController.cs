using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private GameObject newPlayerPanel;
    [SerializeField]
    private GameObject againPlayerPanel;

    private UserDataController dataController;

    private string playerName;

    private void Start()
    {
        dataController = GetComponent<UserDataController>();

           // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;

        if (UserData.instance.PlayerName == "new player" || string.IsNullOrEmpty(UserData.instance.PlayerName))
        {
            newPlayerPanel.SetActive(true);
            againPlayerPanel.SetActive(false);
        }
        else
        {
            againPlayerPanel.SetActive(true);
            newPlayerPanel.SetActive(false);
        }
    }

    public void OnValueChanged()
    {
        playerName = inputField.text;
    }

    public void StartButtonWithoutID()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "유저" + Random.Range(0, 100);
        }

        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;

        dataController.ResetProfile();

        UserData.instance.PlayerName = playerName;
        dataController.Save();

        SceneManager.LoadScene("Lobby");
    }

    public void StartButtonWithID()
    {
        SceneManager.LoadScene("Lobby");
    }
}
