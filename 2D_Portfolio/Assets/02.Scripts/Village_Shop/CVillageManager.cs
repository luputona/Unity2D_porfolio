﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CVillageManager : CSelectShop
{
    //딕셔너리키값을 enum 으로 선언하면 가비지가 생기므로 인터페이스 구현
    class ShopInfoComparer : IEqualityComparer<ShopInfo>
    {
        public bool Equals(ShopInfo x, ShopInfo y)
        {
            return x == y;
        }
        public int GetHashCode(ShopInfo obj)
        {
            return (int)obj;
        }
    }

    [SerializeField]
    private CWeaponShop m_cWeaponShop;
    [SerializeField]
    private CGoodsShop m_cGoodsShop;
    [SerializeField]
    protected CShopCategory m_cShopCategory;
    [SerializeField]
    private CEntryDungeon m_cEntryDungeon;
        
    [SerializeField]
    protected GameObject m_shopPanel;
    [SerializeField]
    protected GameObject[] m_shop;
    [SerializeField]
    protected int m_shopSlotCount;
    [SerializeField]
    private GameObject m_rayStateCheckObj;

    [SerializeField]
    private Text m_descText;

    public CSelectShop selectShop;

    //public List<GameObject> m_slots = new List<GameObject>();
    public Dictionary<ShopInfo, GameObject> m_shopDictionary = new Dictionary<ShopInfo, GameObject>(new ShopInfoComparer()); // 가비지를 없애려면 인터페이스로 만든 컴페어 클래스의 생성자를 넣어줘야함 

    public int m_childCount;
       
    //ryu
    //[SerializeField]
    //private GameObject m_weaponShopScrollViewObj;
    //[SerializeField]
    //protected GameObject m_itemList_Content;

    //void Awake()
    //{
        
    //}
    //void Start()
    //{
    //    //InsertShopDictionary();
    //    //CreatedShopListSlot();
    //}
    void Update()
    {
        TouchGetObj();
    }


    public void InitVillageManager()
    {
        selectShop = null;
        m_shopPanel = GameObject.Find("00_inst_Shop_Panel") as GameObject;

        //ryu
        //m_weaponShopScrollViewObj = GameObject.Find("03_inst_ItemList_ScrollView").gameObject;
        //m_itemList_Content = m_weaponShopScrollViewObj.transform.Find("ItemList_Viewport").transform.Find("ItemList_Content").gameObject;

        m_cGoodsShop = this.gameObject.GetComponent<CGoodsShop>();
        m_cWeaponShop = this.gameObject.GetComponent<CWeaponShop>();
        m_cShopCategory = this.gameObject.GetComponent<CShopCategory>();
        m_cEntryDungeon = this.gameObject.GetComponent<CEntryDungeon>();
        
        m_rayStateCheckObj = GameObject.FindGameObjectWithTag("RayCheck");
        m_childCount = m_shopPanel.transform.childCount;
        
    }

    public void InitRayCheckObj()
    {
        m_rayStateCheckObj.SetActive(false);
    }
    

    public virtual void InsertShopDictionary()
	{
		m_shopSlotCount = 10;
		m_shop = new GameObject[m_childCount];
		for (int i = 0; i < m_childCount; i++)
		{
			m_shop[i] = m_shopPanel.transform.GetChild(i).gameObject;

			m_shopDictionary.Add(ShopInfo.WeaponShop + i, m_shop[i].gameObject);

			m_shop[i].SetActive(false);
		}
		m_shopPanel.SetActive(false);


		//딕셔너리 내용물 확인
		var enumerator = m_shopDictionary.GetEnumerator();

		while(enumerator.MoveNext())
		{
			//Debug.Log("dic : " + enumerator.Current.Key);
		}
        
    }


    protected virtual void TouchGetObj()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), Vector2.zero);

                if (hit)
                {
                    if (hit.collider.gameObject.tag.Equals("RayCheck"))
                    {
                        Debug.Log("RayCheck");
                        return;
                    }
                    if (hit.collider.gameObject.tag.Equals("VillageShops"))
                    {
                        Debug.Log(hit.collider.gameObject.transform.name);
                        selectShop = hit.collider.gameObject.GetComponent<CSelectShop>();
                        m_shopinfo = selectShop.m_shopinfo;

                        OpenShop();
                        m_cEntryDungeon.OpenShop();
                        m_cGoodsShop.OpenShop();
                        m_cWeaponShop.OpenShop();
                    }
                    else
                    {
                        Debug.Log("Not found shops");
                    }
                }
                else
                {
                    selectShop = null;
                    Debug.Log("Not have collider");
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition ), Vector2.zero);
            
                        
            if (hit)
            {
                if(hit.collider.gameObject.tag.Equals("RayCheck"))
                {
                    //Debug.Log("RayCheck");
                    return;
                }
                if(hit.collider.gameObject.tag.Equals("VillageShops"))
                {
                    //Debug.Log(hit.collider.gameObject.transform.name);
                    selectShop = hit.collider.gameObject.GetComponent<CSelectShop>();
                    m_shopinfo = selectShop.m_shopinfo;

                    OpenShop();
                    m_cEntryDungeon.OpenShop();
                    m_cGoodsShop.OpenShop();
                    m_cWeaponShop.OpenShop();
                }
                else
                {
                    //Debug.Log("Not found shops");
                }
               
            }           
            else
            {
                selectShop = null;
                
                //Debug.Log("Not have collider");
            }
        }
    }

    protected virtual void OpenShop()
    {
        

        m_shopDictionary[ShopInfo.Category].SetActive(true);
        m_shopDictionary[ShopInfo.BackButton].SetActive(true);
        //m_shopDictionary[ShopInfo.ShopContenItemList].SetActive(true);
        m_shopDictionary[ShopInfo.ItemDescription].SetActive(true);
        m_rayStateCheckObj.SetActive(true);
        
        if (m_shopinfo == ShopInfo.GoodsShop)
        {
            m_cGoodsShop.m_shopinfo = m_shopinfo;

            m_shopPanel.SetActive(true);                       
            m_shopDictionary[ShopInfo.GoodsShop].SetActive(true);
        }
        if (m_shopinfo == ShopInfo.WeaponShop)
        {
            m_cWeaponShop.m_shopinfo = m_shopinfo;

            m_shopPanel.SetActive(true);                        
            m_shopDictionary[ShopInfo.WeaponShop].SetActive(true);
        }
        if (m_shopinfo == ShopInfo.EntryDungeonDesk)
        {
            m_cEntryDungeon.m_shopinfo = m_shopinfo;

            m_shopPanel.SetActive(true);
            m_shopDictionary[ShopInfo.EntryDungeonDesk].SetActive(true);            
        }        
    }

    public void ClosedShop()
    {
        m_rayStateCheckObj.SetActive(true);
        
        if (m_cShopCategory.m_eBackUiState == CSelectCategory.EBACKUISTATE.Disable)//현재 UI가 샵이고 카테고리 화면 일 경우 
        {
            m_shopDictionary[ShopInfo.Category].SetActive(true);
            CItemShopSlotListManager.GetInstance.ShowSlotList();

            m_shopDictionary[ShopInfo.ShopContenItemList].SetActive(false);

            m_cShopCategory.m_selectGoodsShopCategory = CSelectCategory.ESelectGoodsShopCategory.Default;
            m_cShopCategory.m_selectCategory = CSelectCategory.ESelcetWeaponCategory.Default;          


            //카테고리 선택중 일때 npc대사 
            if(m_shopinfo == CSelectShop.ShopInfo.WeaponShop)
            {
                m_cWeaponShop.WeaponShopMainText();
            }
            else if(m_shopinfo == CSelectShop.ShopInfo.GoodsShop)
            {
                m_cGoodsShop.GoodsShopMainText();
            }
            else if(m_shopinfo == CSelectShop.ShopInfo.EntryDungeonDesk)
            {
                //TODO : 던전은 아직 나갈대 대사 없음
                m_cEntryDungeon.ExitDungeonDeskText();
            }
            

            m_cShopCategory.m_eBackUiState = CSelectCategory.EBACKUISTATE.Closed;            
        }
        else if (m_cShopCategory.m_eBackUiState == CSelectCategory.EBACKUISTATE.Closed)
        {
            m_shopinfo = ShopInfo.Default;
            m_cWeaponShop.m_shopinfo = ShopInfo.Default;
            m_cGoodsShop.m_shopinfo = ShopInfo.Default;
            m_cEntryDungeon.m_shopinfo = ShopInfo.Default;
            ResetItemCategory();
            //CItemShopSlotListManager.GetInstance.m_itemList_Content.transform.a = new Vector2(0.0f, 0.0f);

            CItemShopSlotListManager.GetInstance.DisableDungeonSlot();
            for (int i = 0; i < m_childCount; i++)
            {
                m_shop[i].SetActive(false);
            }
            
            m_shopPanel.SetActive(false);
            m_rayStateCheckObj.SetActive(false);
        }
        
        m_shop[4].gameObject.GetComponent<ScrollRect>().content.anchoredPosition = Vector3.zero;
        m_shop[4].gameObject.GetComponent<ScrollRect>().StopMovement();
        m_shop[3].gameObject.GetComponent<ScrollRect>().content.anchoredPosition = Vector3.zero;
        m_shop[3].gameObject.GetComponent<ScrollRect>().StopMovement();

    }

    //각 상점의 카테고리 초기화
    void ResetItemCategory()
    {
        for(int i = 0; i < m_cShopCategory.m_categorySlotList.Count; i++)
        {
            CItemShopSlotListManager.GetInstance.m_eSelectDungeonCategory = CSelectCategory.ESelectDungeonCategory.Default;
            CItemShopSlotListManager.GetInstance.m_eSelectWeaponCategory = CSelectCategory.ESelcetWeaponCategory.Default;
            CItemShopSlotListManager.GetInstance.m_eSelectGoodsShopCategory = CSelectCategory.ESelectGoodsShopCategory.Default;

            m_cShopCategory.m_categorySlotList[i].GetComponent<CSelectCategory>().m_eCategory = CSelectCategory.ESelcetWeaponCategory.Default;
            m_cShopCategory.m_categorySlotList[i].GetComponent<CSelectCategory>().m_eItemShopCategory = CSelectCategory.ESelectGoodsShopCategory.Default;
            m_cShopCategory.m_categorySlotList[i].GetComponent<CSelectCategory>().m_eDungeonCategory = CSelectCategory.ESelectDungeonCategory.Default;
        }
        Debug.Log("스테이트 초기화 함수 호출");
    }

    //void CreatedShopListSlot()
    //{
    //    //TODO : 웨폰상점 슬롯 임시 생성
    //    for (int i = 0; i < m_shopSlotCount; i++)
    //    {
    //        m_slots.Add(Instantiate(shopSlotPrefab));
    //        m_slots[i].transform.SetParent(m_itemList_Content.transform, false);
    //    }
    //}


}
