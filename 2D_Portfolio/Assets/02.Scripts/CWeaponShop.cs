using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CWeaponShop : CVillageManager
{
    [SerializeField]
    private CItemShopManager m_cItemShopManager;
    [SerializeField]
    private CVillageManager m_touchSpriteCheck;
    [SerializeField]
    private Text m_itemName_Text;
    [SerializeField]
    private Text m_itemCost_Text;
   
    public GameObject m_shopItem;
    
    //protected new GameObject ShopSlotPrefab
    //{
    //    get
    //    {
    //        return base.ShopSlotPrefab;
    //    }
    //}
    
    void Awake()
    {
        //Init();
        m_touchSpriteCheck = this.GetComponent<CVillageManager>();
    }

    void Start()
    {
       
    }
    void Update()
    {
        TouchGetObj();        
    }

    public void InitializeWeaponShop()
    {
        m_cItemShopManager = this.GetComponent<CItemShopManager>();
        base.InitVillageManager();
    }

    public override void InsertShopDictionary()
    {
        base.InsertShopDictionary();
    }

    protected override void TouchGetObj()
    {
        base.TouchGetObj();        
    }

    protected override void OpenShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.WeaponShop)
        {
            Debug.Log("웨폰샵");
            m_shopPanel.SetActive(true);
            m_shop[4].SetActive(true);
            m_shopDictionary[ShopInfo.WeaponShop].SetActive(true);
            m_cShopCategory.m_eBackUiState = CSelectCategory.EBACKUISTATE.Closed;           
        }
    }

    //생성된 슬롯 UI에 Json 데이터를 전달 
    public void InsertSwordItemData()
    {
        for (int i = 0; i < CSwordData.GetInstance.m_swordItemList.Count; i++)
        {
            m_itemName_Text = m_cItemShopManager.m_slots[i].transform.Find("ItemName_Text").GetComponent<Text>();
            m_itemCost_Text = m_cItemShopManager.m_slots[i].transform.Find("ItemCost_Text").GetComponent<Text>();
            m_itemName_Text.text = string.Format("{0}", CSwordData.GetInstance.m_swordItemList[i].m_name);
            m_itemCost_Text.text = string.Format("{0}", CSwordData.GetInstance.m_swordItemList[i].m_cost);

        }
    }
    
    public void InsertBowItemData()
    {
        for (int i = 0; i < CBowData.GetInstance.m_bowItemList.Count; i++)
        {
            m_itemName_Text = m_cItemShopManager.m_slots[i].transform.Find("ItemName_Text").GetComponent<Text>();
            m_itemCost_Text = m_cItemShopManager.m_slots[i].transform.Find("ItemCost_Text").GetComponent<Text>();
            m_itemName_Text.text = string.Format("{0}", CBowData.GetInstance.m_bowItemList[i].m_name);
            m_itemCost_Text.text = string.Format("{0}", CBowData.GetInstance.m_bowItemList[i].m_cost);

        }
    }



    //임시 함수
    void OpenWeaponShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.WeaponShop)
        {
            Debug.Log("웨폰샵");
            m_shopPanel.SetActive(true);
            m_shop[4].SetActive(true);
            m_shopDictionary[ShopInfo.WeaponShop].SetActive(true);
            m_cShopCategory.m_eBackUiState = CSelectCategory.EBACKUISTATE.Closed;

        }
    }


}
