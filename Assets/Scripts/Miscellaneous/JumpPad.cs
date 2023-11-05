using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Transform directionTransform;
    public float padForce;
    private void OnCollisionEnter(Collision collision)    
    {
        if (collision.rigidbody)
        {
            print("Jump pad!!");
            collision.rigidbody.AddForce(directionTransform.up * padForce);
        }
    }
}
