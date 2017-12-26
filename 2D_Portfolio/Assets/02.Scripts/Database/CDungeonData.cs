using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using LitJson;


public class CDungeonData : SingleTon<CDungeonData>, IItemData
{
    private static CDungeonData _instance = null;

    //서버 데이타 로드
    [SerializeField]
    private string m_dungeonDataUrl;
    [SerializeField]
    private JsonData m_dungeonData;
    public List<DungeonInfo> m_dungeonList = new List<DungeonInfo>();

    //로컬데이타 로드 
    [SerializeField]
    private JsonData m_localData;
    [SerializeField]
    private string m_localPath;
    public List<DungeonInfo> m_dungeonLocalList = new List<DungeonInfo>();


    public void Awake()
    {
        //if (_instance != null)
        //{
        //    GameObject.Destroy(this);
        //}
        //else
        //{
        //    GameObject.DontDestroyOnLoad(gameObject);
        //}

        StartCoroutine(LoadData());
        //LoadLocalData();
    }

    // Use this for initialization
    public void Start ()
    {
		
	}
    

    public IEnumerator LoadData()
    {
        WWW www = new WWW(m_dungeonDataUrl);

        yield return www;

        string serverDB = Encoding.UTF8.GetString(www.bytes);

        m_dungeonData = JsonMapper.ToObject(serverDB);

        if (www.isDone)
        {
            ConstructData();
        }
    }

    public void LoadLocalData()
    {
        TextAsset jdatatext = Resources.Load<TextAsset>("Data/DGData");
        m_localPath = jdatatext.text;
        //m_localData = JsonMapper.ToObject(File.ReadAllText(Application.streamingAssetsPath + "/DGData.json"));
        m_localData = JsonMapper.ToObject(m_localPath);

        ConstructLocalData();
    }

    public void ConstructLocalData()
    {
        for (int i = 0; i < m_localData.Count; i++)
        {
            m_dungeonLocalList.Add(new DungeonInfo((int)m_localData[i]["id"], (int)m_localData[i]["floor"], 
                m_localData[i]["boss"].ToString(),m_localData[i]["bossTitle"].ToString() ,(int)m_localData[i]["level"], (int)m_localData[i]["clear"]));

            //Debug.Log(m_categoryLocalList[i].m_category);
        }
    }

    public void ConstructData()
    {
        for (int i = 0; i < m_dungeonData.Count; i++)
        {
            m_dungeonList.Add(new DungeonInfo((int)m_dungeonData[i]["id"],
                (int)m_dungeonData[i]["floor"],
                m_dungeonData[i]["boss"].ToString(), 
                m_dungeonData[i]["bossTitle"].ToString(),
                (int)m_dungeonData[i]["level"], 
                (int)m_dungeonData[i]["clear"]));
            //Debug.Log("C :" + m_categoryList[i].m_category);
        }
        //CShopCategory.GetInstance.m_categoryCount = m_categoryList.Count;
    }


}

[System.Serializable]
public class DungeonInfo
{
    public int m_id;
    public int m_floor;
    public string m_bossName;
    public string m_bossTitle;
    public int m_level;
    public int m_clear;

    public  DungeonInfo(int id, int floor, string bossName,string bossTitle , int level, int clear)
    {
        m_id = id;
        m_floor = floor;
        m_bossName = bossName;
        m_bossTitle = bossTitle;
        m_level = level;
        m_clear = clear;
    }


}
