using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CInventoryManager : SingleTon<CInventoryManager>
{
    private static CInventoryManager Instance = null;

    public enum EINVENTORY_CATEGORY
    {
        Weapon,
        Potion,
        Goods,
        ETC,
        Default = 99
    }

    [SerializeField]
    private LoopScrollRect m_LoopScrollRect;

    public Queue<GameObject> m_slotQueue = new Queue<GameObject>();
    public List<GameObject> m_invenSlotList = new List<GameObject>();
    public Button m_backBtn;

    public GameObject m_inventory_Panel = null;
    //public GameObject m_inst_Item_List = null;

    //카테고리
    public EINVENTORY_CATEGORY m_eINVENTORY_CATEGORY = EINVENTORY_CATEGORY.Weapon;

    //무기 인벤
    //public List<WeaponInventory> m_weaponInventoryList = new List<WeaponInventory>();
    public int m_weaponInvenSize;

    //포션 인벤
    
    private void Awake()
    {
        if(Instance != null)
        {
            GameObject.Destroy(this);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }



        m_inventory_Panel = GameObject.Find("inst_Inventory_Panel").gameObject;
        //m_inst_Item_List = m_inventory_Panel.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;

        int tEndChild = m_inventory_Panel.transform.childCount;
        m_backBtn = m_inventory_Panel.transform.GetChild(tEndChild - 1).GetComponent<Button>();
        m_LoopScrollRect = m_inventory_Panel.transform.GetChild(0).GetComponent<LoopScrollRect>();
    }

    private void Start()
    {
        m_inventory_Panel.SetActive(false);
    }

    //아이템 구매하거나 겟 할때 호출 하게 해야함
    public void UpdateWeaponInventorySlot()
    {
        //무기 갱신 
        m_LoopScrollRect.totalCount = CUserData.GetInstance.m_weaponInvenList.Count; //인벤토리 슬롯 갯수 갱신
        m_LoopScrollRect.RefillCells(); // 인벤토리 생성함수 호출
        m_LoopScrollRect.RefreshCells();
    }

    public void UpdatePotionInventorySlot()
    {

    }


    public void InitInventory()
    {
        m_weaponInvenSize = CUserData.GetInstance.m_weaponInvenList.Count;
        m_LoopScrollRect.RefillCells(); // 인벤토리 생성함수 호출
        m_LoopScrollRect.RefreshCells();
    }

    public void UpdateAddInventory(string category, string itemCode)
    {
        CUserData.GetInstance.m_weaponInvenList.Add(new WeaponInventory(category, itemCode));
        //for(int i = 0; i < m_invenSize; i++)
        //{

        //}
    }

    public void ChangeCategory(EINVENTORY_CATEGORY eINVENTORY_CATEGORY)
    {
        if(EINVENTORY_CATEGORY.Weapon == eINVENTORY_CATEGORY)
        {
            UpdateWeaponInventorySlot();
            
            Debug.Log("무기 카테고리");
        }
        else if(EINVENTORY_CATEGORY.Potion == eINVENTORY_CATEGORY)
        {
            Debug.Log("포션 카테고리");
        }
        else if (EINVENTORY_CATEGORY.Goods == eINVENTORY_CATEGORY)
        {
            Debug.Log("잡화 카테고리");
        }
        else if (EINVENTORY_CATEGORY.ETC == eINVENTORY_CATEGORY)
        {
            Debug.Log("기타 카테고리");
        }

    }

    public void OpenInventoryUI()
    {
        //마을에서 인벤토리 오픈했을때 인벤 리스트 갱신
        //디폴트가 무기여서 무기로 지정
        m_inventory_Panel.SetActive(true);
        InitInventory();
        //m_LoopScrollRect.RefillCells(); // 인벤토리 생성함수 호출
    }

    public void DisableInventoryUI()
    {
        m_inventory_Panel.SetActive(false);
    }

    // Use this for initialization
    //   void Start ()
    //   {

    //	for(int i = 0; i < m_inst_Item_List.transform.childCount; i++)
    //       {            
    //           m_invenSlotList.Add(m_inst_Item_List.transform.GetChild(i).gameObject);
    //           m_invenSlotList[i].transform.name = string.Format("Slot_{0}", i);

    //       }
    //}

    //// Update is called once per frame
    //void Update ()
    //   {

    //       if(Input.GetKeyDown(KeyCode.Space))
    //       {

    //           for(int i = 0;  i < 10; i++)
    //           {
    //               m_inst_Item_List.transform.GetChild(0).SetSiblingIndex(m_inst_Item_List.transform.childCount + i);

    //           }           
    //       }
    //       if (Input.GetKeyDown(KeyCode.Z))
    //       {
    //           //큐에 넣기
    //           for (int i = 9; i >= 0 ; i--)
    //           {
    //               m_slotQueue.Enqueue(m_inst_Item_List.transform.GetChild(69 - i).gameObject);            
    //           }
    //           for (int i = 0; i < 10; i++)
    //           {               
    //               GameObject tIndex = m_slotQueue.Dequeue();
    //               Debug.Log("queue index : " + tIndex.transform.name);
    //               tIndex.transform.SetSiblingIndex(i);
    //           }
    //       }


    //       RectTransform t = m_inst_Item_List.GetComponent<RectTransform>();

    //       Debug.Log("local : " + t.rect.position);
    //       Debug.Log("global : " + t.localPosition);

    //   }
}
