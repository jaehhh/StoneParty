using UnityEngine;
using UnityEngine.UI;

public class IntroToon : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites;
    private int index = 1;

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
        if (index == sprites.Length)
        {
            transform.parent.gameObject.SetActive(false);

            return;
        }

        GetComponentInChildren<Image>().sprite = sprites[index++];
    }
}
