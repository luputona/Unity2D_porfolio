using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftSideCamCheck : MonoBehaviour
{
    [SerializeField]
    private CInputMovement m_inputMovement;

    private void Awake()
    {
        m_inputMovement = GameObject.FindGameObjectWithTag("Village").GetComponent<CInputMovement>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag.Equals("Player"))
        {
            m_inputMovement.FakeFlip = 0.0f;
            m_inputMovement.m_moveSpeed = 0;
            m_inputMovement.m_isSideColCheck = true;
        }
        if (other.tag.Equals("Player") && m_inputMovement.Horizontal < 0)
        {
            m_inputMovement.m_moveSpeed = 0;
            m_inputMovement.m_isSideColCheck = true;
        }
        else if (other.tag.Equals("Player") && m_inputMovement.Horizontal > 0)
        {
            m_inputMovement.m_moveSpeed = m_inputMovement.m_defaultMoveSpeed;
            m_inputMovement.m_isSideColCheck = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag.Equals("Player"))
        {
            m_inputMovement.m_isSideColCheck = false;
        }

    }
}
