using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CLoadSceneManager : SingleTon<CLoadSceneManager>
{
    private static CLoadSceneManager Instance = null;
    [SerializeField]
    private float m_timer;
    [SerializeField]
    private bool m_waiting = false;


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

    private void Start()
    {
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(4.0f);

        m_waiting = true;
    }

    public void GotoMainScene(string sceneName)
    {
        if(m_waiting)
        {
            SceneManager.LoadScene(sceneName);
        }       
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
