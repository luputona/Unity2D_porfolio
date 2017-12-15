using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CnControls;

public class CInputMovement : CMovement
{
    [SerializeField]
    private GameObject m_mapObj;
    [SerializeField]
    private CInputMovement m_mapMovement;   
    [SerializeField]
    private Rigidbody2D m_rigidbody;
    [SerializeField]
    private CFollowCamera m_mainCamera;
    [SerializeField]
    private float h = 0.0f;

    [SerializeField]
    private Transform m_fakePlayerPos;
    [SerializeField]
    private Transform m_fakeDefaultPos;
    [SerializeField]
    private float m_fakeFlip;
    [SerializeField]
    private float m_fakeSpeed = 0.1f;

    public bool m_isSideColCheck;
    public float m_moveSpeed;
    public Vector2 m_fakePosX;

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

        if (h < 0.0f )
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
        else if(h > 0.0f )
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
            m_characterCtrl.gameObject.transform.position = new Vector2(0.0f, 0.0f);
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
