using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponParameters
{
    public float moveSpeedMultiplier;
    public Vector2 swayExtents;
    public AnimationCurve xBobCurve, yBobCurve;
    public float weaponBobDampTime;
    public float bobSpeed;
}
