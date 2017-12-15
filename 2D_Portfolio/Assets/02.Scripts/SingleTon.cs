using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTon<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_instance = null;

    public static T GetInstance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = FindObjectOfType(typeof(T)) as T;
                if(m_instance == null)
                {
                    Debug.Log("not found instance");
                }
                //GameObject obj = new GameObject();
                //m_instance = obj.AddComponent(typeof(T)) as T;
            }
            return m_instance;
        }
    }
}
