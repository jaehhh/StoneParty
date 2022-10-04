using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModTable : MonoBehaviour
{
    // ��� ���̺�
    public enum e_Mod { LineSmash, e_ModLength};

    // �� ���̺�
    public enum e_Map { forest, e_MapLength };
    public Sprite[] mapSprites; // ������ �°� ��� �ʿ�

    public e_Mod[] mods = new e_Mod[(int)e_Mod.e_ModLength]; // ��� ���

    private void Test()
    {
        // ����� ���� ����
        for(int i = 0; i < mods.Length; i++)
        {
            mods[i] = (e_Mod)i;
        }
    }
}
