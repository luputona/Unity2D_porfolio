using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CShopItemManager : MonoBehaviour
{
    [SerializeField]
    private GameObject shopSlotPrefab;
    [SerializeField]
    private int m_shopSlotCount;
    [SerializeField]
    private GameObject m_weaponShopScrollViewObj;
    [SerializeField]
    private GameObject m_itemList_Content;

    public List<GameObject> m_slots = new List<GameObject>();

    private void Awake()
    {
        m_weaponShopScrollViewObj = GameObject.Find("03_inst_ItemList_ScrollView").gameObject;
        m_itemList_Content = m_weaponShopScrollViewObj.transform.Find("ItemList_Viewport").transform.Find("ItemList_Content").gameObject;
    }

    // Use this for initialization
    void Start ()
    {
        m_shopSlotCount = 10;

    }
    public void CreatedShopListSlot()
    {
        //TODO : 웨폰상점 슬롯 임시 생성

        for (int i = 0; i < m_shopSlotCount; i++)
        {
            m_slots.Add(Instantiate(shopSlotPrefab));
            m_slots[i].transform.SetParent(m_itemList_Content.transform, false);
        }
    }

}
