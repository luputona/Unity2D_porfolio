using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSelectShop : MonoBehaviour
{
    public enum ShopInfo
    {        
        WeaponShop,
        ItemShop,
        EntryDungeon,
        ShopContenItemList,
        Category,
        ItemDescription,
        BackButton,
        Default = 99
    }

    public ShopInfo m_shopinfo = ShopInfo.Default;
}
