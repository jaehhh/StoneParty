using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime; 

public class LineSmashManager : MonoBehaviourPunCallbacks
{
    // ����
    private MainGameManager mainGameManager;

    // ���� �� ��������Ʈ
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private LineSmashFlag[] flags;

    // �ٸ� ��ũ��Ʈ���� �����ϴ� ���°�
    [HideInInspector]
    public bool canOccupation;

    // ���� ���� UI
    private Image[] occupationPoints;
    [SerializeField]
    private Transform occupationPointsParent;
    [SerializeField]
    private GameObject occupationPointUI;
    [SerializeField]
    private Color[] colors;
    
    // ���� ���� ��
    int blueCount = 0;
    int orangeCount = 0;

    private void Awake()
    {
        OccupationUISetup();

        mainGameManager = GetComponent<MainGameManager>();

        mainGameManager.spawnPoint = SetSpawnPoint(); // �������� MainGameManager�� ����
        mainGameManager.gameStateChangeEvent.AddListener(OccupationPossibleState); // ���ӽ����ϸ� ������ �� �ֵ���
    }

    private void Start()
    {
        // �ʱ� ���濡 �ִ� ��� ���ɰ��� ���� Ȱ��ȭ

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

    // ���� ���� UI ����
    private void OccupationUISetup()
    {
        // ��� ������ ���� �迭 ũ�� ����
        occupationPoints = new Image[flags.Length];

        // ��� ������ŭ UI ����
        for (int i = 0; i < flags.Length; i++)
        {
            GameObject clone = Instantiate(occupationPointUI, Vector3.zero, Quaternion.identity);
            clone.transform.SetParent(occupationPointsParent);
            occupationPoints[i] = clone.GetComponent<Image>();

            // ù ������ �Ķ���
            if (i < occupationPoints.Length / 2)
            {
                occupationPoints[i].color = colors[0];
                blueCount++;
            }
            // ������ ������ ��Ȳ��
            else
            {
                occupationPoints[i].color = colors[1];
                orangeCount++;
            }
        }
    }

    // ��� ���ɿ� �����ϸ� ���ɻ��� UI �����ϴ� �޼ҵ�
    public void OccupationSuccess(string color) // �Ű������� ���� ������ ���� ������ �޴´�
    {
        int sub = (blueCount - orangeCount) / 2; // ���� �������� ��� ���� ���� 
        int flagNumber = 0;

        if (color == "blue")
        {
            flagNumber = occupationPoints.Length / 2 + sub;

            occupationPoints[flagNumber].color = colors[0];
            blueCount++;
            orangeCount--;

            // ���� ���� ��� ����
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

            // ���� ���� ��� ����
            if (flagNumber != occupationPoints.Length - 1 && flagNumber != 0)
            {
                flags[flagNumber - 1].CanOccChange(true);
                flags[flagNumber + 1].CanOccChange(false);
            }
        }

        Debug.LogWarning($"num : {sub}, flagNumber : {flagNumber}, blueCount : {blueCount}, orangeCount : {orangeCount}");

        // �������� ����
        mainGameManager.spawnPoint = SetSpawnPoint();

        JudgmentVictory(flagNumber);
    }

    // �¸� �Ǵ�
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
