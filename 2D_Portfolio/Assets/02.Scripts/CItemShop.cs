using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CItemShop : CVillageManager
{
    [SerializeField]
    private CItemShopSlotListManager m_cItemShopManager = null;
    [SerializeField]
    private CVillageManager m_touchSpriteCheck = null;
    [SerializeField]
    private Text m_itemName_Text = null;
    [SerializeField]
    private Text m_itemCost_Text = null;

    public Text m_itemDesc_Text = null;
    public GameObject m_shopItem = null;


    void Awake()
    {
        m_touchSpriteCheck = this.GetComponent<CVillageManager>();
    }

    void Start()
    {       
    }

    void Update()
    {
        TouchGetObj();
    }

    public void InitializeItemShop()
    {
        m_cItemShopManager = this.GetComponent<CItemShopSlotListManager>();

        base.InitVillageManager();

    }

    public override void InsertShopDictionary()
    {
        base.InsertShopDictionary();
    }    

    protected override void TouchGetObj()
    {
        base.TouchGetObj();
    }

    protected override void OpenShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.ItemShop)
        {
            Debug.Log("아이템샵");
            m_shopPanel.SetActive(true);
            m_shop[4].SetActive(true);
            m_shopDictionary[ShopInfo.ItemShop].SetActive(true);

            m_cShopCategory.m_eBackUiState = CSelectCategory.EBACKUISTATE.Closed;
        }
    }



    //임시 함수
    void OpenItemShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.ItemShop)
        {
            Debug.Log("아이템샵");
            m_shopPanel.SetActive(true);
            m_shop[4].SetActive(true);
            m_shopDictionary[ShopInfo.ItemShop].SetActive(true);
        }
    }


}
