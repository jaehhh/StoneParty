using UnityEngine;

[CreateAssetMenu(fileName = "New Map", menuName = "New Table/New Map")]
public class TableMap : ScriptableObject
{
    public string mapName;
    public Sprite mapSprite;
    public int[] maxPlayerInTeamMod;
    public int[] maxPlayerInPersonalMod;
}
