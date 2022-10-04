using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class EnabledUIObject : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

    [SerializeField]
    private Color selectColor;
    [SerializeField]
    private Color normalColor;

    private void OnEnable()
    {
        ChangeTextColor(true);
    }

    private void OnDisable()
    {
        ChangeTextColor(false);
    }

    private void ChangeTextColor(bool isSelected)
    {
        if(isSelected)
        {
            target.GetComponent<TextMeshProUGUI>().color = selectColor;
        }
        else
        {
            target.GetComponent<TextMeshProUGUI>().color = normalColor;
        }
        
    }
}
