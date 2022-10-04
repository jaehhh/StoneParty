using UnityEngine;
using TMPro;

public class NameChange : MonoBehaviour
{
    private TMP_InputField inputField;
    private string playerName;

    [SerializeField]
    private PopupUI popupUI;

    private void OnEnable()
    {
        if(inputField == null)
        {
            inputField = transform.GetComponentInChildren<TMP_InputField>();
        }
    }

    public void OnValueChanged()
    {
        playerName = inputField.text;
    }

    public void NameChangeButton()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "À¯Àú" + Random.Range(0, 100);
        }

        // UserData UserData.instance = Resources.Load("Table/UserData") as UserData;
        UserData.instance.PlayerName = playerName;

        FindObjectOfType<ProfileUI>().ProfileUpdate();

        FindObjectOfType<UserDataController>().Save();

        popupUI.ClosePopupUI();
    }
}
