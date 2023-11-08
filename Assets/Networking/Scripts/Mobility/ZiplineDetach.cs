using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZiplineDetach : MonoBehaviour
{
    public float detachMultiplier;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out NetCharacterMotor_V2 ncm))
        {
            other.attachedRigidbody.AddForce(transform.forward * ncm.rappelDetachForce * detachMultiplier);
            ncm.DetachZipline();
        }
    }
}
