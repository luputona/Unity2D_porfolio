using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CInvenCategory : CInventoryManager
{
    [SerializeField]
    private Button m_btn = null;

    void Awake()
    {
        m_btn = GetComponent<Button>();
    }

    // Use this for initialization
    void Start ()
    {
        m_btn.onClick.AddListener(() => base.ChangeCategory(this.m_eINVENTORY_CATEGORY) );
	}
	
}
