using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMovement : MonoBehaviour
{
    protected CCharacterMoveCtrl m_characterCtrl;

    protected virtual void Awake()
    {
        m_characterCtrl = GetComponent<CCharacterMoveCtrl>();
    }

    protected virtual void Start()
    {
        
    }

    public void Flip()
    {
        

        m_characterCtrl.Flip();
    }


}
