using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubTap : MonoBehaviour
{
    [SerializeField]
    private GameObject targetTap;

    public void CloseSubTap()
    {
        targetTap.SetActive(false);
    }

    public void OpenSubTap()
    {
        targetTap.SetActive(true);
    }
}
