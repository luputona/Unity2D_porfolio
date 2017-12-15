using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using System.Text;

public class CSwordData : CItemData
{
    [SerializeField]
    private string m_serverUrl;
    [SerializeField]
    private JsonData m_swordData;

    public List<SwordItem> m_swordItemList = new List<SwordItem>();
    
    protected override void Awake()
    {
        StartCoroutine(LoadData());
    }
    protected override void Start()
    {
    }

    protected override void ConstructData()
    {
        for(int i = 0; i < m_swordData.Count; i++)
        {
            m_swordItemList.Add(new SwordItem(
                (int)m_swordData[i]["id"], 
                m_swordData[i]["name"].ToString(), 
                m_swordData[i]["discription"].ToString(),
                m_swordData[i]["skill_name"].ToString(), 
                m_swordData[i]["skill_Dis"].ToString(), 
                double.Parse(m_swordData[i]["skill_effect_01"].ToString()),
                double.Parse(m_swordData[i]["skill_effect_02"].ToString()),
                double.Parse(m_swordData[i]["skill_effect_03"].ToString()),
                double.Parse(m_swordData[i]["skill_effect_04"].ToString()),
                double.Parse(m_swordData[i]["damage"].ToString()),
                double.Parse(m_swordData[i]["def"].ToString()),
                double.Parse(m_swordData[i]["dodging"].ToString()),
                double.Parse(m_swordData[i]["hp"].ToString()), 
                (int)m_swordData[i]["cost"]));
        }
    }

    protected override IEnumerator LoadData()
    {
        WWW www = new WWW(m_serverUrl);

        yield return www;
        
        //byte[] bytes = Encoding.Default.GetBytes(serverDB);
        string serverDB = Encoding.UTF8.GetString(www.bytes);
        
        m_swordData = JsonMapper.ToObject(serverDB);

        if(www.isDone)
        {
            
        }
        ConstructData();

        for (int i = 0; i < m_swordItemList.Count; i++)
        {
            Debug.Log(m_swordItemList[i].m_name);
        }
        Debug.Log(serverDB);
    }    
}

[System.Serializable]
public class SwordItem
{
    public int m_id;//{ get; set; }
    public string m_name;// { get; set; }
    public string m_discription;// { get; set; }
    public string m_skill_name;// { get; set; }
    public string m_skill_Dis;// { get; set; }
    public double m_skill_effect_01;// { get; set; }
    public double m_skill_effect_02;// { get; set; }
    public double m_skill_effect_03;// { get; set; }
    public double m_skill_effect_04;// { get; set; }
    public double m_damage;// { get; set; }
    public double m_def;// { get; set; }
    public double m_dodging;//{ get; set; }
    public double m_hp;//{ get; set; }
    public int m_cost;// { get; set; }

    public SwordItem(int id, string name, string discription, string skill_name, string skill_dis, 
        double skill_effect_01, double skill_effect_02, 
        double skill_effect_03, double skill_effect_04, double damage,
        double def, double dodging, double hp, int cost)
    {
        m_id = id;
        m_name = name;
        m_discription = discription;
        m_skill_name = skill_name;
        m_skill_Dis = skill_dis;
        m_skill_effect_01 = skill_effect_01;
        m_skill_effect_02 = skill_effect_02;
        m_skill_effect_03 = skill_effect_03;
        m_skill_effect_04 = skill_effect_04;
        m_damage = damage;
        m_def = def;
        m_dodging = dodging;
        m_hp = hp;
        m_cost = cost;
    }

}
