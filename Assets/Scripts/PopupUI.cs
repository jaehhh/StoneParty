using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �κ� ȭ�鿡�� �����ϴ� �˾� UI
public class PopupUI : MonoBehaviour
{
    private LobbyUI lobbyUI;

    [SerializeField]
    private GameObject popupUI;

    private void Awake()
    {
        lobbyUI = GameObject.FindObjectOfType<LobbyUI>().GetComponent<LobbyUI>();
    }

    // ��ư���� ȣ��Ǵ� �˾�UI ���� �޼ҵ�
    public void OpenPopupUI()
    {
        popupUI.SetActive(true);
        lobbyUI.canSwipe = false;
        lobbyUI.mainScrollRect.enabled = false;
    }

    // ��ư���� ȣ��Ǵ� �˾�UI �ݱ� �޼ҵ�
    public void ClosePopupUI()
    {
        popupUI.SetActive(false);
        lobbyUI.canSwipe = true;
        lobbyUI.mainScrollRect.enabled = true;
    }
}
