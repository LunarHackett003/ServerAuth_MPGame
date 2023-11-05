using Eclipse.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eclipse
{
    public class WeaponAnimationHelper : MonoBehaviour
    {
        public Transform leftHand, rightHand;
        public string magFromGunName, magFromPouchName;
        public Weapons.WeaponManager wm;
        
        public void GrabMagazine(Transform hand, string objectName, bool fromPouch)
        {
            BaseFirearm bw = wm.GetCurrentWeapon() as BaseFirearm;
            if(bw != null)
            {
                GameObject mag = Instantiate(bw.magazine.gameObject, bw.magazine.position, bw.magazine.rotation, null);
                mag.transform.SetParent(hand, true);
                mag.transform.localScale = bw.magazine.lossyScale;
                mag.name = objectName;
                mag.SetActive(true);
            }
        }
        public void DestroyMagazines(Transform hand, string magName)
        {
            for (int i = 0; i < hand.childCount; i++)
            {
                if(hand.GetChild(i).name.Contains(magName))
                    Destroy(hand.GetChild(i).gameObject, 0.005f);
            }
        }
        
        public void LeftGrabFromGun()
        {
            GrabMagazine(leftHand, magFromGunName, false);
        }
        public void RightGrabFromGun()
        {
            GrabMagazine(rightHand, magFromGunName, false);
        }
        public void LeftDestroyFromGun()
        {
            DestroyMagazines(leftHand, magFromGunName);
        }
        public void RightDestroyFromGun()
        {
            DestroyMagazines(rightHand, magFromGunName);
        }
    }
}