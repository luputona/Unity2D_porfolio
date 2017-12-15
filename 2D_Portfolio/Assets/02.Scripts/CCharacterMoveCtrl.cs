using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterMoveCtrl : MonoBehaviour
{    
    public bool m_isRightDir;


	// Use this for initialization
	protected void Start ()
    {
		
	}

    // Update is called once per frame
    protected void Update ()
    {
		
	}

    public void Flip()
    {
        Vector2 scale = transform.localScale;
        scale.x *= -1;

        transform.localScale = scale;
        m_isRightDir = !m_isRightDir;
    }
    

    
}
