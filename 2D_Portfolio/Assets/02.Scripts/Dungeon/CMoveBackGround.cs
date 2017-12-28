using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMoveBackGround : CInputMovement
{
    [SerializeField]
    private float m_h = 0;
    [SerializeField]
    private float m_bgMoveSpeed;
    
    public float m_defaultSpeed = 0;
	// Use this for initialization
	protected override void Start ()
    {
        m_rigidbody = this.GetComponent<Rigidbody2D>();
        m_mapMovement = GameObject.FindGameObjectWithTag("Village").GetComponent<CInputMovement>();

    }
	
	// Update is called once per frame
	void Update ()
    {
        InputMove();
        
    }


    void InputMove()
    {
        //base.InputMove();
        //m_h = m_mapMovement.h;

        Debug.Log("무브함수호출");
        Debug.Log("속도확인 : " + m_mapMovement.m_moveSpeed);


        m_rigidbody.velocity = new Vector2(m_mapMovement.Horizontal * m_bgMoveSpeed * -1, m_rigidbody.velocity.y);
        if (m_mapMovement.m_isSideColCheck == true)
        {
            Debug.Log("충돌체크 ");

            m_bgMoveSpeed = m_mapMovement.m_moveSpeed;

            Debug.Log("속도확인 : " + m_mapMovement.m_moveSpeed);
        }
        else
        {
            m_bgMoveSpeed = m_defaultSpeed;
        }

        
    }
}
