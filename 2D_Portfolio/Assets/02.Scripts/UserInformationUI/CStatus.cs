using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public ESTATUS m_eStatus = ESTATUS.Default;
    
}
