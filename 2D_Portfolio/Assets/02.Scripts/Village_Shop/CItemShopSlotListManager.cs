using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CItemShopSlotListManager : SingleTon<CItemShopSlotListManager>
{
    [SerializeField]
    private CWeaponShop m_cWeaponShop;
    [SerializeField]
    private CGoodsShop m_cGoodsShop;
    [SerializeField]
    private CVillageManager m_cVillageManager;
    [SerializeField]
    private CShopCategory m_cShopCategory;
    [SerializeField]
    private CEntryDungeon m_cEntryDungeon;
    [SerializeField]
    private GameObject m_itemListScrollViewObj;
    
    
    [SerializeField]
    private int m_shopSlotCount;
    
    
    public CSelectCategory.ESelcetWeaponCategory m_eSelectWeaponCategory = CSelectCategory.ESelcetWeaponCategory.Default;
    public CSelectCategory.ESelectGoodsShopCategory m_eSelectGoodsShopCategory = CSelectCategory.ESelectGoodsShopCategory.Default;
    public CSelectCategory.ESelectDungeonCategory m_eSelectDungeonCategory = CSelectCategory.ESelectDungeonCategory.Default;


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
        m_cGoodsShop.InitializeItemShop();
        m_cEntryDungeon.InitializeEntryDungeon();


    }

    // Use this for initialization
    void Start ()
    {
        m_shopSlotCount = 10;
        m_cVillageManager.InsertShopDictionary();
        m_cWeaponShop.InsertShopDictionary();
        m_cGoodsShop.InsertShopDictionary();
        m_cEntryDungeon.InsertShopDictionary();

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
        m_cGoodsShop = this.GetComponent<CGoodsShop>();
        m_cShopCategory = this.GetComponent<CShopCategory>();
        m_cEntryDungeon = this.GetComponent<CEntryDungeon>();
    }
    void CreatedShopListSlot()
    {
        //TODO : 상점 슬롯 임시 생성
        for (int i = 0; i < m_shopSlotCount; i++)
        {
            GameObject obj = Instantiate(shopSlotPrefab);
            m_slots.Add(obj);
            m_slots[i].transform.SetParent(m_itemList_Content.transform, false);
            m_slots[i].transform.GetChild(4).gameObject.SetActive(false);

        }
    }

    public void SettingGoodsSlotListInfo(CSelectCategory.ESelectGoodsShopCategory tItemCategory)
    {
        m_eSelectGoodsShopCategory = tItemCategory;
        if(CSelectCategory.ESelectGoodsShopCategory.Potion == m_eSelectGoodsShopCategory)
        {
            Debug.Log("포션 아이템 호출");

            for (int i = 0; i < m_shopSlotCount; i++)
            {
                m_slots[i].transform.name = CPotionData.GetInstance.m_potionItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CPotionData.GetInstance.m_potionItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CPotionData.GetInstance.m_potionItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CPotionData.GetInstance.m_potionItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CPotionData.GetInstance.m_potionItemList[i].m_description;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CPotionData.GetInstance.m_potionItemList[i].m_itemCode;

                if (i <= m_shopSlotCount)
                {
                    //TODO : 잉여 슬롯 가려주기 
                    SlotCount(CPotionData.GetInstance.m_potionItemList.Count);
                }
                else
                {
                    //TODO : 생성되어있는 슬롯보다 데이터의 수가 많을 경우 DB 저글링
                    return;
                }
               
            }
        }
        else if(CSelectCategory.ESelectGoodsShopCategory.Goods == m_eSelectGoodsShopCategory)
        {
            Debug.Log("잡화 아이템 호출");
            //for (int i = 0; i < CSwordData.GetInstance.m_swordItemList.Count; i++)
            //{
            //    
            //    //m_slots[i].transform.name = CSwordData.GetInstance.m_swordItemList[i].m_name;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CSwordData.GetInstance.m_swordItemList[i].m_id;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CSwordData.GetInstance.m_swordItemList[i].m_cost;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CSwordData.GetInstance.m_swordItemList[i].m_name;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CSwordData.GetInstance.m_swordItemList[i].m_description;
            //}
        }
        else if(CSelectCategory.ESelectGoodsShopCategory.ETC == m_eSelectGoodsShopCategory)
        {
            Debug.Log("기타 아이템 호출");

            //for (int i = 0; i < CSwordData.GetInstance.m_swordItemList.Count; i++)
            //{
            //    //m_slots[i].transform.name = CSwordData.GetInstance.m_swordItemList[i].m_name;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CSwordData.GetInstance.m_swordItemList[i].m_id;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CSwordData.GetInstance.m_swordItemList[i].m_cost;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CSwordData.GetInstance.m_swordItemList[i].m_name;
            //    //m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CSwordData.GetInstance.m_swordItemList[i].m_description;
            //}
        }
    }

    //판매 목록의 정보들 표기해주는 함수
    public void SettingWeaponSlotListInfo(CSelectCategory.ESelcetWeaponCategory tItemCategory)
    {        
        m_eSelectWeaponCategory = tItemCategory;
        if (CSelectCategory.ESelcetWeaponCategory.Sword == m_eSelectWeaponCategory)
        {
            //TODO : 
            Debug.Log("검 아이템");
            
            for(int i = 0; i < CWeaponData.GetInstance.m_swordItemList.Count; i++ )
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_swordItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_swordItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_swordItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_swordItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_swordItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_swordItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_swordItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_swordItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[0].m_category;
            }

            //잉여 슬롯 false
            SlotCount(CWeaponData.GetInstance.m_swordItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Bow == m_eSelectWeaponCategory)
        {
            Debug.Log("활 아이템");


            for (int i = 0; i < CWeaponData.GetInstance.m_bowItemList.Count; i++)
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_bowItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_bowItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_bowItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_bowItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_bowItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_bowItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_bowItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_bowItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[1].m_category;
            }


            SlotCount(CWeaponData.GetInstance.m_bowItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Staff == m_eSelectWeaponCategory)
        {
            Debug.Log("스테프 아이템");

            for (int i = 0; i < CWeaponData.GetInstance.m_staffItemList.Count; i++)
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_staffItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_staffItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_staffItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_staffItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_staffItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_staffItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_staffItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_staffItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[2].m_category;
            }

            SlotCount(CWeaponData.GetInstance.m_staffItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Accessory == m_eSelectWeaponCategory)
        {
            Debug.Log("악세 아이템");

            for (int i = 0; i < CWeaponData.GetInstance.m_accessoryItemList.Count; i++)
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_accessoryItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_accessoryItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_accessoryItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_accessoryItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_accessoryItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_accessoryItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_accessoryItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_accessoryItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[3].m_category;
            }

            SlotCount(CWeaponData.GetInstance.m_accessoryItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Mace == m_eSelectWeaponCategory)
        {
            Debug.Log("둔기 아이템");

            for (int i = 0; i < CWeaponData.GetInstance.m_maceItemList.Count; i++)
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_maceItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_maceItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_maceItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_maceItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_maceItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_maceItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_maceItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_maceItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[4].m_category;
            }

            SlotCount(CWeaponData.GetInstance.m_maceItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Spear == m_eSelectWeaponCategory)
        {
            Debug.Log("창 아이템");

            for (int i = 0; i < CWeaponData.GetInstance.m_spearItemList.Count; i++)
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_spearItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_spearItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_spearItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_spearItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_spearItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_spearItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_spearItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_spearItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[5].m_category;
            }

            SlotCount(CWeaponData.GetInstance.m_spearItemList.Count);
        }
        else if (CSelectCategory.ESelcetWeaponCategory.MatialArts == m_eSelectWeaponCategory)
        {
            Debug.Log("격투 아이템");

            for (int i = 0; i < CWeaponData.GetInstance.m_martialArtsItemList.Count; i++)
            {
                m_slots[i].transform.name = CWeaponData.GetInstance.m_martialArtsItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_id = CWeaponData.GetInstance.m_martialArtsItemList[i].m_id;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_cost = CWeaponData.GetInstance.m_martialArtsItemList[i].m_cost;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_name = CWeaponData.GetInstance.m_martialArtsItemList[i].m_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_desc = CWeaponData.GetInstance.m_martialArtsItemList[i].m_description;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_name = CWeaponData.GetInstance.m_martialArtsItemList[i].m_skill_name;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_skill_desc = CWeaponData.GetInstance.m_martialArtsItemList[i].m_skill_Desc;

                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_itemCode = CWeaponData.GetInstance.m_martialArtsItemList[i].m_itemCode;
                m_slots[i].transform.GetChild(3).GetComponent<CGetItemInfomations>().m_category = CWeaponData.GetInstance.m_categoryLocalList[7].m_category;
            }

            SlotCount(CWeaponData.GetInstance.m_martialArtsItemList.Count);
        }
    }

    public void SettingDungeonSlotListInfo(CSelectCategory.ESelectDungeonCategory tESelectDungeonCategory)
    {
        m_eSelectDungeonCategory = tESelectDungeonCategory;

        if (CSelectCategory.ESelectDungeonCategory.Cave == m_eSelectDungeonCategory)
        {
            for(int i = 0; i < CDungeonData.GetInstance.m_dungeonList.Count; i++)
            {
                m_slots[i].transform.GetChild(4).gameObject.SetActive(true);
                m_slots[i].transform.name = CDungeonData.GetInstance.m_dungeonList[i].m_bossTitle;
                m_slots[i].transform.GetChild(4).GetComponent<CGetItemInfomations>().m_floor = CDungeonData.GetInstance.m_dungeonList[i].m_floor;
                m_slots[i].transform.GetChild(4).GetComponent<CGetItemInfomations>().m_bossName = CDungeonData.GetInstance.m_dungeonList[i].m_bossName;
                m_slots[i].transform.GetChild(4).GetComponent<CGetItemInfomations>().m_bossTitle = CDungeonData.GetInstance.m_dungeonList[i].m_bossTitle;
                m_slots[i].transform.GetChild(4).GetComponent<CGetItemInfomations>().m_clear = CDungeonData.GetInstance.m_dungeonList[i].m_clear;

                if (i <= m_shopSlotCount)
                {
                    //TODO : 잉여 슬롯 가려주기 
                    SlotCount(CDungeonData.GetInstance.m_dungeonList.Count);
                }
                else
                {
                    //TODO : 생성되어있는 슬롯보다 데이터의 수가 많을 경우 DB 저글링
                    return;
                }
            }

           
        }
        else if(CSelectCategory.ESelectDungeonCategory.Underworld == m_eSelectDungeonCategory)
        {
            Debug.Log("지하세계");
        }
        else if (CSelectCategory.ESelectDungeonCategory.Forest == m_eSelectDungeonCategory)
        {
            Debug.Log("숲");
        }
        else if (CSelectCategory.ESelectDungeonCategory.Sky == m_eSelectDungeonCategory)
        {
            Debug.Log("하늘구역");
        }
    }

    
    public void SetObejct(string tItemDesc, string skillName, string skillDesc, int floor)
    {
        if(m_cWeaponShop.m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
        {
            m_cWeaponShop.m_itemDesc_Text.text = string.Format("{0} \n\n스킬명\n{1}\n\n스킬 효과\n{2}", tItemDesc , skillName, skillDesc);
        }
        else if(m_cGoodsShop.m_shopinfo == CSelectShop.ShopInfo.GoodsShop)
        {
            m_cGoodsShop.m_itemDesc_Text.text = string.Format("{0}", tItemDesc);
        }       
        else if(m_cEntryDungeon.m_shopinfo == CSelectShop.ShopInfo.EntryDungeonDesk)
        {
            CShopCategory.GetInstance.ShowDungeonInfomation(floor);
        }

        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_id = ;
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_cost = ;
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_name = ;
        //m_slots[tIndex].GetComponent<CGetItemInfomations>().m_disc = ;
    }

    public void BuyItem(string tItemDesc, int tCost , string itemcode, string category)
    {
        if (m_cWeaponShop.m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
        {
            m_cWeaponShop.m_itemDesc_Text.text = string.Format("매번 구매 해줘서 고마워 \n\n\n가격 : {0} \n아이템 이름 : {1} ", tCost, tItemDesc);

            CUpdateUserInfo.GetInstance.m_gold -= tCost;

            CInventoryManager.GetInstance.UpdateAddInventory(category, itemcode);
            CUpdateUserInfo.GetInstance.GetWeaponInventoryToJson();
            CUploadUserData.GetInstance.UploadWeaponInventory();

        }
        else if (m_cGoodsShop.m_shopinfo == CSelectShop.ShopInfo.GoodsShop)
        {
            m_cGoodsShop.m_itemDesc_Text.text = string.Format("매번 구매 해줘서 고마워 \n\n\n가격 : {0} \n아이템 이름 : {1} ", tCost, tItemDesc);

            CUpdateUserInfo.GetInstance.m_gold -= tCost;

            CInventoryManager.GetInstance.UpdateAddPotionInventory(itemcode);
            CUpdateUserInfo.GetInstance.GetPotionInventoryToJson();
            CUploadUserData.GetInstance.UploadPotionInventory();


        }
       

        //TODO : 플레이어 정보 제작 하면 소지금에서 구매액 빠지게 구현
    }

    
    public void DisableDungeonSlot()
    {
        for(int i = 0; i < m_slots.Count; i++)
        {
            if(m_slots[i].transform.GetChild(4))
            {
                m_slots[i].transform.GetChild(4).gameObject.SetActive(false);
            }
        }
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
