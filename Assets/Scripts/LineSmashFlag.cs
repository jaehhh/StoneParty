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
    private LineSmashManager lineSmashManager;
    private MainGameManager mainGameManager;

    // �ڽ� UI
    [SerializeField]
    private Slider slider;

    // ���� ���� ��
    private float occupationTime = 5f; // ���� �ʿ� �ð�

    // ���� ��
    [SerializeField]
    private string flagColor;
    private int enemyCount, alleyCount;
    private bool increasing, decreasing; // �ڷ�ƾ ���� ���� �Ǵ�
    [HideInInspector]
    public bool canOccupation = false;
    private PhotonView lastCollider;

    // ��� ���� ������ �� �ʿ���
    [SerializeField]
    private Material[] mats;
    [SerializeField]
    private GameObject matTarget;

    private float rpcDelay = 0.1f;
    private float currentRpcDelay;

    // �� â��
    private ParticleController particleCanOcc;

    private void Awake()
    {
        lineSmashManager = GameObject.FindObjectOfType<LineSmashManager>().GetComponent<LineSmashManager>();
        mainGameManager = GameObject.FindObjectOfType<MainGameManager>().GetComponent<MainGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return; // ������ �ϴ°� �Ϸ���. ���׾���

        // ���ʷ� Ʈ���� �Ǵ� ��ü�� ���� RPC ȣ��
        // lastCollider = other.transform.parent.gameObject.GetPhotonView();
        // if (!lastCollider.IsMine) return;

        // ������� ��߿� �������� ��
        if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "orange") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "blue"))
        {
            photonView.RPC("CountChange", RpcTarget.AllBufferedViaServer, alleyCount, ++enemyCount);
        }
        // �츮���� ��߿� �������� ��
        else if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "blue") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "orange"))
        {
            photonView.RPC("CountChange", RpcTarget.AllBufferedViaServer, ++alleyCount, enemyCount);
        }

        Occupy();

        Debug.LogWarning($"OnTriggerEnter() enemyCount : {enemyCount}, alleyCount : {alleyCount}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return; // ������ �ϴ°� �Ϸ���. ���׾���

        // ���ʷ� Ʈ���� �Ǵ� ��ü�� ���� RPC ȣ��
        //lastCollider = other.transform.parent.gameObject.GetPhotonView();
        // if (!lastCollider.IsMine) return;

        // ������� ����� ����� ��
        if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "orange") ||
        (other.gameObject.CompareTag("PlayerOrange") && flagColor == "blue"))
        {
            photonView.RPC("CountChange", RpcTarget.AllBufferedViaServer, alleyCount, --enemyCount);
        }
        // �츮���� ����� ����� ��
        else if ((other.gameObject.CompareTag("PlayerBlue") && flagColor == "blue") ||
            (other.gameObject.CompareTag("PlayerOrange") && flagColor == "orange"))
        {
            photonView.RPC("CountChange", RpcTarget.AllBufferedViaServer, --alleyCount, enemyCount);
        }

        Occupy();

        Debug.LogWarning($"OnTriggerExit() enemyCount : {enemyCount}, alleyCount : {alleyCount}");
    }

    private void Occupy()
    {
        currentRpcDelay += Time.deltaTime * 4f;

        if(currentRpcDelay <= rpcDelay)
        {
            Invoke("Occupy", Time.deltaTime * 4f);
        }
        else
        {
            currentRpcDelay = 0;
        }

        // �����̴� �� ����
        float value = slider.value;
        photonView.RPC("RPCSliderValueSet", RpcTarget.AllBuffered, value);

        if (canOccupation == false || lineSmashManager.canOccupation == false)
        {
            // ������ �� ���� ���°� �Ǹ� ���� ����(�������� ��)
            if(decreasing == true || increasing == true)
            {
                photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
                photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, true, false);

                photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false, 0f);
            }

            return;
        }

        // ��߿� ���� ���� �� �ϴ� ������ �����̴� ǥ��
        if (enemyCount > 0 && slider.IsActive() == false)
        {
            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, true, -1f);
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
    private void RPCOccupationCoroutine(bool increase, bool start) // increase -> ���ɼ�ġ ���or�϶�, start -> ��ġ���� ������ ������ ���� ������
    {
        if (increase && start && !increasing)
        {
            increasing = true;
            StartCoroutine("OccupationIncrease");
        }
        else if (increase && !start && increasing)
        {
            increasing = false;
            StopCoroutine("OccupationIncrease");
        }
        else if (!increase && start && !decreasing)
        {
            decreasing = true;
            StartCoroutine("OccupationDecrease");
        }
        else if(!increase && !start && decreasing)
        {
            decreasing = false;
            StopCoroutine("OccupationDecrease");
        }
    }

    // ���ɼ�ġ ����. �� Ŭ���̾�Ʈ�� ����
    private IEnumerator OccupationIncrease()
    {
        while (slider.value < 1f)
        {
            slider.value += Time.deltaTime / occupationTime;

            if (!canOccupation)
            {
                slider.value = 0f;
                slider.gameObject.SetActive(false);

                break;
            }

            yield return null;
        }

        // ���� �Ϸ�

        
        if (PhotonNetwork.IsMasterClient)
        {
            // ���� �Ʊ� �� ���� �ٲ�
            photonView.RPC("CountChange", RpcTarget.AllBufferedViaServer, enemyCount, alleyCount);

            // ��� ���� ����
            if (flagColor == "blue")
            {
                flagColor = "orange";
                photonView.RPC("RPCFlagColor",RpcTarget.OthersBuffered, flagColor);
                photonView.RPC("RPCFlagColorChange", RpcTarget.AllBufferedViaServer, 1);
            }
            else
            {
                flagColor = "blue";
                photonView.RPC("RPCFlagColor", RpcTarget.OthersBuffered, flagColor);
                photonView.RPC("RPCFlagColorChange", RpcTarget.AllBufferedViaServer, 0);
            }

            // �����̴� ��0 �� ��Ȱ��ȭ
            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false, 0f);

            // ��õ��� ������ �Ұ���
            canOccupation = false;
            photonView.RPC("RPCCanOccupation", RpcTarget.AllBuffered, canOccupation);
            photonView.RPC("RPCOccupationCooldownCoroutine", RpcTarget.AllBufferedViaServer); // ������ ������ �Ұ��� Ǯ��

            // ���� ���� �Ǵ� LineSmashManager�� ����. UI�� ���ɰ� �� ����
            photonView.RPC("RPCOccupationSuccess", RpcTarget.AllBufferedViaServer, flagColor);

            photonView.RPC("RPCOccupationCoroutine", RpcTarget.OthersBuffered, true, false); 
        }
    }

    [PunRPC]
    private void RPCCanOccupation(bool value)
    {
        canOccupation = value;
    }

    public void CanOccChange(bool value)
    {
        if(canOccupation && value != canOccupation) // ���� -> �Ұ���
        {
            mainGameManager.GetComponent<ParticleManager>().DeactiveCanOccParticle(particleCanOcc);

            StopCoroutine("OccupationDecrease");
            StopCoroutine("OccupationIncrease");
        }
        else if(!canOccupation && value != canOccupation) // �Ұ��� -> ����
        {
            particleCanOcc = mainGameManager.GetComponent<ParticleManager>().ActiveCanOccParticle(this.transform.position);
        }

        slider.value = 0f;
        slider.gameObject.SetActive(false);

        canOccupation = value;
    }

    [PunRPC]
    private void RPCFlagColor(string color)
    {
        flagColor = color;
    }

    [PunRPC]
    private void CountChange(int alley, int enemy)
    {
        alleyCount = alley;
        enemyCount = enemy;
    }

    // ���ɼ�ġ ����. �� Ŭ���̾�Ʈ�� ����
    private IEnumerator OccupationDecrease()
    {
        while (slider.value > 0f)
        {
            slider.value -= Time.unscaledDeltaTime / occupationTime;
            
            if(!canOccupation)
            {
                slider.value = 0f;
                slider.gameObject.SetActive(false);

                break;
            }
            
            yield return null;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            // �Ʊ��� ���� ���ɼ�ġ ���� �Ϸ�

            photonView.RPC("RPCSliderSetActive", RpcTarget.AllBufferedViaServer, false, 0f);

            photonView.RPC("RPCOccupationCoroutine", RpcTarget.AllBufferedViaServer, false, false);
        }
    }


    // �����̴� �¿���
    [PunRPC]
    private void RPCSliderSetActive(bool value, float sliderValue = -1)
    {
        if (sliderValue >= 0) //��׶����϶� RPC ȣ��޾Ƶ� value�� ���� �����ϵ���.. �����̴��� ���������� value�� ������ �ȵ�
        {
            slider.gameObject.SetActive(true);
            slider.value = sliderValue;
        }
        slider.gameObject.SetActive(value);
    }

    // �����̴� �� ����
    [PunRPC]
    private void RPCSliderValueSet(float value)
    {
        slider.value = value;
    }

    [PunRPC]
    private void RPCIncreasing(bool value)
    {
        increasing = value;
    }
    [PunRPC]
    private void RPCDecreasing(bool value)
    {
        decreasing = value;
    }

    // ���� ����
    [PunRPC]
    private void RPCOccupationSuccess(string color)
    {
        lineSmashManager.GetComponent<ParticleManager>().ActiveOccupyParticle(this.transform.position + new Vector3(0, 2.7f, 0));

        lineSmashManager.OccupationSuccess(color);   
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


}
