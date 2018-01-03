using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CnControls;

public class CInputMovement : CMovement
{
    [SerializeField]
    private CFollowCamera m_mainCamera;

    [SerializeField]
    protected GameObject m_mapObj;
    [SerializeField]
    protected CInputMovement m_mapMovement;   
    [SerializeField]
    protected Rigidbody2D m_rigidbody;   
    [SerializeField]
    protected float h = 0.0f;

    [SerializeField]
    private Transform m_fakePlayerPos;
    [SerializeField]
    private Transform m_fakeDefaultPos;
    [SerializeField]
    private float m_fakeFlip; // 페이크 오브젝트의 이동 좌표 리미트 
    [SerializeField]
    private float m_fakeSpeed = 0.1f;

    public bool m_isSideColCheck;
    public float m_moveSpeed;
    public float m_defaultMoveSpeed;
    public Vector2 m_fakePosX; // 페이크 오브젝트의 이동 좌표 제어

    public float FakeFlip
    {
        get
        {
            return m_fakeFlip;
        }
        set
        {
            m_fakeFlip = value;
        }
    }

    public float Horizontal
    {
        get
        {
            return h;
        }
        set
        {
            h = value;
        }     
    }

    protected override void Awake()
    {
        base.Awake();
        m_mapMovement = GetComponent<CInputMovement>();
        m_mapObj = this.gameObject;        

        m_rigidbody = GetComponent<Rigidbody2D>();
        m_characterCtrl = GameObject.FindWithTag("Player").GetComponent<CCharacterMoveCtrl>();
        m_mainCamera = Camera.main.GetComponent<CFollowCamera>();

        m_fakePlayerPos = m_characterCtrl.transform.Find("FakePosition").GetComponent<Transform>();
        m_fakeDefaultPos = m_fakePlayerPos;
        m_moveSpeed = m_defaultMoveSpeed;
    }

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
        m_isSideColCheck = false;
        m_mainCamera.Init(m_fakePlayerPos);
       
    }

	
	// Update is called once per frame
	void Update ()
    {
        CStatus.GetInstance.TEST();
        InputMove();
        
	}

    void InputMove()
    {        
        h = Input.GetAxis("Horizontal");

        if (h == 0.0f)
        {
            //TODO : 터치 컨트롤러 추가 
            h = CnInputManager.GetAxis("Horizontal");
        }

        if(m_characterCtrl.m_isRightDir && h < 0.0f || (!m_characterCtrl.m_isRightDir && h > 0.0f))
        {
            m_characterCtrl.Flip();
        }
        
        m_rigidbody.velocity = new Vector2(h * m_moveSpeed * -1, m_rigidbody.velocity.y);


        //카메라 제어
        if (h < 0.0f ) //왼쪽
        {
            m_fakeFlip = 0.5f; 
            m_fakePosX = new Vector2(m_fakePlayerPos.position.x + m_fakeFlip, m_fakePlayerPos.position.y);
            if (m_fakePlayerPos.position.x >= 0.6f)
            {
                m_fakeFlip = 0;
            }
            else
            {
                m_fakePlayerPos.position = Vector3.Lerp(m_fakePlayerPos.position, m_fakePosX, m_fakeSpeed * Time.deltaTime);
            }
        }
        else if(h > 0.0f ) //오른쪽
        {
            m_fakeFlip = -0.5f;
            m_fakePosX = new Vector2(m_fakePlayerPos.position.x + m_fakeFlip, m_fakePlayerPos.position.y);
            if (m_fakePlayerPos.position.x <= -0.6f)
            {
                m_fakeFlip = 0;
            }
            else
            {
                m_fakePlayerPos.position = Vector3.Lerp(m_fakePlayerPos.position, m_fakePosX, m_fakeSpeed * Time.deltaTime);
            }

        }
        if( h == 0.0f )
        {
            m_fakePlayerPos.position = new Vector2(0.0f, m_fakeDefaultPos.position.y);
            m_fakePosX = new Vector2(0.0f, m_fakePlayerPos.position.y);
            //m_characterCtrl.gameObject.transform.position = new Vector2(0.0f, 0.0f);
        }
        else if( h == 0.0f && m_isSideColCheck == true)
        {
            m_characterCtrl.gameObject.transform.position = new Vector2(0.0f, 0.0f);// 충돌로 인한 약간의 좌표가 어긋나는 것을 초기화로 잡아줌
        }
    }
    
   
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            //TODO : 
            m_characterCtrl.gameObject.transform.position = new Vector2(0.0f, 0.0f);
            Debug.Log("PlayerColl");
        }
    }
}
