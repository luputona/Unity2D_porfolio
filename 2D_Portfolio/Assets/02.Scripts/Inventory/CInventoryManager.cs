using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CInventoryManager : MonoBehaviour
{
    public GameObject m_inventory_Panel = null;
    public GameObject m_inst_Item_List = null;
    public Mask m_viewPortMask = null;

    public List<GameObject> m_invenSlotList = new List<GameObject>();

    public GameObject[] m_invenSlotArray = new GameObject[70];

    private void Awake()
    {
        m_inventory_Panel = GameObject.Find("inst_Inventory_Panel").gameObject;
        m_inst_Item_List = m_inventory_Panel.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject;
        m_viewPortMask = m_inventory_Panel.transform.GetChild(0).transform.GetChild(0).GetComponent<Mask>();
    }

    // Use this for initialization
    void Start ()
    {
        
		for(int i = 0; i < m_inst_Item_List.transform.childCount; i++)
        {
            m_invenSlotArray[i] = m_inst_Item_List.transform.GetChild(i).gameObject;
            m_invenSlotList.Add(m_inst_Item_List.transform.GetChild(i).gameObject);
            m_invenSlotList[i].transform.name = string.Format("Slot_{0}", i);

        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        
        if(Input.GetKeyDown(KeyCode.Space))
        {

            for(int i = 0;  i < 10; i++)
            {
                m_inst_Item_List.transform.GetChild(0).SetSiblingIndex(m_inst_Item_List.transform.childCount + i);
                
            }
                        
            //for(int i = 0; i < 70; i++)
            //{
            //    m_invenSlotArray[i] = m_inst_Item_List.transform.GetChild(i).gameObject;
            //}
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            for (int i = 0; i < 10 ; i--)
            {
                m_inst_Item_List.transform.GetChild(69).SetSiblingIndex( i);

            }
            //for (int i = 69; i >= 0; i--)
            //{
            //    m_invenSlotArray[i] = m_inst_Item_List.transform.GetChild(i).gameObject;
                
                
            //}
        }
        for(int i = 0;  i < 10; i ++)
        {
            Debug.Log(m_invenSlotArray[i].transform.GetSiblingIndex());
        }
        

        RectTransform t = m_invenSlotArray[0].GetComponent<RectTransform>();

        //Debug.Log("local : " + t.rect.position);
        //Debug.Log("global : " + t.localPosition);

    }
}
