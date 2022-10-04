using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Obstacle : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Vector3 startPosition;
    [SerializeField]
    private Vector3 endPosition;
    [SerializeField]
    private float maxMoveSpeed; // 최대속도
    [SerializeField]
    private float minMoveSpeed; // 최소속도
    [SerializeField]
    private float accelerationTime; // 최대 속도 도달까지 걸리는 시간 (초)
    [SerializeField]
    private float decelerateDistance; // 감속에 들어가는 거리 (0~1f)

    // Debug
    [SerializeField]
    private float moveSpeed;

    private void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            //photonView.RPC("RPCMoveCoroutine",RpcTarget.AllBuffered);

            StartCoroutine("StraightMove"); // Photon Transform View 로 인해 RPC 호출하지 않아도 위치값 공유됨
        }

        StartCoroutine("StraightMove");
    }

    [PunRPC]
    private void RPCMoveCoroutine()
    {
        StartCoroutine("StraightMove");
    }

    private IEnumerator StraightMove()
    {
        moveSpeed = maxMoveSpeed;

        transform.position = startPosition;

        Vector3 destination = endPosition;
        Vector3 temp = startPosition;

        bool accelerate = false;
        bool canDecelerate = false;

        while (true)
        {
            float dis = Vector3.Distance(transform.position, destination);

            // 시작지점에서 처음으로 멀어저야 감속 가능함
            if(dis < 1f - decelerateDistance)
            {
                canDecelerate = true;
            }

            // 필요한 상태가 감속인지 가속인지 확인
            if(accelerate == false)
            {
                // 거리가 가까운지, 감속할 수 있는지 확인
                if(dis <= decelerateDistance && canDecelerate)
                {
                    moveSpeed -= Time.deltaTime * maxMoveSpeed / accelerationTime;
                }
            }
            // 가속이 필요한 상태
            else if(accelerate == true)
            {
                moveSpeed += Time.deltaTime * maxMoveSpeed / accelerationTime;

                if(moveSpeed >= maxMoveSpeed)
                {
                    accelerate = false;
                }
            }

            moveSpeed = Mathf.Clamp(moveSpeed, minMoveSpeed, maxMoveSpeed);

            // while()문에 의한 무기한 이동
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            
            // 목적지와 매우 가까우면 다시 반대로 이동하도록 설정
            if(dis <= 0.01f)
            {
               transform.position = destination;

               destination = temp;

                temp = transform.position;

                accelerate = true;
                canDecelerate = false;
            }

            yield return null;
        }
    }
}
