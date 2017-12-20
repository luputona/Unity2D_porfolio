using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CBuyItem : MonoBehaviour
{
    [SerializeField]
    private CGetItemInfomations m_cGetItemInfomations = null;

    public int m_id = 0;
    public int m_cost = 0;
    public string m_desc = null;
    public string m_name = null;
    // Use this for initialization
    void Start()
    {
        InitializeInfomation();
        this.gameObject.GetComponent<Button>().onClick.AddListener(() => GetItemInfo());
        this.gameObject.GetComponent<Button>().onClick.AddListener(() => CItemShopSlotListManager.GetInstance.BuyItem(m_desc,m_cost));
        
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
    }

    void ItemInfomation()
    {

        this.gameObject.transform.name = m_name;
    }
}
