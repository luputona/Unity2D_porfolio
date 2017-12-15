using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CShopCategory : MonoBehaviour
{
    [SerializeField]
    private GameObject m_shop_Catergory;

    private void Awake()
    {
        m_shop_Catergory = GameObject.Find("Shop_Catergory");

    }


}
