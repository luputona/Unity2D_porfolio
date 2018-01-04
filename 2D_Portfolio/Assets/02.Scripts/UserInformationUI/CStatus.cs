using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

//마을에서만 이용  
// 스텟찍을때만 갱신 하는 클래스 
public class CStatus : SingleTon<CStatus>
{
    private const double LimitedDodge = 75;

    public enum ESTATUS
    {
        Damage,
        Defence,
        Dodge,
        Hp,
        Str,
        Dex,
        Default = 99
    };
    private static CUpdateUserInfo Instance = null;
    public ESTATUS m_eStatus = ESTATUS.Default;

    [SerializeField]
    private string m_id;
    [SerializeField]
    private double m_defDamage;
    [SerializeField]
    private double m_defDefence;
    [SerializeField]
    private double m_defDodge;   
    [SerializeField]
    private double m_defHp;
    [SerializeField]
    private double m_defStr;
    [SerializeField]
    private double m_defDex;
    [SerializeField]
    private double m_weaponDamage;
    [SerializeField]
    private double m_weaponDodge;
    [SerializeField]
    private double m_weaponDef;
    [SerializeField]
    private double m_exceedDodge;
    [SerializeField]
    private double m_weaponHp;

    public double m_curhp;
    public double m_curDamage;
    public double m_curDefence;
    public double m_curDodge;
    public double m_curHp;
    public double m_curStr;
    public double m_curDex;

    public string ID
    {        
        get
        {
            return m_id;
        }     
    }
    public double DefDamage { get { return m_defDamage; } }
    public double DefDefence { get { return m_defDefence; } }
    public double DefDodge { get { return m_defDodge; } }
    public double DefStr { get { return m_defStr; } }
    public double DefDex { get { return m_defDex; } }

    public double DefHp
    {
        get
        {
            return m_defHp;
        }        
    }


    public double SetWeaponDamage
    {
        set
        {
            m_weaponDamage = value;
        }       
    }
    public double SetWeaponDef
    {
        set
        {
            m_weaponDef = value;
        }
    }
    
    public double SetWeaponDodge
    {
        set
        {
            m_weaponDodge = value;
        }
    }

    public double SetWeaponHP
    {
        set
        {
            m_weaponHp = value;
        }
    }   


    //TODO :  디폴트값과 현재값 다시 제작
    

    void Start()
    {
        
    }
    
    //DB에서 바로 받아오는 초기 셋팅값
    public void InitSetStatus(double damage, double def, double dodge, double hp, double str, double dex)
    {
        //공 방 회 피 힘 덱
        m_defDamage =  damage;
        m_defDefence =  def;
        m_defDodge =  dodge;
        m_defHp =  hp;
        m_defStr = str;
        m_defDex = dex;
        
        Debug.Log("???????");
    }

    public void RecordStatus()
    {
        if (ESTATUS.Hp == m_eStatus)
        {            
            if (CUpdateUserInfo.GetInstance.m_point >= 0)
            {
                m_defHp += 1;
                CUpdateUserInfo.GetInstance.m_point -= 1;                
            }
            else
            {
                return;
            }            
        }
        else if (ESTATUS.Str == m_eStatus)
        {            
            if (CUpdateUserInfo.GetInstance.m_point >= 0)
            {
                m_defStr += 1;
                CUpdateUserInfo.GetInstance.m_point -= 1;               
            }
            else
            {
                return;
            }
        }
        else if (ESTATUS.Dex == m_eStatus)
        {            
            if (CUpdateUserInfo.GetInstance.m_point >= 0)
            {
                m_defDex += 1;
                CUpdateUserInfo.GetInstance.m_point -= 1;                          
            }
            else
            {
                return;
            }
        }

        CalculateStatus();
        CUpdateUserInfo.GetInstance.UpdateStatus();
        CUploadUserData.GetInstance.UploadUserStatus();
    }

    //TODO : 스테이터스 UI 가 true가 되면 작동되게 해야함
    public void CalculateStatus()
    {
        CalculateDamage();
        CalculateDef();
        CalculateDodge();
        CalculateHp();
    }

    void CalculateDamage()
    {
        double tAdd = (m_weaponDamage + m_defStr) * 0.01;
        m_defDamage = (m_weaponDamage * 0.1 + m_defStr * 0.1 * tAdd) + m_weaponDamage;      
    }

    void CalculateDef()
    {
        // 증감치 =  회피 초과분 * 0.01 + 회피초과분 * 0.04 
        // 방어 = 기존방어(장비방어) + 증감치       
        double tAdd = (m_exceedDodge * 0.01) + (m_exceedDodge * 0.04);
        m_defDefence = m_weaponDef + tAdd;

    }
    void CalculateDodge()
    {
        double tAdd = (m_weaponDodge * 0.8 * (m_defDex * 0.2)) + m_defDex * 0.01;
        double tdodge = m_weaponDodge + tAdd;

        //민첩이 75보다 높은지 체크
        
        m_defDodge = tdodge;
        if (tdodge > LimitedDodge)
        {
            m_defDodge = LimitedDodge;
            m_exceedDodge = tdodge - LimitedDodge;
            CalculateDef();
        }        
    }
    void CalculateHp()
    {
        double tHp = m_weaponHp + m_defHp;
        m_curhp = tHp;
    }
}
