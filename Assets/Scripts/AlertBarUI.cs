using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class AlertEndEvent : UnityEvent{ };

public class AlertBarUI : MonoBehaviour
{
    public AlertEndEvent alertEndEvent;

    public static AlertBarUI instance = null; // ��������ȭ

    public delegate void DelegateFunction();
    public DelegateFunction delegateHandler;

    [SerializeField]
    private TextMeshProUGUI alertText;

    private IEnumerator coroutine; // �������� �ڷ�ƾ

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public void AlertWithText(string str = "�� �� ���� �����Դϴ�", float time = 3f)
    {
        if(coroutine != null)
        StopCoroutine(coroutine);

        alertText.text = str;

        coroutine = AlertCoroutine(time);
        StartCoroutine(coroutine);
    }

    private IEnumerator AlertCoroutine(float time)
    {
        alertText.transform.parent.gameObject.SetActive(true);

        yield return new WaitForSeconds(time);

        alertText.transform.parent.gameObject.SetActive(false);

        try
        {
            alertEndEvent.Invoke();
            alertEndEvent.RemoveAllListeners();
        }
        catch
        {

        } 
    }
}
