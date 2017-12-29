using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public float m_speed = 2.0f;
    private float m_offset;

    private Vector2 m_vector2;
    private Renderer m_renderer;


    // Use this for initialization
    void Start()
    {
        m_renderer = this.GetComponent<Renderer>();
        m_vector2 = new Vector2();
        m_vector2.y = 0.0f;

    }

    void LateUpdate()
    {
        m_offset += Time.deltaTime * m_speed;
        m_vector2.x = m_offset;
        m_renderer.material.mainTextureOffset = m_vector2;
    }
}
