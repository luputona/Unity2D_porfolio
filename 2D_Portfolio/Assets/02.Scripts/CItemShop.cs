using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CItemShop : CVillageManager
{
    protected new GameObject ShopSlotPrefab
    {
        get
        {
            return base.ShopSlotPrefab;
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void TouchGetObj()
    {
        base.TouchGetObj();
    }
    protected override void OpenShop()
    {
        base.OpenShop();
        if (m_shopinfo == ShopInfo.ItemShop)
        {
            Debug.Log("아이템샵");
            m_shopPanel.SetActive(true);
            m_shop[4].SetActive(true);
            m_shopDictionary[ShopInfo.ItemShop].SetActive(true);
        }
    }


}
