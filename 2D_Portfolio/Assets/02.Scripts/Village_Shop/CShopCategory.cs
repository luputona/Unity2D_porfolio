using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CShopCategory : SingleTon<CShopCategory>//CSelectCategory// 임시로 싱글턴, 추후 다시 모노로 변경
{
    //public enum SelcetCategory
    //{
    //    Sword,
    //    Bow,
    //    Staff,
    //    Accessory,
    //    Mace,
    //    Spear,
    //    MatialArts,
    //    Disable = 98,
    //    Default = 99
    //}

    [SerializeField]
    private GameObject m_shop_Catergory;

    [SerializeField]
    private CVillageManager m_shopcheck;
    [SerializeField]
    private CWeaponShop m_cWeaponShop;
    [SerializeField]
    private CGoodsShop m_cGoodsShop;
    [SerializeField]
    private CEntryDungeon m_cEntryDungeon;
    [SerializeField]
    private CItemShopSlotListManager m_cItemShopManager;
    [SerializeField]
    private Text m_categoryText;
 

    public CSelectCategory.ESelcetWeaponCategory m_selectCategory = CSelectCategory.ESelcetWeaponCategory.Default; // 현재 선택한 카테고리 선별
    public CSelectCategory.ESelectGoodsShopCategory m_selectGoodsShopCategory = CSelectCategory.ESelectGoodsShopCategory.Default; // 현재 선택한 카테고리 선별
    public CSelectCategory.EBACKUISTATE m_eBackUiState = CSelectCategory.EBACKUISTATE.Default;
    //public CSelectShop.ShopInfo m_eShopInfo = CSelectShop.ShopInfo.;

    public int m_categoryCount;
    public bool m_isCategoryBtnColor = true;
    public List<GameObject> m_categorySlotList = new List<GameObject>();
    public GameObject m_categoryslotPrefab;
    public CStaffData cStaffData;   

    void Awake()
    {
        m_shop_Catergory = GameObject.Find("Shop_CatergoryList").gameObject;
        m_shopcheck = this.GetComponent<CVillageManager>();
        m_cWeaponShop = this.GetComponent<CWeaponShop>();
        m_cGoodsShop = this.GetComponent<CGoodsShop>();
        m_cItemShopManager = this.GetComponent<CItemShopSlotListManager>();
        m_cEntryDungeon = this.GetComponent<CEntryDungeon>();
    }

    void Start()
    {        
        PopupCategoryPanel();
    }

    void Update()
    {
        //Debug.Log("m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Sword].GetComponent<CSelectCategory>().m_eCategory " + m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Sword].GetComponent<CSelectCategory>().m_eCategory);

        //for(int i = 0; i < m_categoryCount; i++)
        //{
        //    Debug.Log("m_categorySlotList[i].GetComponent<CSelectCategory>().m_eCategory : " + m_categorySlotList[i].GetComponent<CSelectCategory>().m_eCategory);
        //}

    }

    void PopupCategoryPanel()
    {
        m_categoryCount = 10;
        //카테고리 슬롯 생성
        for (int i = 0; i < m_categoryCount; i++)
        {
            GameObject tObj = Instantiate(m_categoryslotPrefab);
            m_categorySlotList.Add(tObj);
            m_categorySlotList[i].transform.SetParent(m_shop_Catergory.transform,false);
            
            
            
            //enum state 변경
            //m_selectCategory = m_categorySlotList[i].GetComponent<CSelectCategory>().m_eCategory;
            //tObj.gameObject.GetComponent<Button>().onClick.AddListener(() => OpenItemListInCategory(m_selectCategory));

            //string 으로 체크
            string tName = m_categorySlotList[i].transform.name;
            //tObj.gameObject.GetComponent<Button>().onClick.AddListener(() => OpenItemListInCategoryStrVer(tName));

            //상점 리스트에서 광클릭하다가 카테고리로 돌아가는 이유 - > 빌리지매니저에 레이캐스팅이 ui뒤에 가려져 있던 상점체크 콜라이더를 인식해서 발생함
        }
        
        //노말컬러 디폴트 색 정하는 코드
        //m_categoryBtn = m_categorySlotList[0].GetComponent<Button>();
        //ColorBlock cb = m_categoryBtn.colors;
        //cb.normalColor = new Color32(124, 124, 124, 255);
        //m_categoryBtn.colors = cb;        

    }
 
    //enum으로 체크
    public void OpenItemListInCategory(CSelectCategory.ESelcetWeaponCategory tEWeaponSelect = CSelectCategory.ESelcetWeaponCategory.Default, CSelectCategory.ESelectGoodsShopCategory tEGoodsSelect = CSelectCategory.ESelectGoodsShopCategory.Default)
    {
        if(m_shopcheck.m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
        {            
            WeaponCateogryList(tEWeaponSelect);

            Debug.Log("샵구분");            
        }
        else if(m_shopcheck.m_shopinfo == CSelectShop.ShopInfo.GoodsShop)
        {
            GoodsShopCategoryList(tEGoodsSelect);
            Debug.Log("샵구분");
        }
    }

    public void ShowDungeonInfomation(int index)
    {
        m_cEntryDungeon.ShowDungeonInfo(index);
    }
    
    public void ChangeSlotObjNameIsWeaponShop()
    {
        for (int i = 0; i < CWeaponData.GetInstance.m_categoryLocalList.Count; i++)
        {
            string tempStr = CWeaponData.GetInstance.m_categoryLocalList[i].m_category;
            m_categorySlotList[i].SetActive(true);
            m_categorySlotList[i].transform.name = tempStr;              // TODO: 추후 m_categoryLocalList를 m_categoryList 로 변경
            m_categorySlotList[i].GetComponent<CSelectCategory>().InitializeWeaponShopCategory();
            m_categoryText = m_categorySlotList[i].transform.GetChild(0).GetComponent<Text>();
            m_categoryText.text = string.Format("{0}", tempStr);        
            
        }
    }
    
    public void ChangeSlotObjNameIsGoodsShop()
    {
        for (int i = 0; i < CGoodsShopData.GetInstance.m_localGoodsCategoryList.Count; i++)
        {
            string tempStr = CGoodsShopData.GetInstance.m_localGoodsCategoryList[i].m_category;
            m_categorySlotList[i].SetActive(true);
            m_categorySlotList[i].transform.name = tempStr;              // TODO: 추후 m_localGoodsCategoryList m_GoodsCategoryList 로 변경
            m_categorySlotList[i].GetComponent<CSelectCategory>().InitializeGoodsShopCategory();
            m_categoryText = m_categorySlotList[i].transform.GetChild(0).GetComponent<Text>();
            m_categoryText.text = string.Format("{0}", tempStr);        
            
        }
    }

    public void ChangeSlotObjNameIsDungeonEntry()
    {
        //TODO : 추후 유저가 클리어한 층 만큼만 생성하게 변경
        for(int i = 0; i < CDungeonData.GetInstance.m_dungeonList.Count; i++)
        {
            string tempStr = string.Format("제 {0} 층 {1}", CDungeonData.GetInstance.m_dungeonList[i].m_floor, CDungeonData.GetInstance.m_dungeonList[i].m_bossTitle);
            m_categorySlotList[i].SetActive(true);
            m_categorySlotList[i].transform.name = CDungeonData.GetInstance.m_dungeonList[i].m_floor.ToString();
            m_categorySlotList[i].GetComponent<CSelectCategory>().m_dungeonFloorIndex = CDungeonData.GetInstance.m_dungeonList[i].m_floor;
            m_categoryText = m_categorySlotList[i].transform.GetChild(0).GetComponent<Text>();
            m_categoryText.text = string.Format("{0}", tempStr);
            

        }
    }


    //enum으로 체크 
    void WeaponCateogryList(CSelectCategory.ESelcetWeaponCategory tEselect)
    {
        m_cWeaponShop.m_shopDictionary[CSelectShop.ShopInfo.ShopContenItemList].SetActive(true);

        m_selectCategory = tEselect;
        m_shopcheck.m_shopDictionary[CSelectShop.ShopInfo.Category].SetActive(false);
        if (CSelectCategory.ESelcetWeaponCategory.Sword == tEselect)
        {            
            m_cWeaponShop.InsertSwordItemData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            
            Debug.Log("칼오픈");
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Bow == tEselect)
        {
            m_cWeaponShop.InsertBowItemData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("활오픈");
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Staff == tEselect)
        {
            m_cWeaponShop.InsertStaffData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("스태프오픈");
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Accessory == tEselect)
        {
            m_cWeaponShop.InsertAccessoryData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("악세오픈");
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Mace == tEselect)
        {
            m_cWeaponShop.InsertMaceData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("둔기오픈");
        }
        else if (CSelectCategory.ESelcetWeaponCategory.Spear == tEselect)
        {
            m_cWeaponShop.InsertSpearData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("창오픈");
        }
        else if (CSelectCategory.ESelcetWeaponCategory.MatialArts == tEselect)
        {
            m_cWeaponShop.InsertMartailArtsData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("근접오픈");
        }
    }

    void GoodsShopCategoryList(CSelectCategory.ESelectGoodsShopCategory tESelect)
    {
        m_cWeaponShop.m_shopDictionary[CSelectShop.ShopInfo.ShopContenItemList].SetActive(true);

        m_selectGoodsShopCategory = tESelect;
        m_shopcheck.m_shopDictionary[CSelectShop.ShopInfo.Category].SetActive(false);
        if (CSelectCategory.ESelectGoodsShopCategory.Potion == tESelect)
        {
            m_cGoodsShop.InsertPotionItemData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;

            Debug.Log("포션오픈");
        }
        else if (CSelectCategory.ESelectGoodsShopCategory.Goods == tESelect)
        {
            //m_cWeaponShop.InsertBowItemData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("잡화오픈");
        }
        else if (CSelectCategory.ESelectGoodsShopCategory.ETC == tESelect)
        {
            //m_cWeaponShop.InsertStaffData();
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("기타오픈");
        }
    }
    
    public void SlotCount(int tCount)
    {
        if (tCount < m_categorySlotList.Count)
        {
            for (int i = tCount; i < m_categorySlotList.Count; i++)
            {                
                m_categorySlotList[i].SetActive(false);
            }
        }
    }


    








    //string으로 체크 
    void OpenItemListInCategoryStrVer(string tName)
    {
        if (m_shopcheck.m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
        {
            Debug.Log("tEselect : " + tName);

            WeaponCateogryListStrVer(tName);

        }
        else if (m_shopcheck.m_shopinfo == CSelectShop.ShopInfo.GoodsShop)
        {
            //ItemCategoryList(tEShopInfo);
        }
    }

    //string으로 체크 
    void WeaponCateogryListStrVer(string tName)
    {
        m_shopcheck.m_shopDictionary[CSelectShop.ShopInfo.Category].SetActive(false);
        if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Sword].transform.name == tName)
        {
            m_cWeaponShop.InsertSwordItemData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;

            Debug.Log("칼오픈");
        }
        else if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Bow].transform.name == tName)
        {
            m_cWeaponShop.InsertBowItemData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("활오픈");
        }
        else if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Staff].transform.name == tName)
        {
            m_cWeaponShop.InsertStaffData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("스태프오픈");
        }
        else if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Accessory].transform.name == tName)
        {
            m_cWeaponShop.InsertAccessoryData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("악세오픈");
        }
        else if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Mace].transform.name == tName)
        {
            m_cWeaponShop.InsertMaceData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("둔기오픈");
        }
        else if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.Spear].transform.name == tName)
        {
            m_cWeaponShop.InsertSpearData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("창오픈");
        }
        else if (m_categorySlotList[(int)CSelectCategory.ESelcetWeaponCategory.MatialArts].transform.name == tName)
        {
            m_cWeaponShop.InsertMartailArtsData();
            m_cItemShopManager.m_categoryName = tName;
            m_eBackUiState = CSelectCategory.EBACKUISTATE.Disable;
            Debug.Log("근접오픈");
        }
    }
}
