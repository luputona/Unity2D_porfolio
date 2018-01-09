using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CInvenCategory : MonoBehaviour
{
    [SerializeField]
    private Button m_btn = null;

    void Awake()
    {
        m_btn = GetComponent<Button>();
               
    }

    CInventoryManager.EINVENTORY_CATEGORY m_eINVENTORY_CATEGORY = CInventoryManager.EINVENTORY_CATEGORY.Default;

    // Use this for initialization
    void Start ()
    {
        InitState();

        m_btn.onClick.AddListener(() => CInventoryManager.GetInstance.ChangeCategory(this.m_eINVENTORY_CATEGORY) );
        
    }
	
    void InitState()
    {
        if(this.gameObject.transform.name.Equals("inst_Inven_Weapon_Category_Button"))
        {
            this.m_eINVENTORY_CATEGORY = CInventoryManager.EINVENTORY_CATEGORY.Weapon;

        }
        else if(this.gameObject.transform.name.Equals("inst_Inven_Potion_Category_Button"))
        {
            this.m_eINVENTORY_CATEGORY = CInventoryManager.EINVENTORY_CATEGORY.Potion;
        }
        else if (this.gameObject.transform.name.Equals("inst_Inven_Goods_Category_Button"))
        {
            this.m_eINVENTORY_CATEGORY = CInventoryManager.EINVENTORY_CATEGORY.Goods;
        }
        else if (this.gameObject.transform.name.Equals("inst_Inven_Etc_Category_Button"))
        {
            this.m_eINVENTORY_CATEGORY = CInventoryManager.EINVENTORY_CATEGORY.ETC;
        }
        else
        {
            this.m_eINVENTORY_CATEGORY = CInventoryManager.EINVENTORY_CATEGORY.Default;
        }

    }
    

}
