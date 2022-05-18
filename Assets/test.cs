using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Debug.Log(Physics2D.OverlapCircle(transform.position, 3));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 3);
    }
}
