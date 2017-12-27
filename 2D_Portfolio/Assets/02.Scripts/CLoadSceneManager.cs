using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CLoadSceneManager : SingleTon<CLoadSceneManager>
{
    private static CLoadSceneManager Instance = null;

    void Awake()
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
        

	}
	

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
