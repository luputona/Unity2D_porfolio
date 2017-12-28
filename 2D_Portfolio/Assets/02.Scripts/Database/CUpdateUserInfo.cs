using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUpdateUserInfo : MonoBehaviour
{
    private static CUpdateUserInfo Instance = null;

    private void Awake()
    {
        if(Instance != null)
        {
            GameObject.Destroy(this);
        }
        else
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
