using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSideCollCheck : MonoBehaviour
{    
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            //TODO : 충돌하면 이동 애니메이션을 중지하고 아이들로 변경하는 로직 구현
            //Debug.Log("PlayerColl");
        }
    }
}
