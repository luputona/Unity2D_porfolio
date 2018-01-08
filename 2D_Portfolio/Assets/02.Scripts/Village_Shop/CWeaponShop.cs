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
   
    
    void Awake()
    {
        m_touchSpriteCheck = this.GetComponent<CVillageManager>();
    }

    void Start()
    {        
    }

    void Update()
    {
        //TouchGetObj();        
    }

    public void InitializeWeaponShop()
    {
        m_cItemShopManager = this.GetComponent<CItemShopSlotListManager>();
        
        base.InitVillageManager();
        
    }

    public override void InsertShopDictionary()
    {
        base.InsertShopDictionary();
        m_itemDesc_Text = m_shopDictionary[ShopInfo.ItemDescription].gameObject.GetComponentInChildren<Text>();
    }

    protected override void TouchGetObj()
    {
        
    }

    protected override void OpenShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.WeaponShop)
        {
            Debug.Log("웨폰샵");

            WeaponShopMainText();

            m_cShopCategory.m_eBackUiState = CSelectCategory.EBACKUISTATE.Closed;
            
            m_cShopCategory.ChangeSlotObjNameIsWeaponShop();
            m_cShopCategory.SlotCount(CWeaponData.GetInstance.m_categoryLocalList.Count);
        }
    }

    public void WeaponShopMainText()
    {
        //TODO : 추후 서버에 npc대사 모음으로 처리
        m_itemDesc_Text.text = string.Format("아직도 그런 허접한 장비로 다닐 생각?");
    }

    public void ShowItemDataText(int tStart,  string tName, int tCost)
    {
        m_itemName_Text = m_cItemShopManager.m_slots[tStart].transform.Find("ItemName_Text").GetComponent<Text>();
        m_itemCost_Text = m_cItemShopManager.m_slots[tStart].transform.Find("ItemCost_Text").GetComponent<Text>();
        
        m_itemName_Text.text = string.Format("{0}", tName);
        m_itemCost_Text.text = string.Format("{0}", tCost);        
    }
    
    //생성된 슬롯 UI에 Json 데이터를 전달 
    //TODO : tEnd부분 추후 변경 해야함 , 10개 초과되면 에러남 UI굴리기로 처리
    public void InsertSwordItemData()
    {
        int tEnd = CWeaponData.GetInstance.m_swordItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, CWeaponData.GetInstance.m_swordItemList[i].m_name, CWeaponData.GetInstance.m_swordItemList[i].m_cost);       
        }
    }
    
    public void InsertBowItemData()
    {
        int tEnd = CWeaponData.GetInstance.m_bowItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, CWeaponData.GetInstance.m_bowItemList[i].m_name, CWeaponData.GetInstance.m_bowItemList[i].m_cost);
        }
    }

    public void InsertMaceData()
    {
        int tEnd = CWeaponData.GetInstance.m_maceItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i , CWeaponData.GetInstance.m_maceItemList[i].m_name, CWeaponData.GetInstance.m_maceItemList[i].m_cost);            
        }        
    }

    public void InsertMartailArtsData()
    {
        int tEnd = CWeaponData.GetInstance.m_matialArtsItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i , CWeaponData.GetInstance.m_matialArtsItemList[i].m_name , CWeaponData.GetInstance.m_matialArtsItemList[i].m_cost);
        }
    }

    public void InsertSpearData()
    {
        int tEnd = CWeaponData.GetInstance.m_spearItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, CWeaponData.GetInstance.m_spearItemList[i].m_name, CWeaponData.GetInstance.m_spearItemList[i].m_cost);           
        }
    }

    public void InsertStaffData()
    {
        int tEnd = CWeaponData.GetInstance.m_staffItemList.Count;
        for (int i = 0; i < tEnd;  i++)
        {
            ShowItemDataText(i, CWeaponData.GetInstance.m_staffItemList[i].m_name, CWeaponData.GetInstance.m_staffItemList[i].m_cost);           
        }
    }
    public void InsertAccessoryData()
    {
        int tEnd = CWeaponData.GetInstance.m_accessoryItemList.Count;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, CWeaponData.GetInstance.m_accessoryItemList[i].m_name, CWeaponData.GetInstance.m_accessoryItemList[i].m_cost);
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
