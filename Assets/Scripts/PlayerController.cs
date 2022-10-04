using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    // Player Stat Setting Value
    [SerializeField]
    private float movingSpeed = 2;

    // Player Stat
    [SerializeField]
    private TextMeshProUGUI nameText;

    // Network Interation Test 
    [SerializeField]
    private TextMeshProUGUI interactionText;
    private int interactionTime = 0;
    [SerializeField]
    private GameObject keyDownImage;

    public static GameObject LocalPlayerInstance;

    private void Awake()
    {
        keyDownImage.SetActive(false);

        if(photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        nameText.text = PhotonNetwork.NickName;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        UpdateMoving();
        UpdateInteraction();
    }

    private void UpdateMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        transform.position += new Vector3(h * movingSpeed * Time.deltaTime, v * movingSpeed * Time.deltaTime, 0);
    }

    private void UpdateInteraction()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            interactionTime++;

            interactionText.text = interactionTime.ToString();
        }
        else if(Input.GetKey(KeyCode.Space))
        {
            keyDownImage.SetActive(true);
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            keyDownImage.SetActive(false);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.LeaveRoom();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // photonView.IsMine == true, 즉 사건이 일어난 클라이언트가 LocalPlayer일 때 writing 할 수 있다. 그렇지 않으면 reading한다.
        if(stream.IsWriting)
        {
            stream.SendNext(keyDownImage.activeSelf);
            stream.SendNext(interactionTime);
            stream.SendNext(PhotonNetwork.NickName);
        }
        else
        {
            this.keyDownImage.SetActive((bool)stream.ReceiveNext());
            this.interactionText.text = stream.ReceiveNext().ToString();
            this.nameText.text = stream.ReceiveNext().ToString();
        }
    }
}
