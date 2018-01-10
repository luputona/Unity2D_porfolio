using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CInventoryManager : SingleTon<CInventoryManager>
{
    private static CInventoryManager Instance = null;
    [SerializeField]
    private const int InvenCategoryCount = 4;

    public enum EINVENTORY_CATEGORY
    {
        Weapon,
        Potion,
        Goods,
        ETC,
        Default = 99
    }

    public enum EBACKBUTTON
    {
        Disable,
        closed,
        Default = 99
    }



    [SerializeField]
    private LoopScrollRect m_LoopScrollRect = null;

    //사용안함
    //public Queue<GameObject> m_slotQueue = new Queue<GameObject>();
    //public List<GameObject> m_invenSlotList = new List<GameObject>();
    
    //public GameObject m_inst_Item_List = null;


    //최상위 판넬
    public GameObject m_inventory_Panel = null;
    //inst_Item_Info_Panel
    public GameObject m_Item_Info_Panel = null;

    //백 버튼
    public Button m_backBtn = null;

    //카테고리
    public EINVENTORY_CATEGORY m_eINVENTORY_CATEGORY = EINVENTORY_CATEGORY.Weapon;

    //인벤, 장비 정보 여닫기 체크
    public EBACKBUTTON m_eBackButtonCheck = EBACKBUTTON.Default;

    //장비 코드,인벤 인덱스 받아오는 변수
    public int m_invenIndex;
    public string m_category;
    public string m_itemCode;

    //카테고리 버튼 컬러 체크
    [SerializeField]
    private  List<Image> m_categoryBtnColor = new List<Image>();

    //백그라운드 레이캐스트 차단 콜라이더
    [SerializeField]
    private GameObject m_ray_State_check = null;

    //장비 인포
    [SerializeField]
    private Image m_weaponSprite = null; //웨폰 스프라이트
    [SerializeField]
    private Text m_weaponDesc = null; // 웨폰 설명 
    [SerializeField]
    private Text m_weaponName = null; //웨폰 이름
    [SerializeField]
    private Text m_weaponSkill = null; // 웨폰 스킬 
    [SerializeField]
    private Text m_weaponSkillDesc = null; //웨폰 스킬 설명
    //장비의 스탯
    [SerializeField]
    private Text[] m_weaponStatus = null;

    //포션인포
    [SerializeField]
    private Image m_potionSprite = null; //포션 이미지 스프라이트
    [SerializeField]
    private Text m_potionName = null;
    [SerializeField]
    private Text m_potionDesc = null;
    [SerializeField]
    private Text m_potionCount = null;
    //[SerializeField]
    //private Text m_potionCost = null;  
    [SerializeField]
    private Button m_potionInfo_Closed_Button = null;
    [SerializeField]
    private GameObject m_potion_info_BG = null;
    [SerializeField]
    private GameObject m_potion_info_Panel = null;

    private void Awake()
    {        
        //if(Instance != null)
        //{
        //    GameObject.Destroy(this);
        //}
        //else
        //{
        //    GameObject.DontDestroyOnLoad(gameObject);
        //}

        InitializeComponent();

    }

    private void Start()
    {
        int tCount = m_inventory_Panel.transform.childCount;
        for(int i = 0; i < tCount; i++)
        {
            m_inventory_Panel.transform.GetChild(i).gameObject.SetActive(false);
        }
        m_inventory_Panel.SetActive(false);
        m_ray_State_check.SetActive(false);

        this.m_categoryBtnColor[0].color = new Color(255, 255, 255, 255);
    }

    //아이템 구매하거나 겟 할때 호출 하게 해야함
    public void UpdateWeaponInventorySlot()
    {
        //무기 갱신 
        if (m_LoopScrollRect.totalCount < CUserData.GetInstance.m_weaponInvenList.Count || m_LoopScrollRect.totalCount > CUserData.GetInstance.m_weaponInvenList.Count)
        {
            m_LoopScrollRect.totalCount = CUserData.GetInstance.m_weaponInvenList.Count; //인벤토리 슬롯 갯수 갱신
        }

        m_LoopScrollRect.RefillCells(); // 인벤토리 생성함수 호출
        m_LoopScrollRect.RefreshCells();

    }

    //인벤토리에서 아이템의 정보 보여주는 창
    //장비 강화가 없을경우  : 있을경우에는 인벤토리 DB에 장비 변화하는 장비의 스테이터스를 추가하고 아이템 코드가 아닌  인덱스로 호출해야함 
    public void ShowItemInfo(string itemcode, string category)
    {
        m_itemCode = itemcode;
        if (EINVENTORY_CATEGORY.Weapon == m_eINVENTORY_CATEGORY)
        {
            m_Item_Info_Panel.SetActive(true);
            m_eBackButtonCheck = EBACKBUTTON.Disable;

            
            m_category = category;

            CUpdateUserInfo.GetInstance.SetWeaponToChangeCharacterObject();

            Debug.Log("인벤토리 매니저 웨폰 스프라이트 보류");
            //if(!CResourceManager.GetInstance.GetWeaponSprite(itemcode))
            //{
            //    return;
            //}
            //m_weaponSprite.overrideSprite = CResourceManager.GetInstance.GetWeaponSprite(itemcode);


            if (category.Equals("Sword"))
            {
                //스프라이트 매니저가 없어서 패스

                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_description);
                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_skill_Desc);
                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_swordItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_swordDefaultSkillDic[itemcode][3].m_skill_desc);
            }
            else if (category.Equals("Staff"))
            {
                //스프라이트 매니저가 없어서 패스
                //m_weaponSprite
                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_description);
                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_skill_Desc);
                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_staffItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_staffDefaultSkillDic[itemcode][3].m_skill_desc);
            }
            else if (category.Equals("Spear"))
            {
                //스프라이트 매니저가 없어서 패스
                //m_weaponSprite
                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_description);
                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_skill_Desc);
                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_spearItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_spearDefaultSkillDic[itemcode][3].m_skill_desc);
            }
            else if (category.Equals("Martial_arts"))
            {
                //스프라이트 매니저가 없어서 패스
                //m_weaponSprite
                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_description);

                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_skill_Desc);

                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_martialItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_martialDefaultSkillDic[itemcode][3].m_skill_desc);
            }
            else if (category.Equals("Mace"))
            {
                //스프라이트 매니저가 없어서 패스
                //m_weaponSprite
                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_description);
                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_skill_Desc);
                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_maceItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_maceDefaultSkillDic[itemcode][3].m_skill_desc);
            }
            else if (category.Equals("Bow"))
            {
                //스프라이트 매니저가 없어서 패스
                //m_weaponSprite
                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_description);
                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_skill_Desc);
                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_bowItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_bowDefaultSkillDic[itemcode][3].m_skill_desc);
            }
            else if (category.Equals("Accessory"))
            {
                //스프라이트 매니저가 없어서 패스
                //m_weaponSprite
                m_weaponName.text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_name);
                m_weaponDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_description);
                m_weaponSkillDesc.text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_skill_Desc);
                //4 : 공격력 , 5 : 방어력 , 6 : 회피 , 7 : hp
                m_weaponStatus[4].text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_damage);
                m_weaponStatus[5].text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_def);
                m_weaponStatus[6].text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_dodging);
                m_weaponStatus[7].text = string.Format("{0}", CWeaponData.GetInstance.m_accessoryItemDic[itemcode].m_hp);

                m_weaponSkill.text = string.Format("<color='red'>{0}</color>({1})\n{2}\n\n<color='red'>{3}</color>({4})\n{5}\n\n<color='red'>{6}</color>({7})\n{8}\n\n<color='red'>{9}</color>({10})\n{11}\n\n",
                    CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][0].m_skill_name, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][0].m_count, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][0].m_skill_desc,
                    CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][1].m_skill_name, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][1].m_count, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][1].m_skill_desc,
                    CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][2].m_skill_name, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][2].m_count, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][2].m_skill_desc,
                    CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][3].m_skill_name, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][3].m_count, CWeaponData.GetInstance.m_accessoryDefaultSkillDic[itemcode][3].m_skill_desc);
            }
        }
        else if(EINVENTORY_CATEGORY.Potion == m_eINVENTORY_CATEGORY)
        {
            Debug.Log("인벤 포션창 열기");
            m_potion_info_Panel.SetActive(true);

            m_potionName.text = string.Format("<color='red'>{0}</color>", CPotionData.GetInstance.m_potionItemList.Find(x => x.m_itemCode == itemcode).m_name);
            m_potionDesc.text = string.Format("{0}", CPotionData.GetInstance.m_potionItemList.Find(x => x.m_itemCode == itemcode).m_description);
            m_potionCount.text = string.Format("수량 : {0}", CUserData.GetInstance.m_potionInvenDic[itemcode].count);
            // TODO : 이미지 추후 추가
            //m_potionSprite.overrideSprite = 

        }
            
    }

    public void UpdatePotionInventorySlot()
    {
        if (m_LoopScrollRect.totalCount < CUserData.GetInstance.m_potionInvenDic.Count || m_LoopScrollRect.totalCount > CUserData.GetInstance.m_potionInvenDic.Count)
        {
            m_LoopScrollRect.totalCount = CUserData.GetInstance.m_potionInvenDic.Count; //인벤토리 슬롯 갯수 갱신
        }
        
        m_LoopScrollRect.RefillCells(); // 인벤토리 생성함수 호출
        m_LoopScrollRect.RefreshCells();

        
    }


    void InitInventory()
    {
        if(m_LoopScrollRect.totalCount < CUserData.GetInstance.m_weaponInvenList.Count || m_LoopScrollRect.totalCount > CUserData.GetInstance.m_weaponInvenList.Count)
        {
            m_LoopScrollRect.totalCount = CUserData.GetInstance.m_weaponInvenList.Count;            
        }
        InitColorChange();
        m_LoopScrollRect.RefillCells();
        // 인벤토리 생성함수 호출
    }

    public void UpdateAddInventory(string category, string itemCode)
    {
        CUserData.GetInstance.m_weaponInvenList.Add(new WeaponInventory(category, itemCode));
        
    }

    public void UpdateAddPotionInventory( string itemCode)
    {
        //if(CUserData.GetInstance.m_potionInvenDic.ContainsKey(itemCode))
        //{
        //    CUserData.GetInstance.m_potionInvenDic[itemCode].m_count += 1;
        //}
       
        for(int i = 0; i < CUserData.GetInstance.m_potionInvenList.Count; i++)
        {
            if(CUserData.GetInstance.m_potionInvenList[i].itemCode.Equals(itemCode))
            {
                CUserData.GetInstance.m_potionInvenList[i].count += 1;
            }            
        }
        if(!CUserData.GetInstance.m_potionInvenList.Exists(x => x.itemCode == itemCode))
        {
            CUserData.GetInstance.m_potionInvenList.Add(new PotionInventory(itemCode, 1));
        }


    }

    public void ChangeCategory(EINVENTORY_CATEGORY eINVENTORY_CATEGORY)
    {
        m_eINVENTORY_CATEGORY = eINVENTORY_CATEGORY;
        if (EINVENTORY_CATEGORY.Weapon == m_eINVENTORY_CATEGORY)
        {
            UpdateWeaponInventorySlot();
            Debug.Log("무기 카테고리");


            m_categoryBtnColor[0].color = new Color32(255, 255, 255, 255);

            m_categoryBtnColor[1].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[2].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[3].color = new Color32(135, 135, 135, 255);
        }
        else if(EINVENTORY_CATEGORY.Potion == m_eINVENTORY_CATEGORY)
        {
            UpdatePotionInventorySlot();


            m_categoryBtnColor[1].color = new Color32(255, 255, 255, 255);

            m_categoryBtnColor[0].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[2].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[3].color = new Color32(135, 135, 135, 255);
            Debug.Log("포션 카테고리");
        }
        else if (EINVENTORY_CATEGORY.Goods == m_eINVENTORY_CATEGORY)
        {
            m_categoryBtnColor[2].color = new Color32(255, 255, 255, 255);

            m_categoryBtnColor[0].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[1].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[3].color = new Color32(135, 135, 135, 255);
            Debug.Log("잡화 카테고리");
        }
        else if (EINVENTORY_CATEGORY.ETC == m_eINVENTORY_CATEGORY)
        {
            m_categoryBtnColor[3].color = new Color32(255, 255, 255, 255);

            m_categoryBtnColor[0].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[1].color = new Color32(135, 135, 135, 255);
            m_categoryBtnColor[2].color = new Color32(135, 135, 135, 255);
            Debug.Log("기타 카테고리");
        }
    }

    void InitColorChange()
    {
        m_categoryBtnColor[0].color = new Color32(255, 255, 255, 255);
        m_categoryBtnColor[1].color = new Color32(135, 135, 135, 255);
        m_categoryBtnColor[2].color = new Color32(135, 135, 135, 255);
        m_categoryBtnColor[3].color = new Color32(135, 135, 135, 255);
    }


    public void OpenInventoryUI()
    {
        //마을에서 인벤토리 오픈했을때 인벤 리스트 갱신
        //디폴트가 무기여서 무기로 지정
        m_inventory_Panel.SetActive(true);
        int tCount = m_inventory_Panel.transform.childCount;
        for (int i = 0; i < tCount; i++)
        {
            m_inventory_Panel.transform.GetChild(i).gameObject.SetActive(true);
        }

        m_Item_Info_Panel.SetActive(false);
        m_potion_info_Panel.SetActive(false);
        m_ray_State_check.SetActive(true);

        InitInventory();
        m_eBackButtonCheck = EBACKBUTTON.closed;
        //m_LoopScrollRect.RefillCells(); // 인벤토리 생성함수 호출
    }

    public void DisableInventoryUI()
    {
        if(EBACKBUTTON.Disable == m_eBackButtonCheck)
        {
            m_Item_Info_Panel.SetActive(false);
            //m_inventory_Panel.transform.GetChild(5).gameObject.SetActive(false);

            m_eBackButtonCheck = EBACKBUTTON.closed;
        }
        else if(EBACKBUTTON.closed == m_eBackButtonCheck)
        {            
            m_eINVENTORY_CATEGORY = EINVENTORY_CATEGORY.Weapon;
            m_ray_State_check.SetActive(false);

            int tCount = m_inventory_Panel.transform.childCount;
            for (int i = 0; i < tCount; i++)
            {
                m_inventory_Panel.transform.GetChild(i).gameObject.SetActive(false);
            }
            m_inventory_Panel.SetActive(false);
        }        
        else if(EBACKBUTTON.Default == m_eBackButtonCheck)
        {
            int tCount = m_inventory_Panel.transform.childCount;
            for (int i = 0; i < tCount; i++)
            {
                m_inventory_Panel.transform.GetChild(i).gameObject.SetActive(false);
            }
            m_inventory_Panel.SetActive(false);
        }
    }

    public void ClosedPotionInfo()
    {
        m_potion_info_Panel.SetActive(false);
    }


    void InitializeComponent()
    {
        m_inventory_Panel = GameObject.Find("inst_Inventory_Panel").gameObject;
        m_ray_State_check = GameObject.Find("inst_Raycast_State_Check");
        

        //m_inst_Item_List = m_inventory_Panel.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;

        int tEndChild = m_inventory_Panel.transform.childCount;
        m_backBtn = m_inventory_Panel.transform.GetChild(tEndChild - 2).GetComponent<Button>();

        for (int i = 0; i < InvenCategoryCount; i++)
        {
            m_categoryBtnColor.Add(m_inventory_Panel.transform.GetChild(1 + i).GetComponent<Image>());
        }

        m_LoopScrollRect = m_inventory_Panel.transform.GetChild(0).GetComponent<LoopScrollRect>();

        m_Item_Info_Panel = m_inventory_Panel.transform.GetChild(5).gameObject;
        m_Item_Info_Panel.SetActive(false); //아이템 정보 판넬 : ID: 5

        //웨폰 정보 UI모음
        m_weaponSprite = m_Item_Info_Panel.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        m_weaponName = m_Item_Info_Panel.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        m_weaponSkill = m_Item_Info_Panel.transform.GetChild(2).GetChild(0).GetComponent<Text>();
        m_weaponSkillDesc = m_Item_Info_Panel.transform.GetChild(3).GetChild(0).GetComponent<Text>();
        m_weaponDesc = m_Item_Info_Panel.transform.GetChild(4).GetChild(0).GetComponent<Text>();

        m_weaponStatus = new Text[m_Item_Info_Panel.transform.GetChild(5).childCount];
        for(int i = 0; i < m_weaponStatus.Length; i++)
        {
            m_weaponStatus[i] = m_Item_Info_Panel.transform.GetChild(5).GetChild(i).GetComponent<Text>();
        }

        //포션 정보 UI모음
        m_potion_info_Panel = m_inventory_Panel.transform.GetChild(tEndChild - 1).gameObject;
        m_potion_info_BG = m_inventory_Panel.transform.GetChild(tEndChild - 1).GetChild(0).gameObject;
        m_potionSprite = m_potion_info_BG.transform.GetChild(0).GetChild(0).GetComponent<Image>(); //포션 이미지
        m_potionName = m_potion_info_BG.transform.GetChild(1).GetChild(0).GetComponent<Text>();//포션이름
        m_potionDesc = m_potion_info_BG.transform.GetChild(2).GetChild(0).GetComponent<Text>();// 포션 설명
        m_potionCount = m_potion_info_BG.transform.GetChild(2).GetChild(1).GetComponent<Text>();// 포션 수량
        m_potionInfo_Closed_Button = m_potion_info_BG.transform.GetChild(m_potion_info_BG.transform.childCount - 1).GetComponent<Button>();

        m_potion_info_Panel.SetActive(false);
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
