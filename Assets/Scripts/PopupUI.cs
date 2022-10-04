using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 로비 화면에서 등장하는 팝업 UI
public class PopupUI : MonoBehaviour
{
    private LobbyUI lobbyUI;

    [SerializeField]
    private GameObject popupUI;

    private void Awake()
    {
        lobbyUI = GameObject.FindObjectOfType<LobbyUI>().GetComponent<LobbyUI>();
    }

    // 버튼으로 호출되는 팝업UI 열기 메소드
    public void OpenPopupUI()
    {
        popupUI.SetActive(true);
        lobbyUI.canSwipe = false;
        lobbyUI.mainScrollRect.enabled = false;
    }

    // 버튼으로 호출되는 팝업UI 닫기 메소드
    public void ClosePopupUI()
    {
        popupUI.SetActive(false);
        lobbyUI.canSwipe = true;
        lobbyUI.mainScrollRect.enabled = true;
    }
}
