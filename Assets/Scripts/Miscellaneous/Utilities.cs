using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Utilities
{
    public static void Clamp(ref this float fl, float min, float max)
    {
        fl = Mathf.Clamp(fl, min, max);
    }
}
