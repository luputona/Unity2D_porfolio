using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using LitJson;


public class CGoodsShopData : SingleTon<CGoodsShopData>, IItemData
{
    private static CGoodsShopData _instance = null;

    //서버 데이타 로드
    [SerializeField]
    private string m_goodsShopDataUrl = "";
    [SerializeField]
    private JsonData m_goodsData;

    public List<GoodsShopCategory> m_goodsCategoryList = new List<GoodsShopCategory>();

    //로컬 데이타 로드
    [SerializeField]
    private JsonData m_localGoodsData;
    [SerializeField]
    private string m_localPath;
    public List<GoodsShopCategory> m_localGoodsCategoryList = new List<GoodsShopCategory>();


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
        //StartCoroutine(LoadData());
        LoadLocalData();
    }

    // Use this for initialization
    public void Start()
    {

    }

    public void ConstructData()
    {
        for(int i = 0; i < m_goodsData.Count; i++)
        {
            m_goodsCategoryList.Add(new GoodsShopCategory((int)m_goodsData[i]["id"], m_goodsData[i]["Category"].ToString()));
        }
    }

    public void ConstructLocalData()
    {
        for (int i = 0; i < m_localGoodsData.Count; i++)
        {
            m_localGoodsCategoryList.Add(new GoodsShopCategory((int)m_localGoodsData[i]["id"], m_localGoodsData[i]["Category"].ToString()));

            //Debug.Log(m_categoryLocalList[i].m_category);
        }
    }

    public IEnumerator LoadData()
    {
        WWW www = new WWW(m_goodsShopDataUrl);

        yield return www;

        string serverDB = Encoding.UTF8.GetString(www.bytes);

        m_goodsData = JsonMapper.ToObject(serverDB);

        if (www.isDone)
        {
            ConstructData();
        }
    }

    public void LoadLocalData()
    {
        TextAsset jdatatext = Resources.Load<TextAsset>("Data/GoodsShopData");
        m_localPath = jdatatext.text;

        m_localGoodsData = JsonMapper.ToObject(m_localPath);
        //m_localGoodsData = JsonMapper.ToObject(File.ReadAllText(Application.streamingAssetsPath + "/GoodsShopData.json"));

        ConstructLocalData();
    }

}

[System.Serializable]
public class GoodsShopCategory
{
    public int m_id;// { set; get; }
    public string m_category;// { get; set; }

    public GoodsShopCategory(int id, string category)
    {
        m_id = id;
        m_category = category;
    }

}
