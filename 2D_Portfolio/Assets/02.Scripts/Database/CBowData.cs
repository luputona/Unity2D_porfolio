using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;


public class CBowData : SingleTon<CBowData>, IItemData
{
    [SerializeField]
    private string m_serverUrl;
    [SerializeField]
    private JsonData m_bowJsonData;
    
    public void Awake()
    {
        StartCoroutine(LoadData());
    }
    public void Start()
    {
    }

    public void ConstructData()
    {
        for (int i = 0; i < m_bowJsonData.Count; i++)
        {
            CWeaponData.GetInstance.m_bowItemList.Add(new BowItem(
                (int)m_bowJsonData[i]["id"],
                m_bowJsonData[i]["name"].ToString(),
                m_bowJsonData[i]["description"].ToString(),
                m_bowJsonData[i]["skill_name"].ToString(),
                m_bowJsonData[i]["skill_Desc"].ToString(),
                double.Parse(m_bowJsonData[i]["skill_effect_01"].ToString()),
                double.Parse(m_bowJsonData[i]["skill_effect_02"].ToString()),
                double.Parse(m_bowJsonData[i]["skill_effect_03"].ToString()),
                double.Parse(m_bowJsonData[i]["skill_effect_04"].ToString()),
                m_bowJsonData[i]["default_skill"].ToString(),
                double.Parse(m_bowJsonData[i]["damage"].ToString()),
                double.Parse(m_bowJsonData[i]["def"].ToString()),
                double.Parse(m_bowJsonData[i]["dodging"].ToString()),
                double.Parse(m_bowJsonData[i]["hp"].ToString()),
                (int)m_bowJsonData[i]["cost"],
                m_bowJsonData[i]["code"].ToString()));
        }
    }

    public void DefaultSkillToJson()
    {
        for (int i = 0; i < CWeaponData.GetInstance.m_bowItemList.Count; i++)
        {
            CWeaponData.GetInstance.m_bowDefaultSkillDic.Add(CWeaponData.GetInstance.m_bowItemList[i].m_itemCode, new Dictionary<int, DefaultBowSkill>());

            JsonData tData = JsonMapper.ToObject(CWeaponData.GetInstance.m_bowItemList[i].m_default_skill);
            //Debug.Log(" : " + m_swordItemList[i].m_default_skill);

            for (int j = 0; j < tData.Count; j++)
            {
                CWeaponData.GetInstance.m_bowDefaultSkillDic[CWeaponData.GetInstance.m_bowItemList[i].m_itemCode].Add(j, 
                    new DefaultBowSkill((int)tData[j]["id"], tData[j]["skill_name"].ToString(), tData[j]["skill_desc"].ToString(), tData[j]["skill_effect"].ToString(), (int)tData[j]["count"]));
            }
        }
        //Debug.Log(m_bowDefaultSkillDic["w010001"][0].m_skill_name);
    }

    public IEnumerator LoadData()
    {
        WWW www = new WWW(m_serverUrl);

        yield return www;

        //byte[] bytes = Encoding.Default.GetBytes(serverDB);
        string serverDB = Encoding.UTF8.GetString(www.bytes);

        m_bowJsonData = JsonMapper.ToObject(serverDB);

        if (www.isDone)
        {
            ConstructData();
            DefaultSkillToJson();
        }
    }
    public void LoadLocalData()
    {
    }
    public void ConstructLocalData()
    {
    }

}

[System.Serializable]
public class DefaultBowSkill
{
    public int m_id;
    public string m_skill_name;
    public string m_skill_desc;
    public string m_skill_effect;
    public int m_count;

    public DefaultBowSkill(int id, string skill_name, string skill_desc, string skill_effect, int count)
    {
        m_id = id;
        m_skill_name = skill_name;
        m_skill_desc = skill_desc;
        m_skill_effect = skill_effect;
        m_count = count;
    }
}



[System.Serializable]
public class BowItem
{
    public int m_id;//{ get; set; }
    public string m_name;// { get; set; }
    public string m_description;// { get; set; }
    public string m_skill_name;// { get; set; }
    public string m_skill_Desc;// { get; set; }
    public double m_skill_effect_01;// { get; set; }
    public double m_skill_effect_02;// { get; set; }
    public double m_skill_effect_03;// { get; set; }
    public double m_skill_effect_04;// { get; set; }
    public string m_default_skill;
    public double m_damage;// { get; set; }
    public double m_def;// { get; set; }
    public double m_dodging;//{ get; set; }
    public double m_hp;//{ get; set; }
    public int m_cost;// { get; set; }
    public string m_itemCode;

    public BowItem(int id, string name, string description, string skill_name, string skill_desc,
        double skill_effect_01, double skill_effect_02,
        double skill_effect_03, double skill_effect_04, 
        string default_skill, double damage,
        double def, double dodging, double hp, int cost, string itemCode)
    {
        m_id = id;
        m_name = name;
        m_description = description;
        m_skill_name = skill_name;
        m_skill_Desc = skill_desc;
        m_skill_effect_01 = skill_effect_01;
        m_skill_effect_02 = skill_effect_02;
        m_skill_effect_03 = skill_effect_03;
        m_skill_effect_04 = skill_effect_04;
        m_default_skill = default_skill;
        m_damage = damage;
        m_def = def;
        m_dodging = dodging;
        m_hp = hp;
        m_cost = cost;
        m_itemCode = itemCode;
    }

}