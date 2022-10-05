using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

using UnityEngine.UI;

public class MoveController : MonoBehaviourPunCallbacks
{
    // 다른 오브젝트
    public UIManagerInMainGame uiManagerInMainGame; 

    // 내 컴포넌트
    private Rigidbody rigid;
    private PlayerSound playerSound;

    public bool canMove; // 게임의 시작과 종료로부터 여부를 체크
    private bool canMoveForRespawn = true; // 죽고 리스폰했을 때 여부를 체크

    // 좌우이동 속도
    private float maxSpeed = 5; // 최대 속도
    private float acceleration = 10; // 초당 속도 증가량
    private float deceleration = 2f; // 초당 속도 감소량

    // 점프
    private float maxJumpForce = 10; // 최대 점프 높이
    private float minJumpForce = 3f; // 최소 점프량
    private float jumpMaxDelayTime = 0.08f; // 점프 선입력 유효시간
    private float jumpCurrentDelayTime; // 점프 입력 후 경과된 시간
    private float jumpButtonMaxTime = 0.15f; // 버튼 최대 입력 시간
    private float jumpButtonMinTime = 0.05f; // 버튼 최소 입력 시간
    private float jumpButtonCurrentTime; // 버튼 누름 경과된 시간

    // 대쉬
    private float dashForce = 5; // 대쉬량
    private float dashCooldown = 2; // 대쉬 쿨타임
    private float dashCurrentCooldown; // 쿨타임 대기시간
    private Image dashButtonCooldownImage; // 대쉬버튼 쿨다운 이미지

    public bool canJump = false;// 점프 가능성 유무 체크
    private float needGroundedTime = 0.05f;// 점프 가능 조건에 필요한 땅 안착 유지 시간
    private float currentGroundedTime; // 땅 안착을 유지한 시간

    private float respawnTime = 1;

    [SerializeField]
    private KeyCode dashKey = KeyCode.D;

    // 조작 상태 체크 값
    private bool isJumpButtonDown = false;
    private int moveDirection = 0; // 왼쪽 -1, 정지 0, 오른쪽 1

    // 테스트
    [SerializeField]
    private bool testing = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        playerSound = GetComponentInParent<PlayerSound>();

        if (testing)
        {
            canMove = true;

            Debug.LogWarning("맵 테스트중. 포톤뷰이즈마인 비화성화중");
        }
        else
        {
            if (!photonView.IsMine) return;


            // 모바일
            uiManagerInMainGame = FindObjectOfType<UIManagerInMainGame>().GetComponent<UIManagerInMainGame>();
            uiManagerInMainGame.jumpButtonDownEvent.AddListener(JumpButtonDownMobileSwitch);
            uiManagerInMainGame.jumpButtonUpEvent.AddListener(JumpButtonUpMobile);
            uiManagerInMainGame.dashButtonEvent.AddListener(Dash);
            uiManagerInMainGame.moveButtonEvent.AddListener(MovementMobileSwitch);

            // 공통

            dashButtonCooldownImage = GameObject.Find("DashButtonCooldownImage").GetComponent<Image>();
        }
    }

    private void Update()
    {
        /*
        // 속도에 따라 질량 변화
        float velX = rigid.velocity.x;
        if (velX <= 0)
        {
            velX *= -1;
        }
        rigid.mass = Mathf.Clamp(velX, 1f, 5f);*/

        if (!photonView.IsMine) return;

        if (canMove && canMoveForRespawn)
        {

#if UNITY_EDITOR
            Movement();
            Jump();  

            if (Input.GetKeyDown(dashKey))
            {
                Dash();
            }
#endif

            // 모바일
            JumpButtonDownMobile();
            MovementMobile();

            // 현재속도가 최대속도를 초과되면 감속
            if (rigid.velocity.x > maxSpeed || rigid.velocity.x < -maxSpeed)
            {

                Deceleration();
            }
            // 대쉬 쿨타임
            if (dashCurrentCooldown < dashCooldown)
            {
                dashCurrentCooldown += Time.deltaTime;
            }
        }      
    }

    #region PC Control

    private void Movement()
    {
        // 방향키 누르면 가속량만큼 좌우 이동 
        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow) == false)
        {
            // 일정 속도를 넘어가지 않음
            if (rigid.velocity.x > -maxSpeed)
            {
                rigid.velocity += Vector3.left * acceleration * Time.deltaTime;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow) == false)
        {
            // 일정 속도를 넘어가지 않음
            if (rigid.velocity.x < maxSpeed)
            {
                rigid.velocity += Vector3.right * acceleration * Time.deltaTime;
            }
        }
        // 방향키 누르지 않으면 감속량만큼 좌우 이동량 감소
        else
        {
            Deceleration();
        }
    }


    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpButtonDown = true;
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            if (isJumpButtonDown == false) return;

        // 점프키를 누른 시간
        jumpButtonCurrentTime += Time.deltaTime;

            if (jumpButtonCurrentTime >= jumpButtonMaxTime)
            {
                Jumping();
            }
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            if (isJumpButtonDown == false) return;

            Jumping();
        }
    }

    #endregion

    #region Moblie Control

    // 현 스크립트에서 Update()로 계속 돌림
    private void MovementMobile()
    {
        if (moveDirection == -1) // 좌측 이동
        {
            // 일정 속도를 넘어가지 않음
            if (rigid.velocity.x > -maxSpeed)
            {
                rigid.velocity += Vector3.left * acceleration * Time.deltaTime;
            }
        }
        else if (moveDirection == 1) // 우측 이동
        {
            // 일정 속도를 넘어가지 않음
            if (rigid.velocity.x < maxSpeed)
            {
                rigid.velocity += Vector3.right * acceleration * Time.deltaTime;
            }
        }
        // 방향키 누르지 않으면 감속량만큼 좌우 이동량 감소
        else
        {
            Deceleration();
        }
    }

    // 이벤트에 등록해놓고 이동버튼 누르면 value값 -1 혹은 1로 변경. 때면 0
    public void MovementMobileSwitch(int value)
    {
        moveDirection = value;
    }

    // 현 스크립트에서 Update()로 계속 돌리는 메소드
    private void JumpButtonDownMobile()
    {
        if(isJumpButtonDown)
        {
            // 점프키를 누른 시간
            jumpButtonCurrentTime += Time.deltaTime;

            // 점프키 입력 중지 혹은 오래 눌렀을 때 점프
            if(jumpButtonCurrentTime >= jumpButtonMaxTime)
            {
                Jumping();
            }
        }
    }

    private void Jumping()
    {
        StopCoroutine(DelayJump());

        if (canJump == false)
        {
            StartCoroutine(DelayJump());
        }
        else
        {
            playerSound.JumpSound();

            float percent = jumpButtonCurrentTime / jumpButtonMaxTime;
            percent = Mathf.Clamp(percent, 0f, 1f);
            float force = Mathf.Clamp(maxJumpForce * percent, minJumpForce, maxJumpForce);

            rigid.velocity += new Vector3(0, force, 0);

            canJump = false;
            currentGroundedTime = 0;

            jumpButtonCurrentTime = 0;
            isJumpButtonDown = false;
        }
    }

    // 이벤트로 등록해놓고 버튼을 눌렀을 때 bool값 변경하는 메소드
    public void JumpButtonDownMobileSwitch(bool value)
    {
        isJumpButtonDown = value;
    }

    // 이벤트로 등록해놓고 버튼을 땠을 때 점프하는 메소드
    public void JumpButtonUpMobile()
    {
        if (isJumpButtonDown == false) return;

        isJumpButtonDown = false;

        Jumping();
    }

    #endregion

    #region PC + Mobile Control

    // 선입력 점프
    private IEnumerator DelayJump()
    {
        jumpCurrentDelayTime = 0;
        float temp = jumpButtonCurrentTime;

        jumpButtonCurrentTime = 0;
        isJumpButtonDown = false;

        while (jumpCurrentDelayTime < jumpMaxDelayTime)
        {
            if (canJump)
            {
                // 점프하기

                playerSound.JumpSound();

                float percent = jumpButtonCurrentTime / jumpButtonMaxTime;
                percent = Mathf.Clamp(percent, 0f, 1f);
                float force = Mathf.Clamp(maxJumpForce * percent, minJumpForce, maxJumpForce);

                rigid.velocity += new Vector3(0, force, 0);

                canJump = false;
                currentGroundedTime = 0;

                break;
            }

            jumpCurrentDelayTime += Time.deltaTime;

            yield return null;
        }
    }

    // 좌우 이동속도 감속
    private void Deceleration()
    {
        if (rigid.velocity.x > 0)
        {
            rigid.velocity += Vector3.left * deceleration * Time.deltaTime;
        }
        else if (rigid.velocity.x < 0)
        {
            rigid.velocity += Vector3.right * deceleration * Time.deltaTime;
        }
    }

    private void Dash()
    {
        // 쿨타임 체크
        if(dashCurrentCooldown < dashCooldown)
        {
            return;
        }
        if (rigid.velocity.x > 0)
        {
            rigid.velocity += Vector3.right * dashForce;

            dashCurrentCooldown = 0;

            playerSound.DashSound();

            StartCoroutine(DashButtonCooldownImageCoroutine());
        }
        else if (rigid.velocity.x < 0)
        {
            rigid.velocity += Vector3.left * dashForce;

            dashCurrentCooldown = 0;

            playerSound.DashSound();

            StartCoroutine(DashButtonCooldownImageCoroutine());
        }
        else
        {
            return;
        }
    }

    private IEnumerator DashButtonCooldownImageCoroutine()
    {
        float percent = 0;

        while (percent <= 1f)
        {
            percent = dashCurrentCooldown / dashCooldown;

            dashButtonCooldownImage.fillAmount = 1f - percent;

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    #endregion Control

    private IEnumerator Respawn()
    {
        GetComponentInParent<PlayerInGameManager>().RendererEnable(false);

        yield return new WaitForSeconds(respawnTime);

        this.transform.position = GetComponentInParent<PlayerInGameManager>().MainGameManager.RespawnPlayer();

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponentInParent<PlayerInGameManager>().RendererEnable(true);

        canMoveForRespawn = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.transform.CompareTag("DeadGround"))
        {
            playerSound.DeathSound();

            canMoveForRespawn = false;
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            GetComponentInParent<PlayerInGameManager>().CreateParticleDeath(transform.position);

            StartCoroutine("Respawn");
        }
        else if(collision.transform.CompareTag("PlayerOrange") || collision.transform.CompareTag("PlayerBlue"))
        {
            float vel = collision.gameObject.GetComponent<Rigidbody>().velocity.x;
            if (vel < 0) vel *= -1;

            if (vel > 5f)
            playerSound.HitSound();
        }
    }

    // 땅에 닿아있는지 체크
    private void OnCollisionStay(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.transform.CompareTag("Ground"))
        {
            // 땅에 닿은 시간 누적
            currentGroundedTime += Time.deltaTime;

            // 일정 시간동안 땅에 닿아있어야 점프 가능
            if(currentGroundedTime >= needGroundedTime)
            {
                canJump = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.transform.CompareTag("Ground"))
        {
            canJump = false;
        }
    }
}
