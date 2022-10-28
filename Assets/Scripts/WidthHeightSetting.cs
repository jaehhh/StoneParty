using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidthHeightSetting : MonoBehaviour
{
    //private RectTransform rectTransform;

    private float wMulti = 0.92f;
    private float hMulti = 0.46f;


    private void Start()
    {
        StartCoroutine("FixSize");
    }

    private IEnumerator FixSize()
    {
        Debug.LogWarning("FixSize Coroutine Start");

        while (Screen.orientation != ScreenOrientation.Portrait) yield return null;

        yield return new WaitForSeconds(Time.deltaTime * 2);

        Debug.LogWarning("FixSize Coroutine End");

        float w = Screen.width * wMulti;
        float h = Screen.height * hMulti;

        //canvas scaler에서 match로 조절되기 때문에 고정값을 사용해야한다. Screen.height등 사용X
        float cw = 1080 * wMulti;
        float ch = 2280 * hMulti;

        if (h > w)
        {
            cw = ch;
        }
        else
        {
            ch = cw;
        }

        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(cw * 1.1f, ch * 1.1f);
    }
}
