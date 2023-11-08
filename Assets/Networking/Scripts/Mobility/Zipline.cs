using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    [SerializeField] Transform ziplineForwardDirection;
    [SerializeField] float ziplineSpeed;

    public (Vector3 rappelDirection, float rappelSpeed) GetZiplineVector(Vector3 lookDirection)
    {
        if (Vector3.Dot(ziplineForwardDirection.up, lookDirection) > Vector3.Dot(-ziplineForwardDirection.up, lookDirection))
        {
            return (ziplineForwardDirection.up, ziplineSpeed);
        }
        else
        {
            return (-ziplineForwardDirection.up, ziplineSpeed);
        }
    }

}
