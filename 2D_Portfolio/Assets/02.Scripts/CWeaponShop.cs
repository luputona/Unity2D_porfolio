using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CWeaponShop : MonoBehaviour
{
    [SerializeField]
    private GameObject m_weaponShopScrollViewObj;
    [SerializeField]
    private GameObject m_itemList_Content;
    [SerializeField]
    private CTouchSpriteCheck m_touchSpriteCheck;
    [SerializeField]
    private Text m_itemName_Text;
    [SerializeField]
    private Text m_itemCost_Text;
   
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
        
    }

    void Start()
    {
        //TODO : 서버에서 받아온 걸로 카테고리별 분기를 나누어서  생성으로 변경
        for (int i = 0; i < tempSlotCount; i++)
        {
            m_slots.Add(Instantiate(m_shopSlot));
            m_slots[i].transform.SetParent(m_itemList_Content.transform,false);
        }
    }

    //생성된 슬롯 UI에 Json 데이터를 전달
    public void InsertItemData()
    {
        for (int i = 0; i < CSwordData.GetInstance.m_swordItemList.Count; i++)
        {
            m_itemName_Text = m_slots[i].transform.Find("ItemName_Text").GetComponent<Text>();
            m_itemCost_Text = m_slots[i].transform.Find("ItemCost_Text").GetComponent<Text>();
            m_itemName_Text.text = string.Format("{0}", CSwordData.GetInstance.m_swordItemList[i].m_name);
            m_itemCost_Text.text = string.Format("{0}", CSwordData.GetInstance.m_swordItemList[i].m_cost);

        }
    }

    public void CreateItemList()
    {

    }

    

}
