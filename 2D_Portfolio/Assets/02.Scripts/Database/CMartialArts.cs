using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using System.Text;

public class CMartialArts : SingleTon<CMartialArts>, IItemData
{
    [SerializeField]
    private string m_serverUrl;
    [SerializeField]
    private JsonData m_matialArtsJsonData;

    public List<SwordItem> m_matialArtsItemList = new List<SwordItem>();

    public void Awake()
    {
        StartCoroutine(LoadData());
    }
    public void Start()
    {
    }

    public void ConstructData()
    {
        for (int i = 0; i < m_matialArtsJsonData.Count; i++)
        {
            m_matialArtsItemList.Add(new SwordItem(
                (int)m_matialArtsJsonData[i]["id"],
                m_matialArtsJsonData[i]["name"].ToString(),
                m_matialArtsJsonData[i]["description"].ToString(),
                m_matialArtsJsonData[i]["skill_name"].ToString(),
                m_matialArtsJsonData[i]["skill_Desc"].ToString(),
                double.Parse(m_matialArtsJsonData[i]["skill_effect_01"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["skill_effect_02"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["skill_effect_03"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["skill_effect_04"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["damage"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["def"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["dodging"].ToString()),
                double.Parse(m_matialArtsJsonData[i]["hp"].ToString()),
                (int)m_matialArtsJsonData[i]["cost"]));
        }
    }

    public IEnumerator LoadData()
    {
        WWW www = new WWW(m_serverUrl);

        yield return www;

        //byte[] bytes = Encoding.Default.GetBytes(serverDB);
        string serverDB = Encoding.UTF8.GetString(www.bytes);

        m_matialArtsJsonData = JsonMapper.ToObject(serverDB);

        if (www.isDone)
        {
            ConstructData();
        }
        

      
    }
    public void LoadLocalData()
    { }
    public void ConstructLocalData() { }

}


[System.Serializable]
public class MartialArtsItem
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
    public double m_damage;// { get; set; }
    public double m_def;// { get; set; }
    public double m_dodging;//{ get; set; }
    public double m_hp;//{ get; set; }
    public int m_cost;// { get; set; }

    public MartialArtsItem(int id, string name, string description, string skill_name, string skill_desc,
        double skill_effect_01, double skill_effect_02,
        double skill_effect_03, double skill_effect_04, double damage,
        double def, double dodging, double hp, int cost)
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
        m_damage = damage;
        m_def = def;
        m_dodging = dodging;
        m_hp = hp;
        m_cost = cost;
    }

}