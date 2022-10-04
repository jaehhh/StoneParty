using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModTable : MonoBehaviour
{
    // 모드 테이블
    public enum e_Mod { LineSmash, e_ModLength};

    // 맵 데이블
    public enum e_Map { forest, e_MapLength };
    public Sprite[] mapSprites; // 순서에 맞게 등록 필요

    public e_Mod[] mods = new e_Mod[(int)e_Mod.e_ModLength]; // 모드 목록

    private void Test()
    {
        // 모드목록 변수 설정
        for(int i = 0; i < mods.Length; i++)
        {
            mods[i] = (e_Mod)i;
        }
    }
}
