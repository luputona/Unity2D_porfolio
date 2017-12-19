using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CItemShopManager : SingleTon<CItemShopManager>
{
    [SerializeField]
    private CWeaponShop m_cWeaponShop;
    [SerializeField]
    private CItemShop m_cItemShop;
    [SerializeField]
    private CVillageManager m_cVillageManager;
    [SerializeField]
    private GameObject m_weaponShopScrollViewObj;
    [SerializeField]
    private GameObject m_itemList_Content;
    [SerializeField]
    private int m_shopSlotCount;
    [SerializeField]
    private GameObject shopSlotPrefab;

    public List<GameObject> m_slots = new List<GameObject>();

    void Awake()
    {
        InitializeComponent();
        InitializeUI();
        m_cVillageManager.InitVillageManager();
        m_cWeaponShop.InitializeWeaponShop();
        
    }

    // Use this for initialization
    void Start ()
    {
        m_shopSlotCount = 10;
        m_cVillageManager.InsertShopDictionary();
        m_cWeaponShop.InsertShopDictionary();

        CreatedShopListSlot();
    }

    void InitializeUI()
    {
        m_weaponShopScrollViewObj = GameObject.Find("03_inst_ItemList_ScrollView").gameObject;
        m_itemList_Content = m_weaponShopScrollViewObj.transform.Find("ItemList_Viewport").transform.Find("ItemList_Content").gameObject;
    }

    void Update()
    {
        
    }

    void InitializeComponent()
    {
        m_cVillageManager = this.GetComponent<CVillageManager>();
        m_cWeaponShop = this.GetComponent<CWeaponShop>();
        m_cItemShop = this.GetComponent<CItemShop>();
    }
    void CreatedShopListSlot()
    {
        //TODO : 웨폰상점 슬롯 임시 생성
        for (int i = 0; i < m_shopSlotCount; i++)
        {
            m_slots.Add(Instantiate(shopSlotPrefab));
            m_slots[i].transform.SetParent(m_itemList_Content.transform, false);
        }
    }
}
