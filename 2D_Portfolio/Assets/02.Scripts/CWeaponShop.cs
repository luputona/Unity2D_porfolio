using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWeaponShop : MonoBehaviour
{
    [SerializeField]
    private GameObject m_weaponShopScrollViewObj;
    [SerializeField]
    private GameObject m_itemList_Content;
    [SerializeField]
    private CTouchSpriteCheck m_touchSpriteCheck;

    public List<GameObject> m_slots = new List<GameObject>();
    

    public int tempSlotCount;

    public GameObject m_shopSlot;
    public GameObject m_shopItem;

    void Awake()
    {
        m_touchSpriteCheck = this.GetComponent<CTouchSpriteCheck>();
        tempSlotCount = 10;

        m_weaponShopScrollViewObj = GameObject.Find("ItemList_ScrollView").gameObject;
        m_itemList_Content = m_weaponShopScrollViewObj.transform.Find("ItemList_Viewport").transform.Find("ItemList_Content").gameObject;

        for (int i = 0; i < tempSlotCount; i++)
        {
            m_slots.Add(Instantiate(m_shopSlot));
            m_slots[i].transform.SetParent(m_itemList_Content.transform);
        }
    }

    void Start()
    {
        
        
    }


}
