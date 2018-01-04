using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CUserStatusUpButton : CStatus
{

	// Use this for initialization
	void Start ()
    {
        //this.GetComponent<Button>().onClick.AddListener(() => RecordStatus(this.m_eStatus));
        this.GetComponent<Button>().onClick.AddListener(() => this.GetStatusEnum()); 
    }
    void GetStatusEnum()
    {
        CStatus.GetInstance.m_eStatus = this.m_eStatus;
        CStatus.GetInstance.RecordStatus();
    }

    //void RecordStatus()
    //{
    //    if (ESTATUS.Damage == this.m_eStatus)
    //    {
    //        CStatus.GetInstance.m_damage += 1;
    //        CUpdateUserInfo.GetInstance.m_point -= 1;
    //    }
    //    else if (CStatus.ESTATUS.Defence == this.m_eStatus)
    //    {
    //        base.m_defence += 1;
    //        CUpdateUserInfo.GetInstance.m_point -= 1;
    //    }
    //    else if (CStatus.ESTATUS.Dodge == this.m_eStatus)
    //    {
    //        base.m_dodge += 1;
    //        CUpdateUserInfo.GetInstance.m_point -= 1;
    //    }
    //    else if (CStatus.ESTATUS.Hp == this.m_eStatus)
    //    {
    //        base.m_hp += 1;
    //        CUpdateUserInfo.GetInstance.m_point -= 1;
    //    }
    //    else if (CStatus.ESTATUS.Str == this.m_eStatus)
    //    {
    //        base.m_str += 1;
    //        CUpdateUserInfo.GetInstance.m_point -= 1;
    //    }
    //    else if (CStatus.ESTATUS.Dex == this.m_eStatus)
    //    {
    //        base.m_dex += 1;
    //        CUpdateUserInfo.GetInstance.m_point -= 1;
    //    }

    //}

}
