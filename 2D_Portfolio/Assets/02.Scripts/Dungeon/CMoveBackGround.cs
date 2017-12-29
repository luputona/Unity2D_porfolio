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

    [SerializeField]
    private Vector2 m_savedOffset;
    [SerializeField]
    private Renderer m_renderer;

    public float m_offset;

    public Vector2 m_vector2;


    // Use this for initialization
    new void Start ()
    {
        m_rigidbody = this.GetComponent<Rigidbody2D>();
        m_mapMovement = GameObject.FindGameObjectWithTag("Village").GetComponent<CInputMovement>();

        m_renderer = this.GetComponent<Renderer>();

        m_savedOffset = m_renderer.sharedMaterial.GetTextureOffset("_MainTex");

        m_vector2 = new Vector2();
        m_vector2.y = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        MoveBackGroundOffset();

    }

    private void OnDisable()
    {
        m_renderer.sharedMaterial.SetTextureOffset("_MainTex", m_savedOffset);
    }

    void MoveBackGroundOffset()
    {
        m_offset += Time.deltaTime * m_bgMoveSpeed * m_mapMovement.Horizontal;
        m_vector2.x = m_offset;

        m_renderer.material.mainTextureOffset = m_vector2;

        Debug.Log(m_mapMovement.Horizontal);
        
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


    // 오브젝트를 직접 제어 
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
