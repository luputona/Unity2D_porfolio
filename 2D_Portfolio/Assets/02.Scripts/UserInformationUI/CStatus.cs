using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

//마을에서만 이용  
// 스텟찍을때만 갱신 하는 클래스 
public class CStatus : SingleTon<CStatus>
{
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
    protected string m_id;
    [SerializeField]
    protected double m_damage;
    [SerializeField]
    protected double m_defence;
    [SerializeField]
    protected double m_dodge;
    [SerializeField]
    protected double m_hp;
    [SerializeField]
    protected double m_str;
    [SerializeField]
    protected double m_dex;

    public string ID
    {        
        get
        {
            return m_id;
        }     
    }
    public double Damage
    {
        get
        {
            return m_damage;
        }     
    }

    public double Defence
    {
        get
        {
            return m_defence;
        }       
    }
    public double Dodge
    {
        get
        {
            return m_dodge;
        }       
    }
    public double HP
    {
        get
        {
            return m_hp;
        }        
    }

    public double Strength
    {
        get
        {
            return m_str;
        }        
    }
    public double Dex
    {
        get
        {
            return m_dex;
        }       
    }

    void Awake()
    {
     
    }

    void Start()
    {
        
    }

    public void TEST()
    {
        Debug.Log("D : " + Dex);
    }
}
