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
        CInventoryManager.GetInstance.m_category = m_category;
        CInventoryManager.GetInstance.m_itemCode = m_itemCode;
        if (m_itemNameText != null)
        {
            CInventoryManager.GetInstance.m_invenIndex = index;
            if (CWeaponData.GetInstance.m_swordItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_swordItemDic[m_itemCode].m_name);
                
            }
            else if(CWeaponData.GetInstance.m_staffItemDic.ContainsKey(m_itemCode))
            {
                m_itemNameText.text = string.Format("slot Num:\n{0}\n\nitemCode:\n{1}\n\nitemName:\n<color='red'>{2}</color>", index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode, CWeaponData.GetInstance.m_staffItemDic[m_itemCode].m_name);
                
            }            
        }
    }

    void LoadPotionInvenData(int index)
    {
        string tCode = CUserData.GetInstance.m_potionInvenList[index].m_itemCode;
        if(m_itemNameText != null)
        {
            if(CUserData.GetInstance.m_potionInvenDic.ContainsKey(tCode))
            {
                m_itemNameText.text = string.Format("slot num : {0}\nitemcode:\n{1}\nname\n:<color='red'>{2}</color>\n수량 : {3}", index.ToString(), CUserData.GetInstance.m_potionInvenDic[tCode].m_itemCode,
                    CPotionData.GetInstance.m_potionItemList[index].m_name,CUserData.GetInstance.m_potionInvenDic[tCode].m_count);
            }
        }
    }

    

}
