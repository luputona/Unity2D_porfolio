using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CBuyItem : MonoBehaviour
{
    [SerializeField]
    private CGetItemInfomations m_cGetItemInfomations = null;

    //무기
    public int m_id = 0;
    public int m_cost = 0;
    public string m_desc = null;
    public string m_name = null;
    public string m_itemCode = null;
    public string m_category = null;

    //포션
    

    // Use this for initialization
    void Start()
    {
        InitializeInfomation();
        this.gameObject.GetComponent<Button>().onClick.AddListener(() => GetItemInfo());
        this.gameObject.GetComponent<Button>().onClick.AddListener(() => CItemShopSlotListManager.GetInstance.BuyItem(m_desc,m_cost, m_itemCode, m_category));
        
    }

    void InitializeInfomation()
    {
        m_cGetItemInfomations = gameObject.GetComponentInParent<CGetItemInfomations>();
       
    }

    void GetItemInfo()
    {
        m_id = m_cGetItemInfomations.m_id;
        m_cost = m_cGetItemInfomations.m_cost;
        m_desc = m_cGetItemInfomations.m_desc;
        m_name = m_cGetItemInfomations.m_name;
        m_itemCode = m_cGetItemInfomations.m_itemCode;
        m_category = m_cGetItemInfomations.m_category;
    }

    void ItemInfomation()
    {

        this.gameObject.transform.name = m_name;
    }
}
