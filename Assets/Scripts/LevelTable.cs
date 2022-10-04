using UnityEngine;

[CreateAssetMenu(fileName = "New Level Table", menuName = "New Table/New Level Table")]

// 테이블 데이터를 활용할 수 있도록 메모리(변수)에 저장해놓는 스크립트
public class LevelTable : ScriptableObject
{
    public Sprite[] ProfileImages;

    public int[] MaxExp;
}
