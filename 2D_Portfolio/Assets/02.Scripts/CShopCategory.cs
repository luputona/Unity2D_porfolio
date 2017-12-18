using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CShopCategory : CSelectCategory// 임시로 싱글턴, 추후 다시 모노로 변경
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
    private Text m_categoryText;
    [SerializeField]
    private Button m_categoryBtn;
    [SerializeField]
    private CWeaponShop m_cWeaponShop;

    //public ESelcetCategory m_selectCategory = ESelcetCategory.Sword; // 현재 선택한 카테고리 선별

    public bool m_isCategoryBtnColor = true;
    public List<GameObject> m_categorySlotList = new List<GameObject>();
    public GameObject m_categoryslotPrefab;
    public CStaffData cStaffData;   

    void Awake()
    {
        m_shop_Catergory = GameObject.Find("Shop_CatergoryList").gameObject;
        m_shopcheck = this.GetComponent<CVillageManager>();
        m_cWeaponShop = this.GetComponent<CWeaponShop>();
    }

    protected override void Start()
    {        
        base.Start();
        PopupCategoryPanel();        
    }
      
    void PopupCategoryPanel()
    {
        //m_categoryCount = CWeaponData.GetInstance.m_categoryLocalList.Count;

        //카테고리 슬롯 생성
        for (int i = 0; i < m_categoryCount; i++)
        {
            GameObject tObj = Instantiate(m_categoryslotPrefab) as GameObject;
            m_categorySlotList.Add(tObj);
            m_categorySlotList[i].transform.SetParent(m_shop_Catergory.transform,false);
            m_categorySlotList[i].transform.name = CWeaponData.GetInstance.m_categoryLocalList[i].m_category; // TODO: 추후 m_categoryLocalList를 m_categoryList 로 변경
            m_categoryText = m_categorySlotList[i].transform.GetChild(0).GetComponent<Text>();
            m_categoryText.text = string.Format("{0}" ,CWeaponData.GetInstance.m_categoryLocalList[i].m_category);

            string tCatgoryName = m_categorySlotList[i].transform.name;
            
            tObj.gameObject.GetComponent<Button>().onClick.AddListener(() => OpenItemListInCategory(tCatgoryName));
        }
        //노말컬러 디폴트 색 정하는 코드
        //m_categoryBtn = m_categorySlotList[0].GetComponent<Button>();
        //ColorBlock cb = m_categoryBtn.colors;
        //cb.normalColor = new Color32(124, 124, 124, 255);
        //m_categoryBtn.colors = cb;        
    }
 
    void OpenItemListInCategory(string categoryName)
    {        
        Debug.Log("tECategory : " + categoryName);
        
        if(m_shopcheck.m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
        {
            WeaponCateogryList(categoryName);
        }
        else if(m_shopcheck.m_shopinfo == CSelectShop.ShopInfo.ItemShop)
        {
            ItemCategoryList(categoryName);
        }
    }

    void WeaponCateogryList(string categoryName)
    {
        m_shopcheck.m_shopDictionary[CSelectShop.ShopInfo.Category].SetActive(false);
        if (m_categorySlotList[(int)ESelcetWeaponCategory.Sword].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            m_cWeaponShop.InsertSwordItemData();
            Debug.Log("칼오픈");
        }
        else if (m_categorySlotList[(int)ESelcetWeaponCategory.Bow].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            m_cWeaponShop.InsertBowItemData();
            Debug.Log("활오픈");
        }
        else if (m_categorySlotList[(int)ESelcetWeaponCategory.Staff].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            Debug.Log("스태프오픈");
        }
        else if (m_categorySlotList[(int)ESelcetWeaponCategory.Accessory].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            Debug.Log("악세오픈");
        }
        else if (m_categorySlotList[(int)ESelcetWeaponCategory.Mace].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            Debug.Log("둔기오픈");
        }
        else if (m_categorySlotList[(int)ESelcetWeaponCategory.Spear].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            Debug.Log("창오픈");
        }
        else if (m_categorySlotList[(int)ESelcetWeaponCategory.MatialArts].transform.name.Equals(categoryName))
        {
            m_eCategory = ESelcetWeaponCategory.Disable;
            Debug.Log("근접오픈");
        }
    }

    void ItemCategoryList(string categoryName)
    {

    }

}
