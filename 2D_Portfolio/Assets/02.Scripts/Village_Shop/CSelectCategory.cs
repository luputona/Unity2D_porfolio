using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CSelectCategory :MonoBehaviour
{
    public enum EBACKUISTATE
    {
        Disable = 97,
        Closed = 98,
        Default = 99
    }
    public enum ESelcetWeaponCategory
    {
        Sword,
        Bow,
        Staff,
        Accessory,
        Mace,
        Spear,
        MatialArts,        
        Default = 99
    }

    public enum ESelectGoodsShopCategory
    {
        Potion,
        Goods,
        ETC,
        Default = 99            
    }

    

    private Button m_btnColor;

    public CVillageManager m_cVillageManager;
    
    public ESelcetWeaponCategory m_eCategory = ESelcetWeaponCategory.Default;
    public ESelectGoodsShopCategory m_eItemShopCategory = ESelectGoodsShopCategory.Default;

    public int m_dungeonFloorIndex = 0;
    public int m_categoryCount;

    public bool m_isColor = true;
    private void Awake()
    {
        m_cVillageManager = GameObject.Find("VillageManager").GetComponent<CVillageManager>();
    }

    void Start()
    {

        //if (m_eCategory == ESelcetCategory.Sword  && m_isColor)
        //{
        //    ColorBlock cb = m_btnColor.colors;
        //    cb.normalColor = new Color32(124, 124, 124, 255);
        //    m_btnColor.colors = cb;
        //}
        //else
        //{
        //    ColorBlock cb = m_btnColor.colors;
        //    cb.normalColor = new Color32(214, 214, 214, 255);
        //    m_btnColor.colors = cb;
        //}
        //SettingCategory();

        this.gameObject.GetComponent<Button>().onClick.AddListener(()=> CShopCategory.GetInstance.OpenItemListInCategory(m_eCategory, m_eItemShopCategory));
        this.gameObject.GetComponent<Button>().onClick.AddListener(()=> SettingCategory());
        

    }
    

    public void SettingCategory()
    {
        if (m_cVillageManager.m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
        {
            CItemShopSlotListManager.GetInstance.SettingWeaponSlotListInfo(m_eCategory);            
        }
        else if (m_cVillageManager.m_shopinfo == CSelectShop.ShopInfo.GoodsShop)
        {
            //샵 아이테 ㅁ목록의 이름,가격 설명 등 불러오는 함수
            Debug.Log("샵 아이테 ㅁ목록의 이름,가격 설명 등 불러오는 함수 호출");
            CItemShopSlotListManager.GetInstance.SettingGoodsSlotListInfo(m_eItemShopCategory);
            
        }
        else if(m_cVillageManager.m_shopinfo == CSelectShop.ShopInfo.EntryDungeonDesk)
        {
            //직접 접속이므로 desc를 바로 갱신 
            CShopCategory.GetInstance.ShowDungeonInfomation(m_dungeonFloorIndex);
        }
    }

    public void InitializeDungeonIndex()
    {
        for(int i = 0; i < CDungeonData.GetInstance.m_dungeonList.Count; i++)
        {
            m_dungeonFloorIndex = CDungeonData.GetInstance.m_dungeonList[i].m_floor;
        }
    }
    
    public void InitializeGoodsShopCategory()
    {
        m_categoryCount = 2;
        for (int i = 0; i < m_categoryCount; i++)
        {
            if (this.transform.name.Equals("Potion"))
            {
                m_eItemShopCategory = ESelectGoodsShopCategory.Potion;
            }
            else if (this.transform.name == "Goods")
            {
                m_eItemShopCategory = ESelectGoodsShopCategory.Goods;
            }
            else if (this.transform.name == "ETC")
            {
                m_eItemShopCategory = ESelectGoodsShopCategory.ETC;
            }
            //else if (this.transform.name == "Martial_arts")
            //{
            //    m_eCategory = ESelcetWeaponCategory.MatialArts;
            //}
            //else if (this.transform.name == "Mace")
            //{
            //    m_eCategory = ESelcetWeaponCategory.Mace;
            //}
            //else if (this.transform.name == "Bow")
            //{
            //    m_eCategory = ESelcetWeaponCategory.Bow;
            //}
            //else if (this.transform.name == "Accessory")
            //{
            //    m_eCategory = ESelcetWeaponCategory.Accessory;
            //}
            //else
            //{
            //    m_eCategory = ESelcetWeaponCategory.Default;
            //}
        }
    }
        
    public void InitializeWeaponShopCategory()
    {
        m_btnColor = this.GetComponent<Button>();
        m_categoryCount = CWeaponData.GetInstance.m_categoryLocalList.Count;
        for (int i = 0; i < m_categoryCount; i++)
        {
            if (this.transform.name.Equals("Sword") )
            {
                m_eCategory = ESelcetWeaponCategory.Sword;
            }
            else if (this.transform.name == "Staff")
            {
                m_eCategory = ESelcetWeaponCategory.Staff;
            }
            else if (this.transform.name == "Spear")
            {
                m_eCategory = ESelcetWeaponCategory.Spear;
            }
            else if (this.transform.name == "Martial_arts")
            {
                m_eCategory = ESelcetWeaponCategory.MatialArts;
            }
            else if (this.transform.name == "Mace")
            {
                m_eCategory = ESelcetWeaponCategory.Mace;
            }
            else if (this.transform.name == "Bow")
            {
                m_eCategory = ESelcetWeaponCategory.Bow;
            }
            else if (this.transform.name == "Accessory")
            {
                m_eCategory = ESelcetWeaponCategory.Accessory;
            }
            //else
            //{
            //    m_eCategory = ESelcetWeaponCategory.Default;
            //}
        }
    }





    //protected virtual void OpenItemListInCategory(ESelcetCategory eSelect)
    //{

    //}
    
    void ColorChange()
    {        
        //if (m_eCategory == ESelcetCategory.Sword)
        //{
        //    ColorBlock cb = m_btnColor.colors;
        //    cb.normalColor = new Color32(214, 214, 214, 255);
        //    m_btnColor.colors = cb;
        //}
        //if(CShopCategory.GetInstance.m_isCategoryBtnColor == false && m_eCategory == ESelcetCategory.Sword)
        //{
        //    ColorBlock cb = m_btnColor.colors;
        //    cb.normalColor = new Color32(214, 214, 214, 255);
        //    m_btnColor.colors = cb;
        //}
       
    }
    

}
