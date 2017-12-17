using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTouchSpriteCheck : CSelectShop
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

    public CSelectShop selectShop;
    [SerializeField]
    protected GameObject m_shopPanel;
    [SerializeField]
    protected GameObject[] m_shop;
    

    
    public Dictionary<ShopInfo, GameObject> m_shopDictionary = new Dictionary<ShopInfo, GameObject>(new ShopInfoComparer()); // 가비지를 없애려면 인터페이스로 만든 컴페어 클래스의 생성자를 넣어줘야함 

    public int m_childCount;

    protected virtual void Awake()
    {
        selectShop = null;
        m_shopPanel = GameObject.Find("00_Shop_Panel") as GameObject;
        m_cWeaponShop = this.gameObject.GetComponent<CWeaponShop>();
               
        m_childCount = m_shopPanel.transform.childCount;
    }
    protected virtual void Start()
    {
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

    protected virtual void Update()
    {
        TouchGetObj();
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
                    Debug.Log(hit.collider.gameObject.transform.name);
                    selectShop = hit.collider.gameObject.GetComponent<CSelectShop>();
                    m_shopinfo = selectShop.m_shopinfo;

                    OpenShop();
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition ), Vector2.zero);

            if (hit)
            {
                Debug.Log(hit.collider.gameObject.transform.name);
                selectShop = hit.collider.gameObject.GetComponent<CSelectShop>();
                m_shopinfo = selectShop.m_shopinfo;

                OpenShop();
            }
        }
    }

    protected virtual void OpenShop()
    {
        m_shopDictionary[ShopInfo.Category].SetActive(true);
        if (m_shopinfo == ShopInfo.WeaponShop)
        {
            Debug.Log("웨폰샵");
            m_shopPanel.SetActive(true);
            m_shopDictionary[ShopInfo.WeaponShop].SetActive(true);

        }
        else if(m_shopinfo == ShopInfo.ItemShop)
        {
            Debug.Log("아이템샵");
            m_shopPanel.SetActive(true);
            m_shopDictionary[ShopInfo.ItemShop].SetActive(true);
        }
        else if(m_shopinfo == ShopInfo.EntryDungeon)
        {
            Debug.Log("엔트리 던전");

        }
    }
       
}
