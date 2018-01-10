using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterMachine : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator = null;

    void Awake()
    {
        m_animator = this.GetComponent<Animator>();
    }
    // Use this for initialization
    void Start ()
    {
        
	}
	
	
}
