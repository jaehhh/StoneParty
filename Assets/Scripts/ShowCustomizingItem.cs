using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Lobby 화면에서 커스터마이징 부분을 담당

public class ShowCustomizingItem : MonoBehaviour
{
    [SerializeField]
    private Transform targetContent;

    private CustomizingItem[] customizingItems; // 장착아이템 테이블

    [SerializeField]
    private GameObject player;
    private GameObject equippedItem;

    private UserDataController dataController;
    // UserData UserData.instance;

    private void Awake()
    {
        dataController = FindObjectOfType<UserDataController>();
        //UserData.instance = Resources.Load("Table/UserData") as UserData;

        LoadCustomizingItems();
    }

    private void Start()
    {
        SetImageBox();

        SetPlayerEqiuppedItem();
    }

    // 스크립테이블을 참조하여 데이터 보관
    private void LoadCustomizingItems()
    {
        Object[] temp = Resources.LoadAll("Item");

        customizingItems = new CustomizingItem[temp.Length];

        for (int i = 0; i < temp.Length; i++)
        {
            customizingItems[i] = temp[i] as CustomizingItem;
        }

        Debug.Log("customizingItems.Length : " + customizingItems.Length);

        CustomizingItem tempCustomizingItem = null;

        // 코드 오름차순으로 정렬
        for (int i = 0; i < customizingItems.Length; i++)
        {
            for (int j = i+1; j < customizingItems.Length; j++)
            {
                if(customizingItems[i].itemCode > customizingItems[j].itemCode)
                {
                    tempCustomizingItem = customizingItems[j];
                    customizingItems[j] = customizingItems[i];
                    customizingItems[i] = tempCustomizingItem;
                }   
            }
        }
    }

    // UI상에 커스터마이징 아이템 목록 생성
    private void SetImageBox()
    {
        // 오브젝트 2D이미지를 담을 베이스 UI
        GameObject box = Resources.Load<GameObject>("Image_CustomizingPreviewBox");

        for (int i = 0; i < customizingItems.Length; i++)
        {
            // 버튼 생성
            GameObject clone = Instantiate(box, Vector3.zero, Quaternion.identity);
            clone.transform.SetParent(targetContent);

            clone.transform.localScale = Vector3.one;

            // 버튼에 이미지 적용
            clone.transform.GetChild(0).GetComponent<Image>().sprite = customizingItems[i].image2D;

            // 버튼에 기능 적용
            int index = i;
            clone.GetComponent<Button>().onClick.AddListener(() => SelectItem(index));
            //clone.GetComponent<Button>().onClick.AddListener(() => SoundController.Instance.EffectSoundOn());
            // 모바일은 왜 안되지???????????
        }
    }

    // 아이템 그림(버튼)을 클릭하여 아이템을 장착. 매개변수는 몇번째 버튼인지
    public void SelectItem(int index = -1)
    {
        //
        // 이후 추가 구현으로 유저 데이터를 읽어들여서 장착할지 말지 판단해야 함
        //

        //
        // 스크립테이블을 쓰지않고 배열에 데이터를 보관하는 이유는 추후 UI에 선택적으로 배치해야 하기 때문에
        //

        // 우선 장착된 아이템 제거
        if (equippedItem != null)
        {
            Destroy(equippedItem.gameObject);
        }

        // 초기 셋업에서는 매개변수로 -1 받음. Userdata 참조 후 장착중인 아이템을 장착
        if (index == -1)
        {
            int code = UserData.instance.EquippedItemCode;
            string itemName = "Customizing Item" + code.ToString();

            CustomizingItem customizingItem = Resources.Load("item/"+itemName) as CustomizingItem;

            if (customizingItem != null)
            {
                GameObject item = customizingItem.object3D;

                GameObject clone = Instantiate(item, player.transform);

                equippedItem = clone;
            } 
        }
        // 매개변수가 들어오면 지정한 인덱스의 아이템 장착
        else
        {
            // 장착중인 아이템 다시 장착 : 아이템 해체
            if (customizingItems[index].itemCode == UserData.instance.EquippedItemCode)
            {
                UserData.instance.EquippedItemCode = 0;
            }
            else
            {
                GameObject clone = Instantiate(customizingItems[index].object3D, player.transform);

                equippedItem = clone;

                UserData.instance.EquippedItemCode = customizingItems[index].itemCode;
            }

            dataController.Save();
        }
    }

    private void SetPlayerEqiuppedItem()
    {
        SelectItem();
    }
}
