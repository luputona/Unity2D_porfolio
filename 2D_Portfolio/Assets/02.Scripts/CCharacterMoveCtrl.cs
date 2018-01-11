using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterMoveCtrl : MonoBehaviour
{
    private static CResourceManager _instance = null;
    public bool m_isRightDir;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    


    public void Flip()
    {
        Vector2 scale = transform.localScale;
        scale.x *= -1;

        transform.localScale = scale;
        m_isRightDir = !m_isRightDir;
    }
    

    
}
