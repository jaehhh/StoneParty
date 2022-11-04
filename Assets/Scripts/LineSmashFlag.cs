using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

// 동일한 판단을 위해 masterClient에서 점령 상태 관리
// 변경된 데이터는 포톤뷰의 도움없이 각 클라이언트가 LineSmashManager스크립트에서 데이터를 가지고 있는다

public class LineSmashFlag : MonoBehaviourPunCallbacks
{
    // 타 오브젝트
    private LineSmashManager manager;

    // 자식 UI
    [SerializeField]
    private Slider slider;

    // 상태 값
    [SerializeField]
    private string flagColor;
    private float occupationTime = 5f;
    private int enemyCount, alleyCount;
    private bool increasing, decreasing; // 코루틴 동작 여부 판단
    [HideInInspector]
    public bool canOccupation = false;

    // 깃발 색상 변경할 때 필요함
    [SerializeField]
    private Material[] mats;
    [SerializeField]
    private GameObject matTarget;

    private float rpcDelay = 0.1f;
    private float currentRpcDelay;

    private void Awake()
    {
        manager = GameObject.FindObjectOfType<LineSmashManager>().GetComponent<LineSmashManager>();
    }

    private void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Occupy();
        }
    }

    private void Occupy()
    {
        currentRpcDelay += Time.deltaTime;

        if(currentRpcDelay <= rpcDelay)
        {
            return;
        }
        else
        {
            currentRpcDelay = 0;
        }

        if (canOccupation == false || manager.canOccupation == false)
        {
            // 점령할 수 없는 상태가 되면 증감 중지
            if(decreasing == true || increasing == true)
            {
                photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
                photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);

                photonView.RPC("RPCSliderValueSet", RpcTarget.AllBufferedViaServer, 0f);
                photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false);
            }

            return;
        }

        // 깃발에 상대방 있을 때 일단 무조건 슬라이더 표시
        if (enemyCount > 0 && slider.IsActive() == false)
        {
            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, true);
        }

        // 깃발에 아군이 상대보다 적으면 점령수치 증가
        if (alleyCount < enemyCount && increasing == false)
        {
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, true);
        }
        // 아군과 적군의 수가 같을 때 점령수치 증감 모두 중지
        else if (alleyCount == enemyCount && (decreasing == true || increasing == true))
        {
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);
        }
        // 깃발에 상대가 아군보다 적으면 점령수치 감소
        else if (enemyCount < alleyCount && decreasing == false && slider.IsActive())
        {
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, true);
        }
    }

    [PunRPC]
    private void RPCOccupationCoroutine(bool increase, bool start)
    {
        if (increase && start)
        {
            increasing = true;
            StartCoroutine("OccupationIncrease");
        }
        else if (increase && !start)
        {
            increasing = false;
            StopCoroutine("OccupationIncrease");  
        }
        else if (!increase && start)
        {
            decreasing = true;
            StartCoroutine("OccupationDecrease");
        }
        else
        {
            decreasing = false;
            StopCoroutine("OccupationDecrease");
        }
    }

    // 점령수치 증가. 각 클라이언트가 수행
    private IEnumerator OccupationIncrease()
    {
        Debug.LogWarning("OccupationIncrease() 코루틴 작동");

        while (slider.value < 1f)
        {
            slider.value += Time.deltaTime / occupationTime;

            yield return null;
        }

        // 점령 완료

        // 적과 아군 수 서로 바꿈
        int temp = enemyCount;
        enemyCount = alleyCount;
        alleyCount = temp;

        increasing = false;


        if(PhotonNetwork.IsMasterClient)
        {
            // 깃발 색상 변경
            if (flagColor == "blue")
            {
                flagColor = "orange";
                photonView.RPC("RPCFlagColorChange", RpcTarget.AllBufferedViaServer, 1);
            }
            else
            {
                flagColor = "blue";
                photonView.RPC("RPCFlagColorChange", RpcTarget.AllBufferedViaServer, 0);
            }

            // 슬라이더 값0 및 비활성화
            photonView.RPC("RPCSliderValueSet", RpcTarget.AllBufferedViaServer, 0f);
            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false);

            // 잠시동안 재점령 불가능
            canOccupation = false;
            photonView.RPC("RPCOccupationCooldownCoroutine", RpcTarget.AllBufferedViaServer);

            // 점령 성공 판단 LineSmashManager로 전달. UI와 점령값 등 변경
            photonView.RPC("RPCOccupationSuccess", RpcTarget.AllBufferedViaServer, flagColor); // ====================> 문제발생
        }
    }

    // 점령수치 감소
    private IEnumerator OccupationDecrease()
    {
        while (slider.value > 0f)
        {
            slider.value -= Time.deltaTime / occupationTime;

            yield return null;
        }

        decreasing = false;

        photonView.RPC("RPCSliderValueSet", RpcTarget.AllBufferedViaServer, 0f);
        photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false);
    }

    // 슬라이더 온오프
    [PunRPC]
    private void RPCSliderSetActive(bool value)
    {
        slider.gameObject.SetActive(value);
    }
    // 슬라이더 값 조정
    [PunRPC]
    private void RPCSliderValueSet(float value)
    {
        slider.value = value;
    }

    // 점령 성공
    [PunRPC]
    private void RPCOccupationSuccess(string color)
    {
        manager.GetComponent<ParticleManager>().ActiveOccupyParticle(this.transform.position + new Vector3(0, 2.7f, 0));

        manager.OccupationSuccess(color);   
    }

    // 점령성공 후 잠시동안 점령불가능
    [PunRPC]
    private void RPCOccupationCooldownCoroutine()
    {
        StartCoroutine("OccupationCooldown");
    }
    // 점령성공 후 잠시동안 점령불가능
    private IEnumerator OccupationCooldown()
    {
        yield return new WaitForSeconds(2f);

        canOccupation = true;
    }

    // 깃발 색상 변경
    [PunRPC]
    private void RPCFlagColorChange(int num)
    {
        matTarget.GetComponent<MeshRenderer>().material = mats[num];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 상대팀이 깃발에 도달했을 때
        if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "orange") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "blue"))
        {
            enemyCount++;
        }
        // 우리팀이 깃발에 도달했을 때
        else if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "blue") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "orange"))
        {
            alleyCount++;
        }

        Debug.LogWarning($"OnTriggerEnter() enemyCount : {enemyCount}, alleyCount : {alleyCount}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 상대팀이 깃발을 벗어났을 때
        if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "orange") ||
        (other.gameObject.CompareTag("PlayerOrange") && flagColor == "blue"))
        {
            enemyCount--;
        }
        // 우리팀이 깃발을 벗어났을 때
        else if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "blue") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "orange"))
        {
            alleyCount--;
        }

        Debug.LogWarning($"OnTriggerExit() enemyCount : {enemyCount}, alleyCount : {alleyCount}");
    }
}
