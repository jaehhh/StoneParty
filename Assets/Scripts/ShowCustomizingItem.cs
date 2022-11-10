using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Lobby ȭ�鿡�� Ŀ���͸���¡ �κ��� ���

public class ShowCustomizingItem : MonoBehaviour
{
    [SerializeField]
    private Transform targetContent;

    private CustomizingItem[] customizingItems; // ���������� ���̺�

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

    // ��ũ�����̺��� �����Ͽ� ������ ����
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

        // �ڵ� ������������ ����
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

    // UI�� Ŀ���͸���¡ ������ ��� ����
    private void SetImageBox()
    {
        // ������Ʈ 2D�̹����� ���� ���̽� UI
        GameObject box = Resources.Load<GameObject>("Image_CustomizingPreviewBox");

        for (int i = 0; i < customizingItems.Length; i++)
        {
            // ��ư ����
            GameObject clone = Instantiate(box, Vector3.zero, Quaternion.identity);
            clone.transform.SetParent(targetContent);

            clone.transform.localScale = Vector3.one;

            // ��ư�� �̹��� ����
            clone.transform.GetChild(0).GetComponent<Image>().sprite = customizingItems[i].image2D;

            // ��ư�� ��� ����
            int index = i;
            clone.GetComponent<Button>().onClick.AddListener(() => SelectItem(index));
            //clone.GetComponent<Button>().onClick.AddListener(() => SoundController.Instance.EffectSoundOn());
            // ������� �� �ȵ���???????????
        }
    }

    // ������ �׸�(��ư)�� Ŭ���Ͽ� �������� ����. �Ű������� ���° ��ư����
    public void SelectItem(int index = -1)
    {
        //
        // ���� �߰� �������� ���� �����͸� �о�鿩�� �������� ���� �Ǵ��ؾ� ��
        //

        //
        // ��ũ�����̺��� �����ʰ� �迭�� �����͸� �����ϴ� ������ ���� UI�� ���������� ��ġ�ؾ� �ϱ� ������
        //

        // �켱 ������ ������ ����
        if (equippedItem != null)
        {
            Destroy(equippedItem.gameObject);
        }

        // �ʱ� �¾������� �Ű������� -1 ����. Userdata ���� �� �������� �������� ����
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
        // �Ű������� ������ ������ �ε����� ������ ����
        else
        {
            // �������� ������ �ٽ� ���� : ������ ��ü
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
