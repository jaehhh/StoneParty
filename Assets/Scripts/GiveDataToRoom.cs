using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;


// Lobby 씬에서 Room 씬으로 이동할 때 방 정보를 다음 씬으로 전달
// 방에서 로비로 나갈 때 DestroyRoomData()메소드를 호출해 파괴해야 함

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
