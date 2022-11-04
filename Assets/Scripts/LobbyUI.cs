using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Other Objects for Get Component")]
    [SerializeField]
    private LobbyNetwork lobbyNetwork;

    // UI버튼 클릭시 활성화될 화면
    // 로비 화면의 UI버튼이 추가되거나 제거될 때 수정 필요
    // Awake()에서 리스트 수정해주어야 함
    [Header("Opened&Closed Main UI. Not Popup UI")]
    [SerializeField]
    private GameObject homeUIObject;
    [SerializeField]
    private GameObject roomFindUIObject;

    private GameObject currentMainUIObject;
    private GameObject currentPopupUIObject;

    private List<GameObject> UIObjectList = new List<GameObject>();

    // 화면 하단에 있는 버튼
    [SerializeField]
    private Button[] bottomButtons;
    private int currentButtonIndex;


    // ==================================== SWIPE ====================================================

    [SerializeField]
    private Scrollbar scrollBar; // 스크롤바의 위치를 바탕으로 현재 페이지 검사
    [SerializeField]
    private float swipeTime = 0.2f; // 페이지가 스와이프 되는 시간
    [SerializeField]
    private float swipeDistance = 250f; // 페이지가 스와잎되기 위해 움직여야 하는 최소 거리

    private float[] arrayScrollValue; // 각 페이지의 위치 값 [0.0~1.0]
    private float valueDistance = 0; // 각 페이지 사이의 거리
    private int currentIndex = 0; // 현재페이지
    private int maxIndex = 0; // 최대 페이지
    private int centerIndex; // 메인UI 중앙페이지값
    private float startTouchX; // 터치 시작 위치
    private float endTouchX; // 터치 종료 위치
    private bool isSwipeMode = false; // 현재 스와이프 되고 있는지 체크
    [HideInInspector]
    public bool canSwipe = true; // 팝업창으로 인해 스와이프가 불가능한지
    [HideInInspector]
    public ScrollRect mainScrollRect; // 팝업창이 뜨면 메인UI가 좌우로 스와이프 되지 않도록 on off 하기 위함

    [SerializeField]
    private GameObject swipeUI; // 스와이프되며 보여질 페이지들의 부모 오브젝트 (ScrollView_Viewport_Content)

    private void Awake()
    {
        scrollBar.value = 0.5f; // 빠른 중앙 조정
        currentButtonIndex = 1;
        bottomButtons[currentButtonIndex].GetComponent<SpriteChanger>().Selected();

        UIObjectList.Add(homeUIObject);
        UIObjectList.Add(roomFindUIObject);

        mainScrollRect = homeUIObject.GetComponentInChildren<ScrollRect>();

        UIButtonAction("setup");
    }

    // 메인 UI화면을 스와이프 할 수 있도록 하는 셋업 설정
    private void SwipeSetup()
    {
        // 페이지 개수만큼 배열크기 선언
        arrayScrollValue = new float[swipeUI.transform.childCount];

        // 스크롤 되는 페이지 사이 거리 설정. [0.0~1.0]
        valueDistance = 1f / (arrayScrollValue.Length -1);


        #region 뒤에 백그라운드 이미지를 깔아 좌우에 여분이 있는 UI. 각 페이지의 중앙값 설정
        // 슬라이더 수학적 동작 메커니즘 연구 필요
        // 백그라운드 이미지 때문에 좌우에 생기는 여백이 있으면 특정 slider value값 계산 공식이 필요한데 아직 구현 못함
        // 현재는 인스펙터 창에서 오브젝트의 위치값 조절해서 사용중



        // *note : 캔버스 비율이 조절되며 일정한 값이 필요한듯
        int screenWidth = 1080;

        // 모든 페이지의 x사이즈 합. 좌우로 스크롤 되는 contents안에 있는 모든 UI의 총 x길이
        float allPagesSize = swipeUI.GetComponent<RectTransform>().rect.width;
        // 디스플레이 사이즈에 비해 페이지1개가 얼만큼 초과되는지(페이지1개 - 디스플레이크기)
        float pageSurplusSize =  (allPagesSize / swipeUI.transform.childCount) - screenWidth;
        // 초과되는페이지 사이즈가 모든페이지(모든UI 총 길이)에 대해 몇퍼센트 해당하는지
        float surplusPercent = pageSurplusSize / allPagesSize;
       
        // 중앙 페이지 인덱스
        centerIndex = Mathf.FloorToInt((float)arrayScrollValue.Length / 2f);

        int surplusIndex = centerIndex;

        // 설명 : 스크롤 되는 각 페이지의 위치값을 각각의 중앙으로 설정
        // 원리 : 중앙이면 스크롤 값0.5f. 초과값 없음
        //        맨 왼쪽이면 스크롤값 0f. 초과값 우측으로 pageSurplusSize*1
        //        맨 오른쪽이면 스크롤값 1f. 초과값 좌측으로 pageSurplusSize*1
        //        아 모르겠다 
        for (int i = 0; i < arrayScrollValue.Length; ++i)
        {
            if(i < centerIndex)
            {
                float adjust = surplusPercent * ((float)surplusIndex / (float)centerIndex);

                arrayScrollValue[i] = valueDistance * i + adjust/2;

                surplusIndex--;
            }

            else if (i > centerIndex)
            {
                surplusIndex++;

                float adjust = surplusPercent * ((float)surplusIndex / (float)centerIndex);

                arrayScrollValue[i] = valueDistance * i - adjust/2;  
            }

            else
            {
                arrayScrollValue[i] = valueDistance * i;
            }
        }
        #endregion

        // 최대 페이지 수 설정
        maxIndex = swipeUI.transform.childCount;


        SetScrollBarValue(centerIndex);
    }

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        Invoke("SwipeSetup", Time.deltaTime * 3f);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouchX = Input.mousePosition.x;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            endTouchX = Input.mousePosition.x;

            if (isSwipeMode == false && canSwipe)
            {
                UpdateSwipe();
            }
        }
#endif

#if UNITY_ANDROID
        if(Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                startTouchX = touch.position.x;
            }    
            else if(touch.phase == TouchPhase.Ended && canSwipe)
            {
                endTouchX = touch.position.x;

                UpdateSwipe();
            }
        }
#endif
    }

    private void UpdateSwipe()
    {
        if(Mathf.Abs(startTouchX-endTouchX) < swipeDistance)
        {
            StartCoroutine(OnSwipeOneStep(currentIndex));
            return;
        }

        bool isLeft = startTouchX < endTouchX ? true : false;

        if(isLeft)
        {
            if(currentIndex <= 0)
            {
                StartCoroutine(OnSwipeOneStep(currentIndex));
                return;
            }

            currentIndex--;
        }

        else
        {
            if(currentIndex >= maxIndex -1)
            {
                StartCoroutine(OnSwipeOneStep(currentIndex));
                return;
            }

            currentIndex++;
        }

        StartCoroutine(OnSwipeOneStep(currentIndex));
    }

    private IEnumerator OnSwipeOneStep(int index)
    {
        bottomButtons[currentButtonIndex].GetComponent<SpriteChanger>().Deselected();
        bottomButtons[index].GetComponent<SpriteChanger>().Selected();
        currentButtonIndex = index;

        float start = scrollBar.value;
        float current = 0;
        float percent = 0;

        isSwipeMode = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;

            scrollBar.value = Mathf.Lerp(start, arrayScrollValue[index], percent);

            yield return null;
        }

        isSwipeMode = false;
    }

    private void SetScrollBarValue(int index)
    {
        currentIndex = index;
        scrollBar.value = arrayScrollValue[index]; // scrollBar.value 는 [0.0f~1.0f] 사이의 값을 갖는다
    }

    // 역할 : 플레이어 조작에 의해 열고 닫히는 UI가 무엇인지 체크한 후 이어서 닫히고 열리는 다른 UI를 자동적으로 조작
    // 세팅 : 하이어라키 버튼오브젝트에 매개변수로써 작동하는 string값을 설정
    // 작동 : 조작한 버튼의 작성된 매개변수 string값에 따라 타 UI오브젝트를 활성화/비활성화 한다.
    public void UIButtonAction(string openObject)
    {
        #region Reset All Taps When Start Game First; 

        // 첫 실행시 홈 탭을 제외한 모든 탭 비실행
        if (openObject.ToLower() == "setup")
        {
            foreach (GameObject ui in UIObjectList)
            {
                if(!ui.name.ToLower().Contains("home"))
                {
                    // Home 탭을 제외한 모든 UI 비활성화 코드
                    // ui.gameObject.SetActive(false);
                }
                else
                {
                    ui.gameObject.SetActive(true);

                    currentMainUIObject = ui;
                }
            }

            return;
        }
        #endregion

        #region Change the Tap;
        // 팝업UI가 아닌, 큰 메인 UI만 해당

        foreach (GameObject ui in UIObjectList)
        {
            if (ui.name.ToUpper().Contains(openObject.ToUpper()))
            {
                ui.gameObject.SetActive(true);

                // 방찾기 버튼을 누르면 네트워크 로비에 접속
                if (ui.name.ToLower().Contains("roomfind"))
                {
                    lobbyNetwork.FindRoom();
                }

                // 이전에 실행중이었던 UI탭을 종료하고, 새롭게 시작되는 UI탭을 저장
                if (currentMainUIObject != null)
                {
                    // 이전에 실행중이었던 UI탭이 방찾기 탭이면 네트워크 로비를 떠남
                    if (currentMainUIObject.name.ToLower().Contains("roomfind"))
                    {
                        lobbyNetwork.LeaveFindRoom();

                        SetScrollBarValue(centerIndex);
                    }
                    //이전에 실행중이었던 UI탭을 종료
                    currentMainUIObject.gameObject.SetActive(false);
                }
                //새롭게 시작되는 UI탭을 저장
                currentMainUIObject = ui;

                return;
            }
        }

        Debug.LogWarning($"{openObject} 탭을 찾지 못함");
#endregion;
    }

    public void PageButtonAction(int pageNum)
    {
        currentIndex = pageNum;

        StartCoroutine(OnSwipeOneStep(currentIndex));
    }

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}