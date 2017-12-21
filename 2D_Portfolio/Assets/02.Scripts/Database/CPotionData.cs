using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using LitJson;

public class CPotionData : SingleTon<CPotionData>, IItemData
{
    [SerializeField]
    private string m_serverUrl;
    [SerializeField]
    private JsonData m_postionJsonData;

    public List<PotionItem> m_potionItemList = new List<PotionItem>();
    
    public void Awake()
    {
        StartCoroutine(LoadData());
    }
    public void Start()
    {

    }

    public void ConstructData()
    {
        for(int i = 0; i < m_postionJsonData.Count; i++)
        {
            m_potionItemList.Add(new PotionItem( 
                (int)m_postionJsonData[i]["id"] , m_postionJsonData[i]["name"].ToString() , m_postionJsonData[i]["description"].ToString(),
                (int)m_postionJsonData[i]["use_effect_01"], (int)m_postionJsonData[i]["use_effect_02"], (int)m_postionJsonData[i]["use_effect_03"], 
                (int)m_postionJsonData[i]["use_effect_04"], (int)m_postionJsonData[i]["cost"]));
        }
    }

   
    public IEnumerator LoadData()
    {
        WWW www = new WWW(m_serverUrl);

        yield return www;

        string serverDB = Encoding.UTF8.GetString(www.bytes);

        m_postionJsonData = JsonMapper.ToObject(serverDB);

        if(www.isDone)
        {
            ConstructData();
        }

                
    }
    public void ConstructLocalData()
    {
        throw new System.NotImplementedException();
    }

    public void LoadLocalData()
    {
        throw new System.NotImplementedException();
    }

  
}

[Serializable]
public class PotionItem
{
    public int m_id;
    public string m_name;
    public string m_description;
    public int m_useEffect_01;
    public int m_useEffect_02;
    public int m_useEffect_03;
    public int m_useEffect_04;
    public int m_cost;

    public PotionItem(int id, string name, string desc, int useEffect_01, int useEffect_02, int useEffect_03, int useEffect_04, int cost)
    {
        m_id = id;
        m_name = name;
        m_description = desc;
        m_useEffect_01 = useEffect_01;
        m_useEffect_02 = useEffect_02;
        m_useEffect_03 = useEffect_03;
        m_useEffect_04 = useEffect_04;
        m_cost = cost;
    }

    
}
