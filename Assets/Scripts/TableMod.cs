using UnityEngine;

[CreateAssetMenu(fileName = "New Mod", menuName = "New Table/New Mod")]
public class TableMod : ScriptableObject
{
    public string modName;
    public TableMap[] canUseMapsInTeamMod;
    public TableMap[] canUseMapInPersonalMod;
}
