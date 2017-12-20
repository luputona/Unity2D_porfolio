using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CWeaponShop : CVillageManager
{
    [SerializeField]
    private CItemShopSlotListManager m_cItemShopManager = null;
    [SerializeField]
    private CVillageManager m_touchSpriteCheck = null;
    [SerializeField]
    private Text m_itemName_Text = null;
    [SerializeField]
    private Text m_itemCost_Text = null;
    

    public Text m_itemDesc_Text = null;
    public GameObject m_shopItem = null;
    
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
        m_cItemShopManager = this.GetComponent<CItemShopSlotListManager>();
        
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


    public void ShowItemDataText(int tStart, int tEnd, string tName, int tCost)
    {
       
        m_itemName_Text = m_cItemShopManager.m_slots[tStart].transform.Find("ItemName_Text").GetComponent<Text>();
        m_itemCost_Text = m_cItemShopManager.m_slots[tStart].transform.Find("ItemCost_Text").GetComponent<Text>();
        m_itemDesc_Text = m_shopDictionary[ShopInfo.ItemDescription].gameObject.GetComponentInChildren<Text>();
        m_itemName_Text.text = string.Format("{0}", tName);
        m_itemCost_Text.text = string.Format("{0}", tCost);        
        
    }
    
    //생성된 슬롯 UI에 Json 데이터를 전달 
    public void InsertSwordItemData()
    {
        int tEnd = CSwordData.GetInstance.m_swordItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, tEnd, CSwordData.GetInstance.m_swordItemList[i].m_name, CSwordData.GetInstance.m_swordItemList[i].m_cost);       
        }
    }
    
    public void InsertBowItemData()
    {
        int tEnd = CBowData.GetInstance.m_bowItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, tEnd, CBowData.GetInstance.m_bowItemList[i].m_name, CBowData.GetInstance.m_bowItemList[i].m_cost);
        }
    }

    public void InsertMaceData()
    {
        int tEnd = CMaceData.GetInstance.m_maceItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i , tEnd, CMaceData.GetInstance.m_maceItemList[i].m_name, CMaceData.GetInstance.m_maceItemList[i].m_cost);            
        }        
    }

    public void InsertMartailArtsData()
    {
        int tEnd = CMartialArts.GetInstance.m_matialArtsItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i ,tEnd, CMartialArts.GetInstance.m_matialArtsItemList[i].m_name , CMartialArts.GetInstance.m_matialArtsItemList[i].m_cost);
        }
    }

    public void InsertSpearData()
    {
        int tEnd = CSpearData.GetInstance.m_spearItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, tEnd, CSpearData.GetInstance.m_spearItemList[i].m_name, CSpearData.GetInstance.m_spearItemList[i].m_cost);           
        }
    }

    public void InsertStaffData()
    {
        int tEnd = CStaffData.GetInstance.m_staffItemList.Count;
        for (int i = 0; i < tEnd;  i++)
        {
            ShowItemDataText(i, tEnd, CStaffData.GetInstance.m_staffItemList[i].m_name, CStaffData.GetInstance.m_staffItemList[i].m_cost);           
        }
    }
    public void InsertAccessoryData()
    {
        int tEnd = CAccessoryData.GetInstance.m_accessoryItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, tEnd, CAccessoryData.GetInstance.m_accessoryItemList[i].m_name, CAccessoryData.GetInstance.m_accessoryItemList[i].m_cost);
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
