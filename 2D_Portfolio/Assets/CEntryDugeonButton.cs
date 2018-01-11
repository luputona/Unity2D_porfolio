using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CEntryDugeonButton : MonoBehaviour
{
    [SerializeField]
    private Button m_button = null;

    private void Awake()
    {
        m_button = this.GetComponent<Button>();
    }

    // Use this for initialization
    void Start ()
    {
        m_button.onClick.AddListener(() => CLoadSceneManager.GetInstance.ChangeScene("Dungeon"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
