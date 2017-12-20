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
        Potion,
        Default = 99
    }

    public enum ESelectItemShopCategory
    {

    }

    private Button m_btnColor;

    public bool m_isColor = true;
    public ESelcetWeaponCategory m_eCategory = ESelcetWeaponCategory.Default;
    public int m_categoryCount;

    
    void Start()
    {
        InitializeCategory();
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

        this.gameObject.GetComponent<Button>().onClick.AddListener(()=>CItemShopSlotListManager.GetInstance.HideSlotList(m_eCategory));
        this.gameObject.GetComponent<Button>().onClick.AddListener(()=>CShopCategory.GetInstance.OpenItemListInCategory(m_eCategory));

    }

    void TEST()
    {
        Debug.Log("onbbtn : " + m_eCategory);
    }

    protected virtual void InitializeCategory()
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
