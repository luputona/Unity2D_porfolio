using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFollowCamera : MonoBehaviour
{
    //추적대상 - m_fakePlayerPos : CInputMovement 에 있음
    public Transform m_target;
    //부드러운 정도
    public float m_smoothing;
    //간격 
    public Vector3 m_offset;

    public void Init(Transform target)
    {
        m_target = target;

        transform.position = m_target.position + m_offset;
    }

	// Update is called once per frame
	void LateUpdate ()
    {       

        if (m_target == null)
        {
            return;
        }
        Vector3 targetCamPos = m_target.position + m_offset;
        transform.position = Vector3.Lerp(transform.position, targetCamPos, m_smoothing * Time.deltaTime);
	}
}
