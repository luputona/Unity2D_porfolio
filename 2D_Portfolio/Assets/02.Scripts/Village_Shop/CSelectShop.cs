using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSelectShop : MonoBehaviour
{
    public enum ShopInfo
    {        
        WeaponShop,
        GoodsShop,
        EntryDungeonDesk,
        ShopContenItemList,
        Category,
        ItemDescription,
        BackButton,
        EntryDungeonButton,
        Default = 99
    }

    public ShopInfo m_shopinfo = ShopInfo.Default;
}
