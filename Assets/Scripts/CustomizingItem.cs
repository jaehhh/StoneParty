using UnityEngine;

[CreateAssetMenu(fileName = "New Customizing Item", menuName = "New Item/New Customizing Item")]
public class CustomizingItem : ScriptableObject
{
    public int itemCode;
    public string itemName;
    public GameObject object3D;
    public Sprite image2D;
}
