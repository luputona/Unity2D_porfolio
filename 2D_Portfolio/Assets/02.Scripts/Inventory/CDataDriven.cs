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
        

    void Awake()
    {
        m_itemNameText = this.transform.GetChild(0).GetComponent<Text>();

    }
    // Use this for initialization
    void Start ()
    {


	}

    void LoadItemData(List<WeaponInventory> weaponInven, int index)
    {
        //m_itemNameText.text = string.Format("{0}",)
    }

    void LoadWeaponInvenData(int index)
    {
        if (m_itemNameText != null)
        {
            m_itemNameText.text = string.Format("{0}:{1}", index.ToString(), CUserData.GetInstance.m_weaponInvenList[index].m_itemCode);
        }

    }
    private void Update()
    {

        //Debug.Log(CUserData.GetInstance.m_weaponInvenList.Count);
        //for (int i = 0; i < CUserData.GetInstance.m_weaponInvenList.Count; i++)
        //{
        //    Debug.Log(CUserData.GetInstance.m_weaponInvenList[i].m_itemCode);
        //}

    }

}
