using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Eclipse/Scriptables/Weapon List")]
public class WeaponListScriptable : ScriptableObject
{
    public List<WeaponInfo> primaryWeaponList;
    public List<WeaponInfo> secondaryWeaponList;
    public List<WeaponInfo> gadgetList;
}

[System.Serializable]
public class WeaponInfo
{
    public string name = "weapon";
    public string description = "its a weapon!";
    public GameObject weapon;
    public Sprite icon;
}
