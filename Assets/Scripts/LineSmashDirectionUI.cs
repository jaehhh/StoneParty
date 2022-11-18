using UnityEngine;
using UnityEngine.UI;

public class LineSmashDirectionUI : MonoBehaviour
{
    private float maxX, minX;
    private float posX;
    [SerializeField]
    private float dir = 1;
    private float currentTime;
    

    private void Awake()
    {
        posX = this.GetComponent<RectTransform>().position.x;
        maxX = posX + 0.5f;
        minX = posX - 0.5f;

        UpdateUI();
    }

    private void UpdateUI()
    {
        currentTime += Time.deltaTime;

        posX += 1f * dir * Time.deltaTime;

        this.GetComponent<RectTransform>().position = (new Vector3(posX, transform.position.y, transform.position.z));

        float temp = Mathf.Abs(posX);

        if (temp > maxX)
        {
            dir *= -1f;
        }
        else if(temp < minX)
        {
            dir *= -1f;
        }

        // 8초후 점점 사라지기
        Color color = this.GetComponent<Image>().color;

        if (currentTime >= 8)
        {
            color.a -= Time.deltaTime / 2f;

            this.GetComponent<Image>().color = color;

            if(color.a <= 0)
            {
                this.gameObject.SetActive(false);

                return;
            }
        }
        Invoke("UpdateUI", Time.deltaTime);
    }
}
