using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponAnimationManager : NetworkBehaviour
{
    [SerializeField] Animator animator_WM, animator_VM;
    AnimatorOverrideController aoc;
    [SerializeField] LoadoutHolder lh;
    [SerializeField] bool gadgetPending;

    public void SendWeaponSwitch()
    {
        if (gadgetPending)
        {
            lh.SwitchToGadget();
        }
        else
        {
            lh.SwitchWeapon();
        }
        lh.UpdateWeapons();
        gadgetPending = false;
    }
    public void SetGadgetPending()
    {
        gadgetPending = true;
    }
    protected AnimationClipOverrides clipOverrides;
    public void UpdateWeaponAnimations()
    {
        if (!aoc)
        {
            aoc = new(animator_VM.runtimeAnimatorController);
            animator_VM.runtimeAnimatorController = aoc;
            animator_WM.runtimeAnimatorController = aoc;

            clipOverrides = new(aoc.overridesCount);
            aoc.GetOverrides(clipOverrides);
        }


        foreach (var item in lh.GetCurrentWeapon().animSet.overrideTargets)
        {
            clipOverrides[item.overrideName] = item.newClip;
        }
        aoc.ApplyOverrides(clipOverrides);
    }
}
