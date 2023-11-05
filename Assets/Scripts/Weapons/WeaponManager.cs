using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eclipse.Weapons
{
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] protected List<BaseWeapon> weapons;
        [SerializeField] protected int weaponIndex;

        public BaseWeapon GetCurrentWeapon()
        {
            return weapons[weaponIndex];
        }
    }
}