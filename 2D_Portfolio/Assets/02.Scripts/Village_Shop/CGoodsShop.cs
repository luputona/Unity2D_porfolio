using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CGoodsShop : CVillageManager
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
        //TouchGetObj();
    }

    public void InitializeItemShop()
    {
        m_cItemShopManager = this.GetComponent<CItemShopSlotListManager>();

        base.InitVillageManager();

    }

    public override void InsertShopDictionary()
    {
        base.InsertShopDictionary();
        m_itemDesc_Text = m_shopDictionary[ShopInfo.ItemDescription].gameObject.GetComponentInChildren<Text>();
    }    

    protected override void TouchGetObj()
    {        
        
    }

    protected override void OpenShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.GoodsShop)
        {
            Debug.Log("아이템샵");

            GoodsShopMainText();

            m_cShopCategory.m_eBackUiState = CSelectCategory.EBACKUISTATE.Closed;

            m_cShopCategory.ChangeSlotObjNameIsGoodsShop();
            m_cShopCategory.SlotCount(CGoodsShopData.GetInstance.m_localGoodsCategoryList.Count);
        }
    }

    public void ShowItemDataText(int tStart, string tName, int tCost)
    {
        m_itemName_Text = m_cItemShopManager.m_slots[tStart].transform.Find("ItemName_Text").GetComponent<Text>();
        m_itemCost_Text = m_cItemShopManager.m_slots[tStart].transform.Find("ItemCost_Text").GetComponent<Text>();
        
        m_itemName_Text.text = string.Format("{0}", tName);
        m_itemCost_Text.text = string.Format("{0}", tCost);

    }

    public void GoodsShopMainText()
    {
        //TODO : 추후 서버에 npc대사 모음으로 처리
        m_itemDesc_Text.text = string.Format("포션 츄라이 츄라이");
    }

    public void InsertPotionItemData()
    {
        int tEnd = 10;
        for (int i = 0; i < tEnd; i++)
        {
            ShowItemDataText(i, CPotionData.GetInstance.m_potionItemList[i].m_name, CPotionData.GetInstance.m_potionItemList[i].m_cost);
        }
    }


    //임시 함수
    void OpenItemShop()
    {
        //base.OpenShop();
        if (m_shopinfo == ShopInfo.GoodsShop)
        {
            Debug.Log("아이템샵");
            m_shopPanel.SetActive(true);
            m_shop[4].SetActive(true);
            m_shopDictionary[ShopInfo.GoodsShop].SetActive(true);
        }
    }


}
