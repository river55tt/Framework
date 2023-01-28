using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }



    private void OnDrawGizmos()
    {
        //Debug.DrawLine(transform.position, transform.position + transform.up * -50, Color.blue);
    }
}
