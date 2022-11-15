using UnityEngine;
using UnityEngine.UI;

public class IntroToon : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites;
    private int index = 1;

    private AudioSource myAudio;
    private float volume = -1f;

    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            ImageChange();
        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                ImageChange();
            }
        }
#endif
    }

    private void ImageChange()
    {
        if(volume <= 0)
        {
            if (UserData.instance != null)
                volume = UserData.instance.EffectVolume;
            else
                volume = 0.5f;

            myAudio.volume = volume;
        }

        myAudio.Stop();
        myAudio.Play();

        if (index == sprites.Length)
        {
            transform.parent.gameObject.SetActive(false);

            return;
        }

        GetComponentInChildren<Image>().sprite = sprites[index++];
    }

    public void Skip()
    {
        if (volume <= 0)
        {
            if (UserData.instance != null)
                volume = UserData.instance.EffectVolume;
            else
                volume = 0.5f;

            myAudio.volume = volume;
        }

        myAudio.Stop();
        myAudio.Play();

        transform.parent.gameObject.SetActive(false);
    }
}
