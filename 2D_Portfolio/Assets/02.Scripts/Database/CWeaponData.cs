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

    //검
    public List<SwordItem> m_swordItemList = new List<SwordItem>();
    public Dictionary<string, SwordItem> m_swordItemDic = new Dictionary<string, SwordItem>();
    public Dictionary<string, Dictionary<int, DefaultSwordSkill>> m_swordDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultSwordSkill>>();
    
    //스태프
    public List<StaffItem> m_staffItemList = new List<StaffItem>();
    public Dictionary<string, StaffItem> m_staffItemDic = new Dictionary<string, StaffItem>();
    public Dictionary<string, Dictionary<int, DefaultStaffSkill>> m_staffDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultStaffSkill>>();
    
    //창
    public List<SpearItem> m_spearItemList = new List<SpearItem>();
    public Dictionary<string, SpearItem> m_spearItemDic = new Dictionary<string, SpearItem>();
    public Dictionary<string, Dictionary<int, DefaultSpearSkill>> m_spearDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultSpearSkill>>();

    //격투
    public List<MartialArtsItem> m_martialArtsItemList = new List<MartialArtsItem>();
    public Dictionary<string, MartialArtsItem> m_martialItemDic = new Dictionary<string, MartialArtsItem>();
    public Dictionary<string, Dictionary<int, DefaultMartialArtsSkill>> m_martialDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultMartialArtsSkill>>();

    //메이스
    public List<MaceItem> m_maceItemList = new List<MaceItem>();
    public Dictionary<string, MaceItem> m_maceItemDic = new Dictionary<string, MaceItem>();
    public Dictionary<string, Dictionary<int, DefaultMaceSkill>> m_maceDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultMaceSkill>>();

    //활
    public List<BowItem> m_bowItemList = new List<BowItem>();
    public Dictionary<string, BowItem> m_bowItemDic = new Dictionary<string, BowItem>();
    public Dictionary<string, Dictionary<int, DefaultBowSkill>> m_bowDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultBowSkill>>();

    //악세사리
    public List<AccessoryItem> m_accessoryItemList = new List<AccessoryItem>();
    public Dictionary<string, AccessoryItem> m_accessoryItemDic = new Dictionary<string, AccessoryItem>();
    public Dictionary<string, Dictionary<int, DefaultAccessorySkill>> m_accessoryDefaultSkillDic = new Dictionary<string, Dictionary<int, DefaultAccessorySkill>>();
        

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
        //Debug.Log(m_martialArtsItemList[1].m_name);
        //Debug.Log(m_martialItemDic["w060002"].m_skill_Desc);
        //Debug.Log(m_bowItemDic["w010001"].m_name);
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
