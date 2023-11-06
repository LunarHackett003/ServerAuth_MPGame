using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutSelector : MonoBehaviour
{
    public GameObject primaryWeapon, secondaryWeapon, gadget;
    public static LoadoutSelector instance;
    public enum Slot
    {
        primary = 0, secondary = 1, gadget = 2
    }

    public WeaponListScriptable weaponList;
    public int primIndex, secIndex, gadgetIndex;
    public TextMeshProUGUI weaponPrimName, weaponPrimDesc, weaponSecName, weaponSecDesc, gadgetName, gadgetDesc;
    public Image weaponPrimIcon, weaponSecIcon, gadgetIcon;
    private void Start()
    {
        instance = this;

        UpdatePrimaryWeapon();
        UpdateSecondaryWeapon();
        UpdateGadget();
    }
    public void UpdatePrimaryWeapon()
    {
        WeaponInfo wi = weaponList.primaryWeaponList[primIndex];

        primaryWeapon = wi.weapon;
        weaponPrimName.text = wi.name;
        weaponPrimDesc.text = wi.description;
        if(wi.icon)
            weaponPrimIcon.sprite = wi.icon;
    }
    public void UpdateSecondaryWeapon()
    {
        WeaponInfo wi = weaponList.secondaryWeaponList[secIndex];

        secondaryWeapon = wi.weapon;
        weaponSecName.text = wi.name;
        weaponSecDesc.text = wi.description;
        if (wi.icon)
            weaponSecIcon.sprite = wi.icon;
    }
    public void UpdateGadget()
    {
        WeaponInfo wi = weaponList.gadgetList[gadgetIndex];

        gadget = wi.weapon;
        gadgetName.text = wi.name;
        gadgetDesc.text = wi.description;
        if(wi.icon)
            gadgetIcon.sprite = wi.icon;
    }
    public void IncrememntWeaponList(int slot)
    {
        Slot _slot = (Slot)slot;

        switch (_slot)
        {
            case Slot.primary:
                IncrementIndexAndModulo(ref primIndex, weaponList.primaryWeaponList.Count, 1);
                break;
            case Slot.secondary:
                IncrementIndexAndModulo(ref secIndex, weaponList.secondaryWeaponList.Count, 1);
                break;
            case Slot.gadget:
                IncrementIndexAndModulo(ref gadgetIndex, weaponList.gadgetList.Count, 1);
                break;
            default:
                break;
        }
        UpdatePrimaryWeapon();
        UpdateSecondaryWeapon();
        UpdateGadget();
    }
    public void DecrementWeaponList(int slot)
    {
        Slot _slot = (Slot)slot;
        switch (_slot)
        {
            case Slot.primary:
                IncrementIndexAndModulo(ref primIndex, weaponList.primaryWeaponList.Count, -1);
                break;
            case Slot.secondary:
                IncrementIndexAndModulo(ref secIndex, weaponList.secondaryWeaponList.Count, -1);
                break;
            case Slot.gadget:
                IncrementIndexAndModulo(ref gadgetIndex, weaponList.gadgetList.Count, -1);
                break;
            default:
                break;
        }
        UpdatePrimaryWeapon();
        UpdateSecondaryWeapon();
        UpdateGadget();
    }
    public void IncrementIndexAndModulo(ref int index, int modulus, int increment)
    {
        index += increment;
        index %= modulus;
    }
    public void SpawnPlayer()
    {
        PlayerInputProxy.instance.RequestPlayerCharacter();
    }
}
