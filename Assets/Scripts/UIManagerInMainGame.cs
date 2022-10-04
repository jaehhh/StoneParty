using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


// 포톤네트워크로 뒷늦게 생성된 각 로컬 플레이어에 있는 MoveController 스크립트와 화면UI버튼을 연결하도록 이벤트함수를 사용 
public class MoveButtonEvent : UnityEvent<int> { }
public class JumpButtonDownEvent : UnityEvent<bool>  { }
public class JumpButtonUpEvent : UnityEvent { }
public class DashButtonEvent : UnityEvent { }



public class UIManagerInMainGame : MonoBehaviour
{
    [HideInInspector]
    public MoveButtonEvent moveButtonEvent = new MoveButtonEvent();
    [HideInInspector]
    public JumpButtonDownEvent jumpButtonDownEvent = new JumpButtonDownEvent();
    [HideInInspector]
    public JumpButtonUpEvent jumpButtonUpEvent = new JumpButtonUpEvent();
    [HideInInspector]
    public DashButtonEvent dashButtonEvent = new DashButtonEvent();

    [SerializeField]
    private Image exitPopupUI;

    private GameObject currentPopupUI; // 현재 팝업된 UI

    public void MoveButton(int direction = 0)
    {
        moveButtonEvent.Invoke(direction);
    }

    public void JumpButtonDown(bool buttonDown)
    {
        jumpButtonDownEvent.Invoke(buttonDown);
    }

    public void JumpbuttonUp()
    {
        jumpButtonUpEvent.Invoke();
    }

    public void DashButton()
    {
        dashButtonEvent.Invoke();
    }

    public void ExitButton()
    {
        exitPopupUI.gameObject.SetActive(true);

        currentPopupUI = exitPopupUI.gameObject;
    }

    public void CloseTap()
    {
        currentPopupUI.SetActive(false);
    }

    public void AcceptExit()
    {
        GetComponent<MainGameManager>().ExitRoom();
    }
}
