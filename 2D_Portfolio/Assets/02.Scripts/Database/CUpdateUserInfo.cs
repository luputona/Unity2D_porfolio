using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.UI;

//모든 씬에서 유저 데이터 갱신 및 업데이트
public class CUpdateUserInfo : SingleTon<CUpdateUserInfo>
{
    private static CUpdateUserInfo Instance = null;

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
    }
    

    public GameObject m_characterObject = null;
    public Image m_characterMainImage = null;

    public string m_statusString;// { get; set; }
    public string m_name;// { get; set; }
    public int m_rank;// { get; set; }
    public string m_cur_Set_ItemCode;
    public int m_gold;
    public string m_weaponInven;
    public string m_goodsInven;
    public string m_clearDungeon;
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



    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

  

    void SetWeaponToChangeCharacterObject()
    {
        string tCurSetWeapon = CUserData.GetInstance.m_userDataList[0].cur_set_itemcode;

        // TODO : 무기의 키값에 따라서 캐릭터 외형 변경 및 스테이터스 갱신 , 유저정보창 일러 변경
        if (CSwordData.GetInstance.m_swordItemDic.ContainsKey(tCurSetWeapon))
        {
            
        }
        else if (CStaffData.GetInstance.m_staffItemDic.ContainsKey(tCurSetWeapon))
        {
            
        }
        else if (CSpearData.GetInstance.m_spearItemDic.ContainsKey(tCurSetWeapon))
        {
          
        }
        else if (CMartialArts.GetInstance.m_martialItemDic.ContainsKey(tCurSetWeapon))
        {
            
        }
        else if (CMaceData.GetInstance.m_maceItemDic.ContainsKey(tCurSetWeapon))
        {
          
        }
        else if (CBowData.GetInstance.m_bowItemDic.ContainsKey(tCurSetWeapon))
        {
           
        }
        else if (CAccessoryData.GetInstance.m_accessoryItemDic.ContainsKey(tCurSetWeapon))
        {
           
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
        return null;
    }

    public string GetGoodsInventoryToJson()
    {
        return null;
    }

    public string GetClearDungeonToJson()
    {
        return null;
    }

    

   
}
