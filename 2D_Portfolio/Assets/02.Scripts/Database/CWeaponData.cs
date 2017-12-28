using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;

public class CWeaponData : SingleTon<CWeaponData>, IItemData
{
    private static CWeaponData _instance;

    //서버 데이타 로드
    [SerializeField]
    private string m_weaponDataUrl;
    [SerializeField]
    private JsonData m_weaponData;
    public List<WeaponCategory> m_categoryList = new List<WeaponCategory>();

    //로컬데이타 로드 
    [SerializeField]
    private JsonData m_localData;
    [SerializeField]
    private string m_localPath;
    public List<WeaponCategory> m_categoryLocalList = new List<WeaponCategory>();
   
    
    public void Awake()
    {
        if (_instance != null)
        {
            GameObject.Destroy(this);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }

        StartCoroutine(LoadData());
        LoadLocalData();
        
    }
    public void Start()
    {
       
    }

    public IEnumerator LoadData()
    {
        WWW www = new WWW(m_weaponDataUrl);

        yield return www;

        string serverDB = Encoding.UTF8.GetString(www.bytes);

        m_weaponData = LitJson.JsonMapper.ToObject(serverDB);

        if (www.isDone)
        {
            ConstructData();       
        }        
    }

    public void LoadLocalData()
    {
        TextAsset jdatatext = Resources.Load<TextAsset>("Data/WeaponData");
        m_localPath = jdatatext.text;
        //m_localData = JsonMapper.ToObject(File.ReadAllText(Application.streamingAssetsPath + "/WeaponData.json"));
        m_localData = LitJson.JsonMapper.ToObject(m_localPath);

        ConstructLocalData();
    }

    public void ConstructLocalData()
    {
        for (int i = 0; i < m_localData.Count; i++)
        {
            m_categoryLocalList.Add(new WeaponCategory((int)m_localData[i]["id"], m_localData[i]["Category"].ToString()));

            //Debug.Log(m_categoryLocalList[i].m_category);
        }
    }

    public void ConstructData()
    {
        for (int i = 0; i < m_weaponData.Count; i++)
        {
            m_categoryList.Add(new WeaponCategory((int)m_weaponData[i]["id"], m_weaponData[i]["Category"].ToString()));
            //Debug.Log("C :" + m_categoryList[i].m_category);
        }
        //CShopCategory.GetInstance.m_categoryCount = m_categoryList.Count;
    }

    
}
[System.Serializable]
public class WeaponCategory
{
    public int m_id;// { set; get; }
    public string m_category;// { get; set; }

    public WeaponCategory(int id, string category)
    {
        m_id = id;
        m_category = category;
    }

}
