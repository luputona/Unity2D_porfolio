using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;

public class LoadPotionData : SingleTon<LoadPotionData>
{
    private static LoadPotionData Instance;
    [SerializeField]
    private JsonData m_potionData;
    [SerializeField]
    private string dataUrl;

    public bool m_isComplete = false; //데이타 다운 체크

    public List<PostionData> m_postionData = new List<PostionData>();

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
    }


    IEnumerator DownLoadPotionData()
    {
        WWW www = new WWW(dataUrl);
        yield return www;
        
        string severDB = www.text;
    }

    //IEnumerator GetLocalPositonData()
    //{

    //}
	
}

public class PostionData
{

}