using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSelectCategory :MonoBehaviour
{
    public enum ESelcetCategory
    {
        Sword,
        Bow,
        Staff,
        Accessory,
        Mace,
        Spear,
        MatialArts,
        Disable = 98,
        Default = 99
    }

    private Button m_btnColor;

    public bool m_isColor = true;
    public ESelcetCategory m_eCategory = ESelcetCategory.Default;

    

    private void Start()
    {
        m_btnColor = this.GetComponent<Button>();

        for (int i = 0; i < CShopCategory.GetInstance.m_categoryCount; i++)
        {
            if (this.transform.name == "Sword")
            {
                m_eCategory = ESelcetCategory.Sword;
            }
            else if (this.transform.name == "Staff")
            {
                m_eCategory = ESelcetCategory.Staff;
            }
            else if (this.transform.name == "Spear")
            {
                m_eCategory = ESelcetCategory.Spear;
            }
            else if (this.transform.name == "Martial_arts")
            {
                m_eCategory = ESelcetCategory.MatialArts;
            }
            else if (this.transform.name == "Mace")
            {
                m_eCategory = ESelcetCategory.Mace;
            }
            else if (this.transform.name == "Bow")
            {
                m_eCategory = ESelcetCategory.Bow;
            }
            else if (this.transform.name == "Accessory")
            {
                m_eCategory = ESelcetCategory.Accessory;
            }
            else
            {
                m_eCategory = ESelcetCategory.Default;
            }
        }
        

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

        //m_btnColor.onClick.AddListener(() => CShopCategory.GetInstance.ButtonColorSetting());
        //m_btnColor.onClick.AddListener(() => ColorChange());
    }

    void ColorChange()
    {        
        //if (m_eCategory == ESelcetCategory.Sword)
        //{
        //    ColorBlock cb = m_btnColor.colors;
        //    cb.normalColor = new Color32(214, 214, 214, 255);
        //    m_btnColor.colors = cb;
        //}
        if(CShopCategory.GetInstance.m_isCategoryBtnColor == false && m_eCategory == ESelcetCategory.Sword)
        {
            ColorBlock cb = m_btnColor.colors;
            cb.normalColor = new Color32(214, 214, 214, 255);
            m_btnColor.colors = cb;
        }
       
    }


}
