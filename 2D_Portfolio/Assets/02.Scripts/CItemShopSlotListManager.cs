using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CItemShopSlotListManager : SingleTon<CItemShopSlotListManager>
{
    [SerializeField]
    private CWeaponShop m_cWeaponShop;
    [SerializeField]
    private CItemShop m_cItemShop;
    [SerializeField]
    private CVillageManager m_cVillageManager;
    [SerializeField]
    private CShopCategory m_cShopCategory;
    [SerializeField]
    private GameObject m_itemListScrollViewObj;
    
    
    [SerializeField]
    private int m_shopSlotCount;
    
    [SerializeField]
    private CSelectCategory.ESelcetWeaponCategory m_eItemCategory = CSelectCategory.ESelcetWeaponCategory.Default;


    public int tempCount;

    public GameObject m_itemList_Content;
    public GameObject shopSlotPrefab = null;
    public string m_categoryName = null;
    public string m_itemDescString = null;

    public List<GameObject> m_slots = new List<GameObject>();

    void Awake()
    {
        InitializeComponent();
        InitializeUI();
        m_cVillageManager.InitVillageManager();
        m_cWeaponShop.InitializeWeaponShop();
        m_cItemShop.InitializeItemShop();


    }

    // Use this for initialization
    void Start ()
    {
        m_shopSlotCount = 10;
        m_cVillageManager.InsertShopDictionary();
        m_cWeaponShop.InsertShopDictionary();
        m_cItemShop.InsertShopDictionary();

        CreatedShopListSlot();
        m_cVillageManager.InitRayCheckObj();

    }

  
    void InitializeUI()
    {
        m_itemListScrollViewObj = GameObject.Find("03_inst_ItemList_ScrollView").gameObject;
        m_itemList_Content = m_itemListScrollViewObj.transform.Find("ItemList_Viewport").transform.Find("ItemList_Content").gameObject;
    }

    void Update()
    {
        
    }

    void InitializeComponent()
    {
        m_cVillageManager = this.GetComponent<CVillageManager>();
        m_cWeaponShop = this.GetComponent<CWeaponShop>();
        m_cItemShop = this.GetComponent<CItemShop>();
        m_cShopCategory = this.GetComponent<CShopCategory>();
    }
    void CreatedShopListSlot()
    {
        //TODO : 상점 슬롯 임시 생성
        for (int i = 0; i < m_shopSlotCount; i++)
        {
            GameObject obj = Instantiate(shopSlotPrefab);
            m_slots.Add(obj);
            m_slots[i].transform.SetParent(m_itemList_Content.transform, false);

        }
    }


    public void HideSlotList(CSelectCategory.ESelcetWeaponCategory tItemCategory)
    {        
        m_eItemCategory = tItemCategory;
        if (CSelectCategory.ESelcetWeaponCategory.Sword == m_eItemCategory)
        {
            //TODO : 
            Debug.Log("검 아이템");
            
            for(int i = 0; i < CSwordData.GetInstance.m_swordItemList.Count; i++ )
            {
                m_slots[i].transform.name = CSwordData.GetInstance.m_swordItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CSwordData.GetInstance.m_swordItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CSwordData.GetInstance.m_swordItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CSwordData.GetInstance.m_swordItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CSwordData.GetInstance.m_swordItemList[i].m_description;
            }


            SlotCount(CSwordData.GetInstance.m_swordItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Bow == m_eItemCategory)
        {
            Debug.Log("활 아이템");


            for (int i = 0; i < CBowData.GetInstance.m_bowItemList.Count; i++)
            {
                m_slots[i].transform.name = CBowData.GetInstance.m_bowItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CBowData.GetInstance.m_bowItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CBowData.GetInstance.m_bowItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CBowData.GetInstance.m_bowItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CBowData.GetInstance.m_bowItemList[i].m_description;
            }


            SlotCount(CBowData.GetInstance.m_bowItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Staff == m_eItemCategory)
        {
            Debug.Log("스테프 아이템");

            for (int i = 0; i < CStaffData.GetInstance.m_staffItemList.Count; i++)
            {
                m_slots[i].transform.name = CStaffData.GetInstance.m_staffItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CStaffData.GetInstance.m_staffItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CStaffData.GetInstance.m_staffItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CStaffData.GetInstance.m_staffItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CStaffData.GetInstance.m_staffItemList[i].m_description;
            }

            SlotCount(CStaffData.GetInstance.m_staffItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Accessory == m_eItemCategory)
        {
            Debug.Log("악세 아이템");

            for (int i = 0; i < CAccessoryData.GetInstance.m_accessoryItemList.Count; i++)
            {
                m_slots[i].transform.name = CAccessoryData.GetInstance.m_accessoryItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CAccessoryData.GetInstance.m_accessoryItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CAccessoryData.GetInstance.m_accessoryItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CAccessoryData.GetInstance.m_accessoryItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CAccessoryData.GetInstance.m_accessoryItemList[i].m_description;
            }

            SlotCount(CAccessoryData.GetInstance.m_accessoryItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Mace == m_eItemCategory)
        {
            Debug.Log("둔기 아이템");

            for (int i = 0; i < CMaceData.GetInstance.m_maceItemList.Count; i++)
            {
                m_slots[i].transform.name = CMaceData.GetInstance.m_maceItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CMaceData.GetInstance.m_maceItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CMaceData.GetInstance.m_maceItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CMaceData.GetInstance.m_maceItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CMaceData.GetInstance.m_maceItemList[i].m_description;
            }

            SlotCount(CMaceData.GetInstance.m_maceItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Spear == m_eItemCategory)
        {
            Debug.Log("창 아이템");

            for (int i = 0; i < CSpearData.GetInstance.m_spearItemList.Count; i++)
            {
                m_slots[i].transform.name = CSpearData.GetInstance.m_spearItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CSpearData.GetInstance.m_spearItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CSpearData.GetInstance.m_spearItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CSpearData.GetInstance.m_spearItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CSpearData.GetInstance.m_spearItemList[i].m_description;
            }

            SlotCount(CSpearData.GetInstance.m_spearItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.MatialArts == m_eItemCategory)
        {
            Debug.Log("격투 아이템");

            for (int i = 0; i < CMartialArts.GetInstance.m_matialArtsItemList.Count; i++)
            {
                m_slots[i].transform.name = CMartialArts.GetInstance.m_matialArtsItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CMartialArts.GetInstance.m_matialArtsItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CMartialArts.GetInstance.m_matialArtsItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CMartialArts.GetInstance.m_matialArtsItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CMartialArts.GetInstance.m_matialArtsItemList[i].m_description;
            }

            SlotCount(CMartialArts.GetInstance.m_matialArtsItemList.Count);
        }
    }

    public void SetObejct(string tItemDesc)
    {
        m_cWeaponShop.m_itemDesc_Text.text = string.Format("{0}", tItemDesc);
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_id = ;
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_cost = ;
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_name = ;
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_disc = ;
    }

    public void BuyItem(string tItemDesc, int tCost)
    {

        m_cWeaponShop.m_itemDesc_Text.text = string.Format("매번 구매 해줘서 고마워 \n\n\n가격 : {0} \n아이템 이름 : {1} ",tCost, tItemDesc);

        //TODO : 플레이어 정보 제작 하면 소지금에서 구매액 빠지게 구현
    }

    


    public void ShowSlotList()
    {
        for(int i = 0; i < m_slots.Count; i++)
        {
            if(!m_slots[i].activeSelf)
            {
                m_slots[i].SetActive(true);
            }
        }
    }

    void SlotCount(int tCount)
    {
        if(tCount < m_slots.Count)
        {
            for (int i = tCount; i < m_slots.Count; i++)
            {
                m_slots[i].SetActive(false);
            }
        }
        
    }
        

}
