using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.UI;

//모든 씬에서 유저 데이터 갱신 및 업데이트
public class CUpdateUserInfo : SingleTon<CUpdateUserInfo>
{
    private static CUpdateUserInfo Instance = null;

    //public List<>

    [SerializeField]
    private CUserInfoUIManager m_cUserInfoUIManager;

    public GameObject m_characterObject = null;
    public Animator m_charAnim = null;
    public Image m_characterMainImage = null;
    public Image m_weaponThumbnail = null;

    public string m_statusString;// { get; set; }
    public string m_name;// { get; set; }
    public int m_rank;// { get; set; }
    public string m_cur_Set_ItemCode;
    public int m_gold;
    public string m_weaponInvenString;
    public string m_potionInvenString;
    public string m_goodsInvenString;
    public string m_clearDungeonString;
    public int m_point;
    public int m_userCode;

    //스테이터스
    //공 방 회 피 힘 덱
    public double m_damage;
    public double m_defence;
    public double m_dodge;
    public double m_hp;
    public double m_str;
    public double m_dex;

    //무기 인벤토리
    public string m_category;
    public string m_itemCode;
    public int m_invenSize;
    

    void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(this);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }

        m_characterObject = GameObject.FindGameObjectWithTag("Character");
        m_charAnim = m_characterObject.GetComponent<Animator>();
        m_cUserInfoUIManager = GameObject.Find("VillageManager").GetComponent<CUserInfoUIManager>();
    }

    void Start()
    {
        InitUserInfo(CUserData.GetInstance.m_userDataList[0].m_point, CUserData.GetInstance.m_userDataList[0].m_userCode, 
            CUserData.GetInstance.m_userDataList[0].m_name, CUserData.GetInstance.m_userDataList[0].m_rank, 
            CUserData.GetInstance.m_userDataList[0].m_gold, CUserData.GetInstance.m_userDataList[0].m_cur_set_itemcode);
    }

    // Update is called once per frame
    void Update ()
    {
        SetWeaponToChangeCharacterObject();
        UpdateStatus();
    }

    public void InitUserInfo(int point, int userCode, string name, int rank, int gold, string cur_set_ItemCode)
    {
        //유저 정보 대입
        m_point = point;
        m_userCode = userCode;
        m_name = name;
        m_rank = rank;
        m_gold = gold;
        m_cur_Set_ItemCode = cur_set_ItemCode;
    }

    public void UpdateStatus()
    {
        // 연산이 완료된 스테이터스 대입 
        m_damage = CStatus.GetInstance.DefDamage;
        m_defence = CStatus.GetInstance.DefDefence;
        m_dodge = CStatus.GetInstance.DefDodge;
        m_hp = CStatus.GetInstance.DefHp;
        m_str = CStatus.GetInstance.DefStr;
        m_dex = CStatus.GetInstance.DefDex;
        
    }

   
    public void SetCurrentEquipWeapon()
    {
        //m_cUserInfoUIManager.ShowUserCurrentSettingWeapon();
        //m_cUserInfoUIManager.ShowUserCurrentSettingWeaponSkill();
        m_cur_Set_ItemCode = CInventoryManager.GetInstance.m_itemCode;
        CUploadUserData.GetInstance.UploadCurSetWeapon();
        Debug.Log(m_cur_Set_ItemCode);
    }

    //현재 착용한 무기 스탯을 계산쪽으로 넘겨주는 함수
    public void SetWeaponToChangeCharacterObject()
    {
        string tCurSetWeapon = m_cur_Set_ItemCode;



        // TODO : 무기의 키값에 따라서 캐릭터 외형 변경 및 스테이터스 갱신 , 유저정보창 일러 변경
        Debug.Log("Cupdateuserinfo 부분 애니메이터 보류");

        if(CResourceManager.GetInstance.m_characterAnimator.ContainsKey(tCurSetWeapon) == false)
        {
            m_charAnim.runtimeAnimatorController = null;
            return;
        }

        m_charAnim.runtimeAnimatorController = CResourceManager.GetInstance.GetAnimator(tCurSetWeapon);

        if (CWeaponData.GetInstance.m_swordItemDic.ContainsKey(tCurSetWeapon))
        {
            // 무기의 데미지를 넘김
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_swordItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef    = CWeaponData.GetInstance.m_swordItemDic[tCurSetWeapon].m_def;  //무기의 방어
            CStatus.GetInstance.SetWeaponDodge  = CWeaponData.GetInstance.m_swordItemDic[tCurSetWeapon].m_dodging; //무기의 회피
            CStatus.GetInstance.SetWeaponHP     = CWeaponData.GetInstance.m_swordItemDic[tCurSetWeapon].m_hp;
            
        }
        else if (CWeaponData.GetInstance.m_staffItemDic.ContainsKey(tCurSetWeapon))
        {
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_staffItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef    = CWeaponData.GetInstance.m_staffItemDic[tCurSetWeapon].m_def;
            CStatus.GetInstance.SetWeaponDodge  = CWeaponData.GetInstance.m_staffItemDic[tCurSetWeapon].m_dodging;
            CStatus.GetInstance.SetWeaponHP     = CWeaponData.GetInstance.m_staffItemDic[tCurSetWeapon].m_hp;

        }
        else if (CWeaponData.GetInstance.m_spearItemDic.ContainsKey(tCurSetWeapon))
        {
            //TODO : 
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_spearItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef = CWeaponData.GetInstance.m_spearItemDic[tCurSetWeapon].m_def;
            CStatus.GetInstance.SetWeaponDodge = CWeaponData.GetInstance.m_spearItemDic[tCurSetWeapon].m_dodging;
            CStatus.GetInstance.SetWeaponHP = CWeaponData.GetInstance.m_spearItemDic[tCurSetWeapon].m_hp;
        }
        else if (CWeaponData.GetInstance.m_martialItemDic.ContainsKey(tCurSetWeapon))
        {
            //TODO :   
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_martialItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef = CWeaponData.GetInstance.m_martialItemDic[tCurSetWeapon].m_def;
            CStatus.GetInstance.SetWeaponDodge = CWeaponData.GetInstance.m_martialItemDic[tCurSetWeapon].m_dodging;
            CStatus.GetInstance.SetWeaponHP = CWeaponData.GetInstance.m_martialItemDic[tCurSetWeapon].m_hp;
        }
        else if (CWeaponData.GetInstance.m_maceItemDic.ContainsKey(tCurSetWeapon))
        {
            //TODO : 
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_maceItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef = CWeaponData.GetInstance.m_maceItemDic[tCurSetWeapon].m_def;
            CStatus.GetInstance.SetWeaponDodge = CWeaponData.GetInstance.m_maceItemDic[tCurSetWeapon].m_dodging;
            CStatus.GetInstance.SetWeaponHP = CWeaponData.GetInstance.m_maceItemDic[tCurSetWeapon].m_hp;
        }
        else if (CWeaponData.GetInstance.m_bowItemDic.ContainsKey(tCurSetWeapon))
        {
            //TODO : 
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_bowItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef = CWeaponData.GetInstance.m_bowItemDic[tCurSetWeapon].m_def;
            CStatus.GetInstance.SetWeaponDodge = CWeaponData.GetInstance.m_bowItemDic[tCurSetWeapon].m_dodging;
            CStatus.GetInstance.SetWeaponHP = CWeaponData.GetInstance.m_bowItemDic[tCurSetWeapon].m_hp;
        }
        else if (CWeaponData.GetInstance.m_accessoryItemDic.ContainsKey(tCurSetWeapon))
        {
            //TODO : 
            CStatus.GetInstance.SetWeaponDamage = CWeaponData.GetInstance.m_accessoryItemDic[tCurSetWeapon].m_damage;
            CStatus.GetInstance.SetWeaponDef = CWeaponData.GetInstance.m_accessoryItemDic[tCurSetWeapon].m_def;
            CStatus.GetInstance.SetWeaponDodge = CWeaponData.GetInstance.m_accessoryItemDic[tCurSetWeapon].m_dodging;
            CStatus.GetInstance.SetWeaponHP = CWeaponData.GetInstance.m_accessoryItemDic[tCurSetWeapon].m_hp;
        }
    }

    public string GetStatusToJson()
    {
        //json 형태를 스트링으로 변환해서 업로드 할때 스트링으로 업로드

        UserStatus userStatus = new UserStatus(m_damage, m_defence, m_dodge, m_hp, m_str, m_dex, m_userCode);

        m_statusString = JsonMapper.ToJson(userStatus);

        return m_statusString;
    }

    public string GetWeaponInventoryToJson()
    {
        //TODO : 인벤토리 클래스 필요 
        
        //WeaponInventory weaponInventory = new WeaponInventory(m_category, m_itemCode);

        m_weaponInvenString = JsonMapper.ToJson(CUserData.GetInstance.m_weaponInvenList);

        Debug.Log(m_weaponInvenString);
        return m_weaponInvenString;
    }

    public string GetPotionInventoryToJson()
    {
        m_potionInvenString = JsonMapper.ToJson(CUserData.GetInstance.m_potionInvenList);

        Debug.Log(m_potionInvenString);
        return m_potionInvenString;
    }

    public string GetGoodsInventoryToJson()
    {
        //TODO : 인벤토리 클래스 필요 

        return null;
    }

    public string GetClearDungeonToJson()
    {
        //TODO :  전용 클래스 필요 
        return null;
    }

    

   
}
