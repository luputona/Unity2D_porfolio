using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CUploadUserData : SingleTon<CUploadUserData>
{
    [SerializeField]
    private string m_uploadUserAllDataURL;
    [SerializeField]
    private string m_uploadUserNameURL;
    [SerializeField]
    private string m_uploadWeaponInvenURL;
    [SerializeField]
    private string m_uploadGoodsInvenURL;
    [SerializeField]
    private string m_uploadUserSimpleDataURL;

    private static CUpdateUserInfo Instance = null;

    private void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(this);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }



    void Start()
    {
     
    }

    // Update is called once per frame
    void Update ()
    {


    }

    public void UploadUserData(string name, string status, int rank, string tcur_set_itemcode, int gold, string weaponInven, string goodsInven, string claerDungeon, int point, int userCode)
    {
        WWWForm form = new WWWForm();
        form.AddField("nickname", name);
        form.AddField("status", status);
        form.AddField("rank", rank);
        form.AddField("cur_set_itemcode", tcur_set_itemcode);
        form.AddField("gold", gold);
        form.AddField("weaponInventory", weaponInven);
        form.AddField("goodsInventory", goodsInven);
        form.AddField("clearDungeon", claerDungeon);
        form.AddField("point", point);
        form.AddField("userCode", userCode);

        WWW www = new WWW(m_uploadUserAllDataURL, form);
    }

    public void UploadUserAllData()
    {
        WWWForm form = new WWWForm();
        form.AddField("nickname", CUpdateUserInfo.GetInstance.m_name);
        form.AddField("status", CUpdateUserInfo.GetInstance.GetStatusToJson());
        form.AddField("rank", CUpdateUserInfo.GetInstance.m_rank);
        form.AddField("cur_set_itemcode", CUpdateUserInfo.GetInstance.m_cur_Set_ItemCode);
        form.AddField("gold", CUpdateUserInfo.GetInstance.m_gold);
        form.AddField("weaponInventory", CUpdateUserInfo.GetInstance.GetWeaponInventoryToJson());
        form.AddField("goodsInventory", CUpdateUserInfo.GetInstance.GetGoodsInventoryToJson());
        form.AddField("clearDungeon", CUpdateUserInfo.GetInstance.GetClearDungeonToJson());
        form.AddField("point", CUpdateUserInfo.GetInstance.m_point);
        form.AddField("userCode", CUpdateUserInfo.GetInstance.m_userCode);

        WWW www = new WWW(m_uploadUserAllDataURL, form);
    }


    //스테이터스 UI전용 
    public void UploadUserSimpleData()
    {
        WWWForm form = new WWWForm();
        //form.AddField("nickname", CUpdateUserInfo.GetInstance.m_name);
        form.AddField("status", CUpdateUserInfo.GetInstance.GetStatusToJson());
        form.AddField("rank", CUpdateUserInfo.GetInstance.m_rank);
        form.AddField("cur_set_itemcode", CUpdateUserInfo.GetInstance.m_cur_Set_ItemCode);
        //form.AddField("gold", CUpdateUserInfo.GetInstance.m_gold);
        //form.AddField("weaponInventory", CUpdateUserInfo.GetInstance.GetWeaponInventoryToJson());
        //form.AddField("goodsInventory", CUpdateUserInfo.GetInstance.GetGoodsInventoryToJson());
        //form.AddField("clearDungeon", CUpdateUserInfo.GetInstance.GetClearDungeonToJson());
        form.AddField("point", CUpdateUserInfo.GetInstance.m_point);
        form.AddField("userCode", CUpdateUserInfo.GetInstance.m_userCode);

        WWW www = new WWW(m_uploadUserAllDataURL, form);
    }

    // 이름 변경ㅇ 전용
    public void UploadUserName()
    {
        //string str = "http://13.112.49.138/DGtop/UploadName.php";
        WWWForm form = new WWWForm();
        form.AddField("nickname", CUpdateUserInfo.GetInstance.m_name);
        form.AddField("userCode", CUpdateUserInfo.GetInstance.m_userCode);

        WWW www = new WWW(m_uploadUserNameURL, form);
    }

    //무기 인벤토리 전용
    public void UploadWeaponInventory()
    {

    }

    //잡화 인벤토리 전용
    public void UploadGoodsInventory()
    {

    }

   
}
