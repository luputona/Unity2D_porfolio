using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDungeonManager : SingleTon<CDungeonManager>
{
    private static CDungeonManager Instance = null;

    public int m_floorIndex;

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

    // Use this for initialization
    void Start ()
    {
        if(m_floorIndex == 0)
        {
            //TODO : 해당 플로어의 던전 생성
        }
        else if(m_floorIndex == 1)
        {
            //TODO : 해당 플로어의 던전 생성
        }
        else if (m_floorIndex == 2)
        {
            //TODO : 해당 플로어의 던전 생성
        }
        else if (m_floorIndex == 3)
        {
            //TODO : 해당 플로어의 던전 생성
        }
        else if (m_floorIndex == 4)
        {
            //TODO : 해당 플로어의 던전 생성
        }
        else if (m_floorIndex == 5)
        {


        }

    }
	
	// Update is called once per frame
	void Update ()
    {

	}
}
