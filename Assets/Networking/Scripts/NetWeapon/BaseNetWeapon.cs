using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseNetWeapon : NetworkBehaviour
{
    public Transform magazine;
    public WeaponAnimationContainer animSet;
}
