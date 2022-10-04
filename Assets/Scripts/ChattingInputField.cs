using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class ChattingInputField : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI chattingText;

    public void OnEndEdit()
    {
        string newText = GetComponent<TMP_InputField>().text;

        if(newText != "")
        {
            chattingText.text += "\n" + newText;
        }  

        GetComponent<TMP_InputField>().text = null;
    }

    public void OnDeselect()
    {
        return;
    }
}
