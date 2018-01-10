using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CItemChangeBtn : MonoBehaviour
{
    [SerializeField]
    private Button m_btn;

	// Use this for initialization
	void Start ()
    {
        m_btn = this.GetComponent<Button>();
        m_btn.onClick.AddListener(() => OpenItemInfoPanel());
	}

    void OpenItemInfoPanel()
    {
        CInventoryManager.GetInstance.m_inventory_Panel.SetActive(true);
        //int tCount = CInventoryManager.GetInstance.m_inventory_Panel.transform.childCount;
        for(int i = 0; i  < 5; i++)
        {
            CInventoryManager.GetInstance.m_inventory_Panel.transform.GetChild(i).gameObject.SetActive(true);
        }
        CInventoryManager.GetInstance.UpdateWeaponInventorySlot();

        CInventoryManager.GetInstance.m_backBtn.gameObject.SetActive(true);
        //CInventoryManager.GetInstance.ShowItemInfo(CInventoryManager.GetInstance.m_itemCode, CInventoryManager.GetInstance.m_category);
        CInventoryManager.GetInstance.m_eBackButtonCheck = CInventoryManager.EBACKBUTTON.Default;
    }
	
}
