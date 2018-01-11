using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.UI;
using LitJson;

public class CUserData : SingleTon<CUserData>
{
    private const int DataIndex = 0; // TODO : 테스트로 DB에 있는 0번 유저 정보만 불러오게 고정 추후 클라에 있는 유저 코드랑 서버DB랑 비교해서 해당 유저걸로 불러오게 변경
    private static CUserData Instance = null;

    [SerializeField]
    private string m_userInfoUrl;
    [SerializeField]
    private UserMainInfo m_userData;
    [SerializeField]
    private JsonData m_userJsonData;
    [SerializeField]
    private string m_searchUserCodeURL;
    [SerializeField]
    private Text m_isDone;
    public JsonData m_statusData;
    public JsonData m_weaponInvenData;
    public JsonData m_potionInvenData;
    
    public List<UserMainInfo> m_userDataList = new List<UserMainInfo>();

    public List<UserStatus> m_userStatusList = new List<UserStatus>();
    public List<PotionInventory> m_potionInvenList = new List<PotionInventory>();
    public List<WeaponInventory> m_weaponInvenList = new List<WeaponInventory>();    
    
    public Dictionary<string, PotionInventory> m_potionInvenDic = new Dictionary<string, PotionInventory>();
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

        //TODO : 임시로 유저코드 강제 고정  추후 변경
        PlayerPrefs.SetInt("usercode",12345678);

        StartCoroutine(LoadData());

    }
    // Use this for initialization
    void Start ()
    {
        m_isDone = GameObject.Find("IsDone").GetComponent<Text>();


    }


    public IEnumerator LoadData()
    {
        WWWForm form = new WWWForm();
        form.AddField("userCode", PlayerPrefs.GetInt("usercode"));


        WWW www = new WWW(m_searchUserCodeURL, form);

        //WWW www = new WWW(m_userInfoUrl);

        yield return www;

        string severDB = Encoding.UTF8.GetString(www.bytes);
        m_userJsonData = JsonMapper.ToObject(severDB);


        if (www.error == null)
        {
            ConstructData();
            StatusToObject();
            WeaponInventoryToObject();
            PotionInventoryToObject();

            if(www.isDone)
            {
                m_isDone.text = string.Format("{0}", m_userStatusList[0].dex);
            }
            //CUpdateUserInfo.GetInstance.InitUserStatus();
        }
        else
        {
            Debug.Log("ERRor : " + www.error);
        }
        //CStatus.GetInstance.InitSetStatus(m_userStatusList[0].damage, m_userStatusList[0].def, m_userStatusList[0].dodge, m_userStatusList[0].hp, m_userStatusList[0].str, m_userStatusList[0].dex);

    }

    void UserCodeCheck()
    {
        WWWForm form = new WWWForm();

        form.AddField("userCode", 12345678);

        WWW www = new WWW(m_searchUserCodeURL);
    }

    void ConstructData()
    {
        for(int i =0; i< m_userJsonData.Count; i++)
        {
            m_userDataList.Add(new UserMainInfo(                
                m_userJsonData[i]["nickname"].ToString(), 
                m_userJsonData[i]["status"].ToString(),
                (int)m_userJsonData[i]["rank"],
                m_userJsonData[i]["cur_set_itemcode"].ToString(),
                (int)m_userJsonData[i]["gold"],
                m_userJsonData[i]["weaponInventory"].ToString(), 
                m_userJsonData[i]["potionInventory"].ToString(),
                m_userJsonData[i]["goodsInventory"].ToString(),
                m_userJsonData[i]["clearDungeon"].ToString(), 
                (int)m_userJsonData[i]["point"],
                (int)m_userJsonData[i]["userCode"] ));

        }        
    }

    public void StatusToObject()
    {

        m_statusData = JsonMapper.ToObject(m_userDataList[0].m_status);

        //Debug.Log(m_userDataList[0].m_status);
       // Debug.Log(m_statusData[0][0].ToString());

        //m_userStatusList.Add(new UserStatus((int)m_statusData[DataIndex]["userCode"],
        //    double.Parse(m_statusData[DataIndex]["damage"].ToString()),
        //    double.Parse(m_statusData[DataIndex]["def"].ToString()),
        //    double.Parse(m_statusData[DataIndex]["dodge"].ToString()),
        //    double.Parse(m_statusData[DataIndex]["hp"].ToString()),
        //    double.Parse(m_statusData[DataIndex]["str"].ToString()),
        //    double.Parse(m_statusData[DataIndex]["dex"].ToString())));


        m_userStatusList.Add(new UserStatus(            
         double.Parse(m_statusData[0].ToString()),
         double.Parse(m_statusData[1].ToString()),
         double.Parse(m_statusData[2].ToString()),
         double.Parse(m_statusData[3].ToString()),
         double.Parse(m_statusData[4].ToString()),
         double.Parse(m_statusData[5].ToString()),
         (int)m_statusData[6]));
    }

    public void WeaponInventoryToObject()
    {
        m_weaponInvenData = JsonMapper.ToObject(m_userDataList[0].m_weaponInven);

        for(int i = 0; i < m_weaponInvenData.Count; i++ )
        {
            m_weaponInvenList.Add(new WeaponInventory(m_weaponInvenData[i][0].ToString(), m_weaponInvenData[i][1].ToString()));
        }
    }
    public void PotionInventoryToObject()
    {
        m_potionInvenData = JsonMapper.ToObject(m_userDataList[0].m_potionInven);
        
        for(int i = 0; i < m_potionInvenData.Count; i++)
        {
            m_potionInvenList.Add( new PotionInventory(  m_potionInvenData[i]["itemCode"].ToString(), (int)m_potionInvenData[i]["count"]));
            m_potionInvenDic.Add(m_potionInvenData[i][0].ToString() ,new PotionInventory(m_potionInvenData[i][0].ToString(), (int)m_potionInvenData[i][1]));
        }
        
    }
    
    //void LoadLocalData();
    //void ConstructLocalData();
    
}
[System.Serializable]
public class WeaponInventory
{
    public string m_category;
    public string m_itemCode;

    public WeaponInventory(string category, string itemCode)
    {
        m_category = category;
        m_itemCode = itemCode;
    }
}
[System.Serializable]
public class PotionInventory
{
    public string itemCode;
    public int count;

    public PotionInventory(string itemCode, int count)
    {
        this.itemCode = itemCode;
        this.count = count;
    }
}



[System.Serializable]
public class UserStatus
{
    public double damage;
    public double def;
    public double dodge;
    public double hp;
    public double str;
    public double dex;
    public int userCode;

    public UserStatus( double tDamage, double tDef, double tDodge, double tHp, double tStr, double tDex, int tUserCode)
    {
        userCode = tUserCode;
        damage = tDamage;
        def = tDef;
        dodge = tDodge;
        hp = tHp;
        str = tStr;
        dex = tDex;
        
        //CStatus.GetInstance.InitSetStatus(tDamage, tDef, tDodge , tHp, tStr, tDex);
    }

}


[System.Serializable]
public class UserMainInfo
{    
    public string m_name;
    public string m_status; //json 을 텍스트로 변경해서 받아야함
    public int m_rank;
    public int m_gold;
    public string m_weaponInven; //json 을 텍스트로 변경해서 받아야함
    public string m_potionInven;
    public string m_goodsInven; //json 을 텍스트로 변경해서 받아야함
    public string m_clearDungeon; //json 을 텍스트로 변경해서 받아야함
    public int m_point;
    public int m_userCode;
    public string m_cur_set_itemcode;

    public UserMainInfo(string name,  string status, int rank, string tcur_set_itemcode, int gold, string weaponInven, string potionInven,string goodsInven, string claerDungeon, int point ,int userCode)
    {
       
        m_name = name;
        m_status = status;
        m_rank = rank;
        m_cur_set_itemcode = tcur_set_itemcode;
        m_gold = gold;
        m_weaponInven = weaponInven;
        m_potionInven = potionInven;
        m_goodsInven = goodsInven;
        m_clearDungeon = claerDungeon;
        m_point = point;
        m_userCode = userCode;
        
        //CUpdateUserInfo.GetInstance.InitUserInfo(point, userCode, name, rank, gold, tcur_set_itemcode);
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



