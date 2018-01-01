using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CGetItemInfomations : MonoBehaviour
{
    public int m_id = 0;
    public int m_cost = 0;
    public string m_desc = "";
    public string m_name = "";
    public string m_skill_desc = "";
    public string m_skill_name = "";


    //던전일경우
    public int m_floor = 0;
    public string m_bossName = "";
    public string m_bossTitle = "";
    public int m_clear = 0;


    // Use this for initialization
    void Start ()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(()=>CItemShopSlotListManager.GetInstance.SetObejct(m_desc, m_skill_name , m_skill_desc, m_floor));
	}

    void SetObject()
    {
//        this.gameObject.transform.name
    }
	
    void ItemInfomation()
    {
        
        this.gameObject.transform.name = m_name;
    }

}

/*

"id": 0,
        "name": "숏소드",
        "discription": "매우 짧은 검",
        "skill_name": "혼신의 일격",
        "skill_Dis": "강한 일격을 날림 \r\n 기본 공격력의 10%추가 데미지",
        "skill_effect_01": 0.1,
        "skill_effect_02": 0,
        "skill_effect_03": 0,
        "skill_effect_04": 0,
        "damage": 10,
        "def": 10,
        "dodging": 1,
        "hp": 10,
        "cost": 500  

*/
