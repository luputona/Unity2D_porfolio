using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using LitJson;

public class CUserData : SingleTon<CUserData>
{
    private const int DataIndex = 0;
    private static CUserData Instance = null;

    [SerializeField]
    private string m_userInfoUrl;
    [SerializeField]
    private UserMainInfo m_userData;
    [SerializeField]
    private JsonData m_userJsonData;

    public List<UserMainInfo> m_userDataList = new List<UserMainInfo>();

    public List<UserStatus> m_userStatusList = new List<UserStatus>();
    public Dictionary<string, GameObject> m_weaponInventory = new Dictionary<string, GameObject>();

    
    private void Awake()
    {
        if(Instance != null)
        {
            GameObject.Destroy(this);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }


        StartCoroutine(LoadData());
    }
    // Use this for initialization
    void Start ()
    {
        
    }

    private void Update()
    {
        

    }


    IEnumerator LoadData()
    {
        WWW www = new WWW(m_userInfoUrl);

        yield return www;

        string severDB = Encoding.UTF8.GetString(www.bytes);
        m_userJsonData = JsonMapper.ToObject(severDB);

        if(www.isDone)
        {
            ConstructData();
            StatusToJson();
        }
        
    }
    
    void ConstructData()
    {
        for(int i =0; i< m_userJsonData.Count; i++)
        {
            m_userDataList.Add(new UserMainInfo(
                (int)m_userJsonData[i]["id"], 
                m_userJsonData[i]["nickname"].ToString(), 
                m_userJsonData[i]["status"].ToString(),
                m_userJsonData[i]["cur_set_itemcode"].ToString(),
                (int)m_userJsonData[i]["gold"],
                m_userJsonData[i]["weaponInventory"].ToString(), 
                m_userJsonData[i]["goodsInventory"].ToString(),
                m_userJsonData[i]["clearDungeon"].ToString(), 
                (int)m_userJsonData[i]["userCode"] ));

        }
    }

    public void StatusToJson()
    {

        JsonData tData = JsonMapper.ToObject(m_userDataList[0].m_status);

        Debug.Log(m_userDataList[0].m_status);
        Debug.Log(tData[0][0].ToString());

        //m_userStatusList.Add(new UserStatus((int)tData[DataIndex]["userCode"],
        //    double.Parse(tData[DataIndex]["damage"].ToString()),
        //    double.Parse(tData[DataIndex]["def"].ToString()),
        //    double.Parse(tData[DataIndex]["dodge"].ToString()),
        //    double.Parse(tData[DataIndex]["hp"].ToString()),
        //    double.Parse(tData[DataIndex]["str"].ToString()),
        //    double.Parse(tData[DataIndex]["dex"].ToString())));


        m_userStatusList.Add(new UserStatus(
            (int)tData[DataIndex][0],
         double.Parse(tData[DataIndex][1].ToString()),
         double.Parse(tData[DataIndex][2].ToString()),
         double.Parse(tData[DataIndex][3].ToString()),
         double.Parse(tData[DataIndex][4].ToString()),
         double.Parse(tData[DataIndex][5].ToString()),
         double.Parse(tData[DataIndex][6].ToString())));
    }

    public void WeaponInventoryToJson()
    {
        JsonData tData = JsonMapper.ToObject(m_userDataList[0].m_weaponInven);


    }
    
    //void LoadLocalData();
    //void ConstructLocalData();
    
}




[System.Serializable]
public class UserStatus
{
    public int userCode;
    public double damage;
    public double def;
    public double dodge;
    public double hp;
    public double str;
    public double dex;
    


    public UserStatus(int tUserCode, double tDamage, double tDef, double tDodge, double tHp, double tStr, double tDex)
    {
        userCode = tUserCode;
        damage = tDamage;
        def = tDef;
        dodge = tDodge;
        hp = tHp;
        str = tStr;
        dex = tDex;
        
    }

}


[System.Serializable]
public class UserMainInfo
{
    public int m_id;
    public string m_name;
    public string m_status; //json 을 텍스트로 변경해서 받아야함
    public int m_gold;
    public string m_weaponInven; //json 을 텍스트로 변경해서 받아야함
    public string m_goodsInven; //json 을 텍스트로 변경해서 받아야함
    public string m_clearDungeon; //json 을 텍스트로 변경해서 받아야함
    public int m_userCode;
    public string cur_set_itemcode;

    public UserMainInfo(int id, string name, string status, string tcur_set_itemcode, int gold, string weaponInven, string goodsInven, string claerDungeon, int userCode)
    {
        m_id = id;
        m_name = name;
        m_status = status;
        cur_set_itemcode = tcur_set_itemcode;
        m_gold = gold;
        m_weaponInven = weaponInven;
        m_goodsInven = goodsInven;
        m_clearDungeon = claerDungeon;
        m_userCode = userCode;
        
    }

}



//public class JsonMapper
//{
//    static JsonMapper()
//    {
//        LitJson.JsonMapper.RegisterExporter<float>((obj, writer) => { writer.Write(System.Convert.ToDouble(obj)); });
//        LitJson.JsonMapper.RegisterImporter<double, float>((input) => { return System.Convert.ToSingle(input); });
//        LitJson.JsonMapper.RegisterImporter<System.Int32, long>((input) => { return System.Convert.ToInt64(input); });
//    }

//    //public static JsonData ToObject(string json)
//    //{
//    //    return LitJson.JsonMapper.ToObject(json);
//    //}

//    //public static string ToJson(object obj)
//    //{
//    //    return LitJson.JsonMapper.ToJson(obj);
//    //}
//}



