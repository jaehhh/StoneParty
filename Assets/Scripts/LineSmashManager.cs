using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime; 

public class LineSmashManager : MonoBehaviourPunCallbacks
{
    // 참조
    private MainGameManager mainGameManager;

    // 스폰 및 점령포인트
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private LineSmashFlag[] flags;

    // 다른 스크립트에서 참조하는 상태값
    [HideInInspector]
    public bool canOccupation;

    // 점령 상태 UI
    private Image[] occupationPoints;
    [SerializeField]
    private Transform occupationPointsParent;
    [SerializeField]
    private GameObject occupationPointUI;
    [SerializeField]
    private Color[] colors;
    
    // 점령 상태 값
    int blueCount = 0;
    int orangeCount = 0;

    private void Awake()
    {
        OccupationUISetup();

        mainGameManager = GetComponent<MainGameManager>();

        mainGameManager.spawnPoint = SetSpawnPoint(); // 스폰지점 MainGameManager로 전달
        mainGameManager.gameStateChangeEvent.AddListener(OccupationPossibleState); // 게임시작하면 점령할 수 있도록
    }

    private void Start()
    {
        // 초기 전방에 있는 깃발 점령가능 상태 활성화

        flags[occupationPoints.Length / 2 - 1].CanOccChange(true);
        flags[occupationPoints.Length / 2].CanOccChange(true);
    }

    private Vector3 SetSpawnPoint()
    {
        int num = (string)PhotonNetwork.LocalPlayer.CustomProperties["teamColor"] == "blue" ? blueCount - 1 : spawnPoints.Length - orangeCount;

        if(num < 0)
        {
            num++;
        }
        else if(num >= spawnPoints.Length)
        {
            num--;
        }

        Vector3 spawnPoint = spawnPoints[num].position;

        return spawnPoint;
    }

    // 점령 상태 UI 생성
    private void OccupationUISetup()
    {
        // 깃발 개수에 따라 배열 크기 선언
        occupationPoints = new Image[flags.Length];

        // 깃발 개수만큼 UI 생성
        for (int i = 0; i < flags.Length; i++)
        {
            GameObject clone = Instantiate(occupationPointUI, Vector3.zero, Quaternion.identity);
            clone.transform.SetParent(occupationPointsParent);
            occupationPoints[i] = clone.GetComponent<Image>();

            // 첫 절반은 파란색
            if (i < occupationPoints.Length / 2)
            {
                occupationPoints[i].color = colors[0];
                blueCount++;
            }
            // 나머지 절반은 주황색
            else
            {
                occupationPoints[i].color = colors[1];
                orangeCount++;
            }
        }
    }

    // 깃발 점령에 성공하면 점령상태 UI 변경하는 메소드
    public void OccupationSuccess(string color) // 매개변수로 점령 성공한 팀의 색상을 받는다
    {
        int sub = (blueCount - orangeCount) / 2; // 블루와 오렌지의 깃발 개수 차이 
        int flagNumber = 0;

        if (color == "blue")
        {
            flagNumber = occupationPoints.Length / 2 + sub;

            occupationPoints[flagNumber].color = colors[0];
            blueCount++;
            orangeCount--;

            // 점령 가능 깃발 변경
            if (flagNumber != occupationPoints.Length - 1 && flagNumber != 0)
            {
                flags[flagNumber - 1].CanOccChange(false);
                flags[flagNumber + 1].CanOccChange(true);
            }
        }
        else
        {
            flagNumber = occupationPoints.Length / 2 - 1 + sub;

            occupationPoints[flagNumber].color = colors[1];
            orangeCount++;
            blueCount--;

            // 점령 가능 깃발 변경
            if (flagNumber != occupationPoints.Length - 1 && flagNumber != 0)
            {
                flags[flagNumber - 1].CanOccChange(true);
                flags[flagNumber + 1].CanOccChange(false);
            }
        }

        Debug.LogWarning($"num : {sub}, flagNumber : {flagNumber}, blueCount : {blueCount}, orangeCount : {orangeCount}");

        // 스폰구역 변경
        mainGameManager.spawnPoint = SetSpawnPoint();

        JudgmentVictory(flagNumber);
    }

    // 승리 판단
    private void JudgmentVictory(int flagNumber)
    {
        int sub = blueCount - orangeCount;

        if(sub > 0)
        {
            mainGameManager.winner = "blue";
        }
        else if(sub < 0)
        {
            mainGameManager.winner = "orange";
        }
        else if (sub ==0)
        {
            mainGameManager.winner = "";
        }


        if (flagNumber == occupationPoints.Length-1 || flagNumber == 0)
        {
            mainGameManager.gameStateChangeEvent.Invoke(false);
        }
    }

    private void OccupationPossibleState(bool value)
    {
        canOccupation = value;
    }
}
