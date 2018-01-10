using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CEquipWeapon : MonoBehaviour
{
    [SerializeField]
    private Button m_btn;

    private void Awake()
    {
        m_btn = GetComponent<Button>();
    }

    // Use this for initialization
    void Start ()
    {
       

        m_btn.onClick.AddListener(() => CUpdateUserInfo.GetInstance.SetCurrentEquipWeapon());
        
	}
	
	
}
