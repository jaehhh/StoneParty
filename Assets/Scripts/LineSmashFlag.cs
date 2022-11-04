using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

// ������ �Ǵ��� ���� masterClient���� ���� ���� ����
// ����� �����ʹ� ������� ������� �� Ŭ���̾�Ʈ�� LineSmashManager��ũ��Ʈ���� �����͸� ������ �ִ´�

public class LineSmashFlag : MonoBehaviourPunCallbacks
{
    // Ÿ ������Ʈ
    private LineSmashManager manager;

    // �ڽ� UI
    [SerializeField]
    private Slider slider;

    // ���� ��
    [SerializeField]
    private string flagColor;
    private float occupationTime = 5f;
    private int enemyCount, alleyCount;
    private bool increasing, decreasing; // �ڷ�ƾ ���� ���� �Ǵ�
    [HideInInspector]
    public bool canOccupation = false;

    // ��� ���� ������ �� �ʿ���
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
            // ������ �� ���� ���°� �Ǹ� ���� ����
            if(decreasing == true || increasing == true)
            {
                photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
                photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);

                photonView.RPC("RPCSliderValueSet", RpcTarget.AllBufferedViaServer, 0f);
                photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false);
            }

            return;
        }

        // ��߿� ���� ���� �� �ϴ� ������ �����̴� ǥ��
        if (enemyCount > 0 && slider.IsActive() == false)
        {
            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, true);
        }

        // ��߿� �Ʊ��� ��뺸�� ������ ���ɼ�ġ ����
        if (alleyCount < enemyCount && increasing == false)
        {
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, true);
        }
        // �Ʊ��� ������ ���� ���� �� ���ɼ�ġ ���� ��� ����
        else if (alleyCount == enemyCount && (decreasing == true || increasing == true))
        {
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);
        }
        // ��߿� ��밡 �Ʊ����� ������ ���ɼ�ġ ����
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

    // ���ɼ�ġ ����. �� Ŭ���̾�Ʈ�� ����
    private IEnumerator OccupationIncrease()
    {
        Debug.LogWarning("OccupationIncrease() �ڷ�ƾ �۵�");

        while (slider.value < 1f)
        {
            slider.value += Time.deltaTime / occupationTime;

            yield return null;
        }

        // ���� �Ϸ�

        // ���� �Ʊ� �� ���� �ٲ�
        int temp = enemyCount;
        enemyCount = alleyCount;
        alleyCount = temp;

        increasing = false;


        if(PhotonNetwork.IsMasterClient)
        {
            // ��� ���� ����
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

            // �����̴� ��0 �� ��Ȱ��ȭ
            photonView.RPC("RPCSliderValueSet", RpcTarget.AllBufferedViaServer, 0f);
            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false);

            // ��õ��� ������ �Ұ���
            canOccupation = false;
            photonView.RPC("RPCOccupationCooldownCoroutine", RpcTarget.AllBufferedViaServer);

            // ���� ���� �Ǵ� LineSmashManager�� ����. UI�� ���ɰ� �� ����
            photonView.RPC("RPCOccupationSuccess", RpcTarget.AllBufferedViaServer, flagColor); // ====================> �����߻�
        }
    }

    // ���ɼ�ġ ����
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

    // �����̴� �¿���
    [PunRPC]
    private void RPCSliderSetActive(bool value)
    {
        slider.gameObject.SetActive(value);
    }
    // �����̴� �� ����
    [PunRPC]
    private void RPCSliderValueSet(float value)
    {
        slider.value = value;
    }

    // ���� ����
    [PunRPC]
    private void RPCOccupationSuccess(string color)
    {
        manager.GetComponent<ParticleManager>().ActiveOccupyParticle(this.transform.position + new Vector3(0, 2.7f, 0));

        manager.OccupationSuccess(color);   
    }

    // ���ɼ��� �� ��õ��� ���ɺҰ���
    [PunRPC]
    private void RPCOccupationCooldownCoroutine()
    {
        StartCoroutine("OccupationCooldown");
    }
    // ���ɼ��� �� ��õ��� ���ɺҰ���
    private IEnumerator OccupationCooldown()
    {
        yield return new WaitForSeconds(2f);

        canOccupation = true;
    }

    // ��� ���� ����
    [PunRPC]
    private void RPCFlagColorChange(int num)
    {
        matTarget.GetComponent<MeshRenderer>().material = mats[num];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // ������� ��߿� �������� ��
        if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "orange") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "blue"))
        {
            enemyCount++;
        }
        // �츮���� ��߿� �������� ��
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

        // ������� ����� ����� ��
        if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "orange") ||
        (other.gameObject.CompareTag("PlayerOrange") && flagColor == "blue"))
        {
            enemyCount--;
        }
        // �츮���� ����� ����� ��
        else if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "blue") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "orange"))
        {
            alleyCount--;
        }

        Debug.LogWarning($"OnTriggerExit() enemyCount : {enemyCount}, alleyCount : {alleyCount}");
    }
}
