using UnityEngine;
using TMPro;

public class NameShowInTitle : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<TextMeshProUGUI>().text = UserData.instance.PlayerName;
    }
}
