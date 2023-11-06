using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Eclipse/Scriptables/Animation Container")]
public class WeaponAnimationContainer : ScriptableObject
{
    public List<OverrideClipTarget> overrideTargets;
}
[System.Serializable]
public class OverrideClipTarget
{
    public AnimationClip newClip;
    public string overrideName;
}
