using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CEquipWeapon : MonoBehaviour
{
    [SerializeField]
    private Button m_btn;

	// Use this for initialization
	void Start ()
    {
        m_btn = GetComponent<Button>();

        m_btn.onClick.AddListener(() => CUpdateUserInfo.GetInstance.SetCurrentEquipWeapon());
        
	}
	
	
}
