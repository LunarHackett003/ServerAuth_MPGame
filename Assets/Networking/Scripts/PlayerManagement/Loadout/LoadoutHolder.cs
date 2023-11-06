using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoadoutHolder : NetworkBehaviour
{
    public enum CurrentWeapon
    {
        primary = 0,
        secondary = 1,
        gadget = 2
    }
    public CurrentWeapon weapon;
    CurrentWeapon weaponbeforegadget;
    public Transform weaponPoint, weaponPoint_VM;
    public GameObject primaryGO, secondaryGO, gadgetGO;
    public GameObject primaryGO_VM, secondaryGO_VM, gadgetGO_VM;

    public NetworkVariable<int> primaryIndex = new(writePerm: NetworkVariableWritePermission.Owner), 
        secondaryIndex = new(writePerm: NetworkVariableWritePermission.Owner), 
        gadgetIndex = new(writePerm: NetworkVariableWritePermission.Owner);
    private void Start()
    {
        if (IsOwner)
        {
            primaryIndex.Value = LoadoutSelector.instance.primIndex;
            secondaryIndex.Value = LoadoutSelector.instance.secIndex;
            gadgetIndex.Value = LoadoutSelector.instance.gadgetIndex;
        }

        primaryGO = Instantiate(LoadoutSelector.instance.weaponList.primaryWeaponList[primaryIndex.Value].weapon, weaponPoint, false);
        primaryGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        secondaryGO = Instantiate(LoadoutSelector.instance.weaponList.secondaryWeaponList[secondaryIndex.Value].weapon, weaponPoint, false);
        secondaryGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        gadgetGO = Instantiate(LoadoutSelector.instance.weaponList.gadgetList[primaryIndex.Value].weapon, weaponPoint, false);
        gadgetGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        primaryGO.name = LoadoutSelector.instance.weaponList.primaryWeaponList[primaryIndex.Value].weapon.name;
        secondaryGO.name = LoadoutSelector.instance.weaponList.secondaryWeaponList[secondaryIndex.Value].weapon.name;
        gadgetGO.name = LoadoutSelector.instance.weaponList.gadgetList[gadgetIndex.Value].weapon.name;

        if (IsOwner)
        {
            primaryGO_VM = Instantiate(LoadoutSelector.instance.weaponList.primaryWeaponList[primaryIndex.Value].weapon, weaponPoint_VM, false);
            primaryGO_VM.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            primaryGO_VM.name = LoadoutSelector.instance.weaponList.primaryWeaponList[primaryIndex.Value].weapon.name;
            secondaryGO_VM = Instantiate(LoadoutSelector.instance.weaponList.secondaryWeaponList[secondaryIndex.Value].weapon, weaponPoint_VM, false);
            secondaryGO_VM.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            secondaryGO_VM.name = LoadoutSelector.instance.weaponList.secondaryWeaponList[secondaryIndex.Value].weapon.name;
            gadgetGO_VM = Instantiate(LoadoutSelector.instance.weaponList.gadgetList[primaryIndex.Value].weapon, weaponPoint_VM, false);
            gadgetGO_VM.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gadgetGO_VM.name = LoadoutSelector.instance.weaponList.gadgetList[gadgetIndex.Value].weapon.name;
        }

        GetComponent<LocalLayerCorrection>().FixThisShit();
        UpdateWeapons();
    }
    public void SetPrimary()
    {
        bool setactive = weapon == CurrentWeapon.primary;
        if (primaryGO_VM)
            primaryGO_VM.SetActive(setactive);
        if(primaryGO)
            primaryGO.SetActive(setactive);
    }
    public void SetSecondary()
    {
        bool setactive = weapon == CurrentWeapon.secondary;
        if(secondaryGO_VM)
            secondaryGO_VM.SetActive(setactive);
        if(secondaryGO)
            secondaryGO.SetActive(setactive);
    }
    public void SetGadget()
    {
        bool setactive = weapon == CurrentWeapon.gadget;
        if(gadgetGO_VM)
            gadgetGO_VM.SetActive(setactive);
        if(gadgetGO)
            gadgetGO.SetActive(setactive);
    }
    public void SwitchWeapon()
    {
        if(weapon == CurrentWeapon.gadget)
        {
            weapon = weaponbeforegadget;
        }
        else
        {
            weapon = (weapon == CurrentWeapon.primary) ? CurrentWeapon.secondary : CurrentWeapon.primary;
        }
    }
    public void SwitchToGadget()
    {
        weaponbeforegadget = weapon;
        weapon = CurrentWeapon.gadget;
    }
    public void UpdateWeapons()
    {
        SetPrimary();
        SetSecondary();
        SetGadget();

        GetComponentInChildren<WeaponAnimationManager>().UpdateWeaponAnimations();
    }
    public BaseNetWeapon GetCurrentWeapon()
    {
        switch (weapon)
        {
            case CurrentWeapon.primary:
                return primaryGO.GetComponent<BaseNetWeapon>();
            case CurrentWeapon.secondary:
                return secondaryGO.GetComponent<BaseNetWeapon>();
            case CurrentWeapon.gadget:
                return gadgetGO.GetComponent<BaseNetWeapon>();
            default:
                return null;
        }
    }
}
