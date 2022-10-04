using UnityEngine;

[System.Serializable]
public class UserData
{
    public static UserData instance;

    public string PlayerName;
    public int Level;
    public int Exp;
    public int ProfileImageCode;

    public int EquippedItemCode; // value = 0 : none

    // option setting
    public float EffectVolume;
    public float BGMVolume;

    public UserData()
    {
        if(instance == null)
        instance = this;
    }
}