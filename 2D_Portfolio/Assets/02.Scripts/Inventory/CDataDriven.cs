using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CDataDriven : MonoBehaviour
{
    [SerializeField]
    private Text m_itemNameText;
    [SerializeField]
    private Image m_thumbnail_Image;
    [SerializeField]
    private Button m_openItemInfo_Btn = null;

    [SerializeField]
    private int m_itemIndex;
    [SerializeField]
    private string m_itemCode;
    [SerializeField]
    private string m_category;

    void Awake()
    {
        m_itemNameText = this.transform.GetChild(0).GetComponent<Text>();
        m_openItemInfo_Btn = this.GetComponent<Button>();
        m_thumbnail_Image = this.GetComponent<Image>();

    }
    // Use this for initialization
    void Start ()
    {
        m_openItemInfo_Btn.onClick.AddListener(() => CInventoryManager.GetInstance.ShowItemInfo(m_itemCode, m_category));

	}

    void GetItemInfo()
    {

    }


    void LoadItemData(List<WeaponInventory> weaponInven, int index)
    {
        //m_itemNameText.text = string.Format("{0}",)
    }

    void LoadWeaponInvenData(int index)
    {
        m_itemCode  = CUserData.GetInstance.m_weaponInvenList[index].m_itemCode;
        m_category = CUserData.GetInstance.m_weaponInvenList[index].m_category;
        
        if (m_itemNameText != null)
        {
            CInventoryManager.GetInstance.m_invenIndex = index;
            if (CWeaponData.GetInstance.m_swordItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_swordItemDic[m_itemCode].m_name);

            }
            else if(CWeaponData.GetInstance.m_staffItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_staffItemDic[m_itemCode].m_name);
               
            }
            else if (CWeaponData.GetInstance.m_spearItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_spearItemDic[m_itemCode].m_name);
                
            }
            else if (CWeaponData.GetInstance.m_martialItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_martialItemDic[m_itemCode].m_name);
               
            }
            else if (CWeaponData.GetInstance.m_maceItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_maceItemDic[m_itemCode].m_name);
             
            }
            else if (CWeaponData.GetInstance.m_bowItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_bowItemDic[m_itemCode].m_name);
             
            }
            else if (CWeaponData.GetInstance.m_accessoryItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", 
                    index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_accessoryItemDic[m_itemCode].m_name);
               
            }
        }
    }

    void LoadPotionInvenData(int index)
    {
        string tCode = CUserData.GetInstance.m_potionInvenList[index].itemCode;
        m_itemCode = tCode;
        if(m_itemNameText != null)
        {
            if(CUserData.GetInstance.m_potionInvenList.Exists(x => x.itemCode == tCode))
            {
                m_itemNameText.text = string.Format("slot num : {0}\nitemcode:\n{1}\nname\n:<color='red'>{2}</color>\n수량 : {3}", index.ToString(), 
                    CUserData.GetInstance.m_potionInvenList.Find(x => x.itemCode == m_itemCode).itemCode,
                    CPotionData.GetInstance.m_potionItemList[index].m_name , 
                    CUserData.GetInstance.m_potionInvenList.Find(x => x.itemCode == m_itemCode).count);
            }
        }
    }

    

}
