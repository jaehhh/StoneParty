using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;


// Lobby ������ Room ������ �̵��� �� �� ������ ���� ������ ����
// �濡�� �κ�� ���� �� DestroyRoomData()�޼ҵ带 ȣ���� �ı��ؾ� ��

public class GiveDataToRoom : MonoBehaviour
{
    [HideInInspector]
    public RoomOptions roomOption = new RoomOptions();

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void DestroyRoomData()
    {
        Destroy(this.gameObject);
    }
}
