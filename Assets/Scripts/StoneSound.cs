using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSound : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] rollingClips;
    [SerializeField]
    private AudioClip[] jumpDownClips;
    private int clipIndex;

    private AudioSource myAudio;
    private Rigidbody myRigid;
    private MoveController moveController;

    private float userDataVolume;
    private float maxPitch = 0.8f;
    private float minPitch = 0.25f;

    private bool isJumping = false;

    private void Start()
    {
        myAudio = GetComponent<AudioSource>();
        myRigid = GetComponentInParent<Rigidbody>();
        moveController = GetComponentInParent<MoveController>();

        clipIndex = Random.Range(0, rollingClips.Length);
        myAudio.clip = rollingClips[clipIndex];
        myAudio.Play();

        if (UserData.instance != null)
            userDataVolume = UserData.instance.EffectVolume;
        else userDataVolume = 1f;
    }

    private void Update()
    {
        float velX = myRigid.velocity.x;

        if (velX < 0) velX *= -1;

        if (velX > 0.4f)
        {
            velX /= 6f; // 6f : max Speed Sound

            velX = Mathf.Clamp(velX, 0f, 1f);

            // volume
            float temp = userDataVolume;
            myAudio.volume = velX * temp;

            // pitch
            velX = Mathf.Clamp(velX, minPitch, maxPitch);
            myAudio.pitch = velX;

            if (moveController.canJump == false)
            {
                myAudio.Pause();

                isJumping = true;

                return;
            }

            // 재생 중지상태거나 재생 완료
            if (myAudio.isPlaying == false)
            {
                // 클립 변경
                if (isJumping)
                {
                    int i  = Random.Range(0, jumpDownClips.Length);

                    myAudio.clip = jumpDownClips[i];

                    isJumping = false;
                }
                else
                {             
                    while (true)
                    {
                        int i = Random.Range(0, rollingClips.Length);

                        if (clipIndex != i)
                        {
                            clipIndex = i;

                            break;
                        }
                    }

                    myAudio.clip = rollingClips[clipIndex];
                }
  
                myAudio.Play();
            }
        }
        // 속도 일정 이하면 돌굴러가는 소리 중단
        else
        {
            if (myAudio.isPlaying)
            {
                myAudio.Pause();
            }
        }
    }
}
