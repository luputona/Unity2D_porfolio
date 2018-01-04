using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CUserInfoUIManager : CStatus
{
    public enum EMAIN_UILIST
    {
        SetCharacterImage = 0,
        UserStatus,
        SetWeapon,
        SetSkill,
        ChangeNickNameBtn
    };


    [SerializeField]
    private const int m_statusUp_UICount = 6;
    [SerializeField]
    private const int m_defaultSkill_Count = 4;
    [SerializeField]
    private const int m_Set_Skill_TextCount = 8;


    public GameObject m_main_UserInformation_Panel = null;
    public GameObject m_Top_UserInformation_Panel = null;
    public GameObject m_Bottom_UserInfo_Panel = null;
    public List<GameObject> m_main_UserInformation_Panel_List = new List<GameObject>();
    public Image m_set_cur_main_CharaterImage = null;
    public Image m_set_cur_Weapon_Thumbnail = null;

    public Button[] m_statusUp_Button = new Button[m_statusUp_UICount];
    public Text[] m_statusUp_text = new Text[m_statusUp_UICount];
    public Text[] m_statusUp_Name_Text = new Text[m_statusUp_UICount];
    public Text[] m_cur_Set_Skill_Text = new Text[m_Set_Skill_TextCount];
    public Text[] m_cur_User_Elements_Info = new Text[3];
    public Text m_cur_WeaponName_Text = null;
    public Text m_cur_WeaponDesc_Text = null;

    //public ESTATUS m_eSTATUS = ESTATUS.Default;

    void Awake()
    {
        InitializeComponent();
        
    }
    // Use this for initialization
    void Start ()
    {
        //m_eStatus = ESTATUS.Default;
        

    }
    void Update()
    {
        //TODO : 임시 호출 , 타이틀 부분 완성되서 DB가 전 씬에서 먼저 불러와 지게 되면  초기화 함수쪽으로 이동 
        
        ShowUserCurrentSettingWeapon();
        ShowUserCurrentSettingWeaponSkill();
        ShowTopElementsUserInfo();
        ShowUserStatusInText();
        //base.SetStatus();
    }

   

    public void InitializeComponent()
    {
        m_Bottom_UserInfo_Panel = GameObject.Find("Bottom_MenuUI_Panel");
        m_main_UserInformation_Panel = GameObject.Find("inst_Main_UserInformation_Panel");
        m_Top_UserInformation_Panel = GameObject.Find("inst_Top_UserInformation_Panel");
        

        for (int i = 0; i < m_main_UserInformation_Panel.transform.childCount; i++)
        {
            m_main_UserInformation_Panel_List.Add(m_main_UserInformation_Panel.transform.GetChild(i).gameObject);
        }
        
        m_set_cur_main_CharaterImage = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.SetCharacterImage].GetComponent<Image>();
               
        for (int i = 0; i < m_statusUp_UICount; i++)
        {
            m_statusUp_Button[i] = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.UserStatus].transform.GetChild(i).GetComponent<Button>();
            m_statusUp_text[i] = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.UserStatus].transform.GetChild(m_statusUp_UICount + i).GetComponent<Text>();
            m_statusUp_Name_Text[i] = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.UserStatus].transform.GetChild(m_statusUp_UICount * 2 + i).GetComponent<Text>();
        }

        m_set_cur_Weapon_Thumbnail = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.SetWeapon].transform.GetChild(0).GetComponent<Image>();
        m_cur_WeaponName_Text = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.SetWeapon].transform.GetChild(1).GetComponent<Text>();
        m_cur_WeaponDesc_Text = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.SetWeapon].transform.GetChild(2).GetComponent<Text>();

        for(int i = 0; i < m_Set_Skill_TextCount; i++)
        {
            m_cur_Set_Skill_Text[i] = m_main_UserInformation_Panel_List[(int)EMAIN_UILIST.SetSkill].transform.GetChild(i).GetComponent<Text>();
        }

        for(int i = 0; i < 3; i++ )
        {
            m_cur_User_Elements_Info[i] = m_Top_UserInformation_Panel.transform.GetChild(i).GetChild(1).GetComponent<Text>();
        }


        m_main_UserInformation_Panel.SetActive(false);
    }

    public void ShowUserStatusInText()
    {
        //TODO : 
        m_statusUp_text[(int)ESTATUS.Damage].text = string.Format("{0}",  (int)CStatus.GetInstance.DefDamage);
        m_statusUp_text[(int)ESTATUS.Defence].text = string.Format("{0}", (int)CStatus.GetInstance.DefDefence);
        m_statusUp_text[(int)ESTATUS.Dodge].text = string.Format("{0}", (int)CStatus.GetInstance.DefDodge);
        m_statusUp_text[(int)ESTATUS.Hp].text = string.Format("{0}", (int)CStatus.GetInstance.m_curhp);
        m_statusUp_text[(int)ESTATUS.Str].text = string.Format("{0}", (int)CStatus.GetInstance.DefStr);
        m_statusUp_text[(int)ESTATUS.Dex].text = string.Format("{0}", (int)CStatus.GetInstance.DefDex);
        m_main_UserInformation_Panel_List[5].transform.GetChild(0).GetComponent<Text>().text = string.Format("{0}", CUpdateUserInfo.GetInstance.m_point );
                
    }

    public void ShowUserCurrentSettingWeapon()
    {
        string tCurSetWeapon = CUpdateUserInfo.GetInstance.m_cur_Set_ItemCode; // TODO : UpdateuserInfo 에서 받아오게 변경 
        string tShowWeaponInfoName = "";
        string tShowWeaponInfoSkillDesc = "";

        if (CSwordData.GetInstance.m_swordItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CSwordData.GetInstance.m_swordItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CSwordData.GetInstance.m_swordItemDic[tCurSetWeapon].m_skill_Desc;
        }
        else if(CStaffData.GetInstance.m_staffItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CStaffData.GetInstance.m_staffItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CStaffData.GetInstance.m_staffItemDic[tCurSetWeapon].m_skill_Desc;
        }
        else if(CSpearData.GetInstance.m_spearItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CSpearData.GetInstance.m_spearItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CSpearData.GetInstance.m_spearItemDic[tCurSetWeapon].m_skill_Desc;
        }
        else if (CMartialArts.GetInstance.m_martialItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CMartialArts.GetInstance.m_martialItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CMartialArts.GetInstance.m_martialItemDic[tCurSetWeapon].m_skill_Desc;
        }
        else if (CMaceData.GetInstance.m_maceItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CMaceData.GetInstance.m_maceItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CMaceData.GetInstance.m_maceItemDic[tCurSetWeapon].m_skill_Desc;
        }
        else if (CBowData.GetInstance.m_bowItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CBowData.GetInstance.m_bowItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CBowData.GetInstance.m_bowItemDic[tCurSetWeapon].m_skill_Desc;
        }
        else if (CAccessoryData.GetInstance.m_accessoryItemDic.ContainsKey(tCurSetWeapon))
        {
            tShowWeaponInfoName = CAccessoryData.GetInstance.m_accessoryItemDic[tCurSetWeapon].m_name;
            tShowWeaponInfoSkillDesc = CAccessoryData.GetInstance.m_accessoryItemDic[tCurSetWeapon].m_skill_Desc;
        }
        m_cur_WeaponName_Text.text = string.Format("{0}", tShowWeaponInfoName);
        m_cur_WeaponDesc_Text.text = string.Format("{0}", tShowWeaponInfoSkillDesc);
    }

    public void ShowUserCurrentSettingWeaponSkill()
    {
        string tCurSetWeapon = CUpdateUserInfo.GetInstance.m_cur_Set_ItemCode;
    
        if (CSwordData.GetInstance.m_swordDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CSwordData.GetInstance.m_swordDefaultSkillDic[tCurSetWeapon][i].m_skill_name);               
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CSwordData.GetInstance.m_swordDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);                
            }
        }
        else if (CStaffData.GetInstance.m_staffDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {               
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CStaffData.GetInstance.m_staffDefaultSkillDic[tCurSetWeapon][i].m_skill_name);
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CStaffData.GetInstance.m_staffDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);                
            }
        }
        else if (CSpearData.GetInstance.m_spearDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {               
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CSpearData.GetInstance.m_spearDefaultSkillDic[tCurSetWeapon][i].m_skill_name);                
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CSpearData.GetInstance.m_spearDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);                
            }
        }
        else if (CMartialArts.GetInstance.m_martialDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {                
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CMartialArts.GetInstance.m_martialDefaultSkillDic[tCurSetWeapon][i].m_skill_name);                
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CMartialArts.GetInstance.m_martialDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);                
            }
        }
        else if (CMaceData.GetInstance.m_maceDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {                
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CMaceData.GetInstance.m_maceDefaultSkillDic[tCurSetWeapon][i].m_skill_name);              
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CMaceData.GetInstance.m_maceDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);                
            }
        }
        else if (CBowData.GetInstance.m_bowDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CBowData.GetInstance.m_bowDefaultSkillDic[tCurSetWeapon][i].m_skill_name);               
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CBowData.GetInstance.m_bowDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);                
            }
        }
        else if (CAccessoryData.GetInstance.m_accessoryDefaultSkillDic.ContainsKey(tCurSetWeapon))
        {
            for (int i = 0; i < m_defaultSkill_Count; i++)
            {
                m_cur_Set_Skill_Text[i].text = string.Format("{0}", CAccessoryData.GetInstance.m_accessoryDefaultSkillDic[tCurSetWeapon][i].m_skill_name);                
                m_cur_Set_Skill_Text[m_defaultSkill_Count+i].text = string.Format("{0}", CAccessoryData.GetInstance.m_accessoryDefaultSkillDic[tCurSetWeapon][i].m_skill_desc);              
            }
        }
    }

    //화면 상단에 있는 유저 정ㅇ보
    public void ShowTopElementsUserInfo()
    {
        m_cur_User_Elements_Info[0].text = string.Format("{0}", CUpdateUserInfo.GetInstance.m_name);
        m_cur_User_Elements_Info[1].text = string.Format("{0}", CUpdateUserInfo.GetInstance.m_rank );
        m_cur_User_Elements_Info[2].text = string.Format("{0}", CUpdateUserInfo.GetInstance.m_gold);
    }
    
    public void OpenUserInfomation()
    {        
        CStatus.GetInstance.CalculateStatus(); 

        m_main_UserInformation_Panel.SetActive(true);
    }
    

}
