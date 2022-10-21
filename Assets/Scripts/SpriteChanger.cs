using UnityEngine;
using UnityEngine.UI;

public class SpriteChanger : MonoBehaviour
{
    [SerializeField]
    private Sprite selectedSprite;
    [SerializeField]
    private Sprite deselectedSprite;

    public void Selected()
    {
        GetComponent<Image>().sprite = selectedSprite;
    }

    public void Deselected()
    {
        GetComponent<Image>().sprite = deselectedSprite;
    }
}
