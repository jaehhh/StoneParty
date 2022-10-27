using UnityEngine;
using System.Collections.Generic;

public class MemoryPool
{
    private class PoolItem
    {
        public bool isActive;
        public GameObject gameObject;
    }

    private int startMaxCount = 3; // 초기 개수
    private int maxCount;
    private int activeCount;

    private GameObject poolObject;
    private List<PoolItem> poolItemList;

    public MemoryPool(GameObject poolObject)
    {
        maxCount = 0;
        activeCount = 0;
        this.poolObject = poolObject;

        poolItemList = new List<PoolItem>();

        InstantiateObjects(startMaxCount);
    }

    public void InstantiateObjects(int num = 1)
    {
        maxCount += num;

        for(int i = 0; i < num; ++i)
        {
            PoolItem poolItem = new PoolItem();

            poolItem.isActive = false;
            poolItem.gameObject = GameObject.Instantiate(poolObject);
            poolItem.gameObject.SetActive(false);
    
            poolItemList.Add(poolItem);
        }
    }

    public GameObject ActivePoolItem()
    {
        if (poolItemList == null) return null;

        // 모자르면 생성
        if(maxCount <= activeCount)
        {
            InstantiateObjects();
        }

        // 활성화 안된 오브젝트 찾기 -> SetAcitve true
        for(int i =0; i < poolItemList.Count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.isActive == false)
            {
                activeCount++;

                poolItem.isActive = true;
                poolItem.gameObject.SetActive(true);

                return poolItem.gameObject;
            }
        }

        return null;
    }

    public void DeactivePoolItem(GameObject deactiveItem)
    {
        if (poolItemList == null || deactiveItem == null) return;

        // 매개변수로 들어온 오브젝트와 리스트에 있는 동일한 오브젝트 검출 -> SetActive false
        for (int i = 0; i < poolItemList.Count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject == deactiveItem)
            {
                activeCount--;

                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);

                return;
            }
        }
    }
}
