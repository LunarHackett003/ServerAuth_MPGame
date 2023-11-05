using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eclipse.Character
{
    public class CharacterMotor : MonoBehaviour
    {
        [SerializeField] protected CapsuleCollider capsule;
        [SerializeField] Animator animator;
        public float stance;

        [SerializeField] protected float standingCapsuleHeight, crouchingCapsuleHeight;
        [SerializeField] protected float standingWalkSpeed, crouchingWalkSpeed;
        [SerializeField] protected float crouchStanceChangeTime;


        private void FixedUpdate()
        {
            stance = Mathf.Clamp01(stance);
            animator.SetFloat("Stance", stance);
            UpdateCapsuleCollider();
        }
        void UpdateCapsuleCollider()
        {
            if (!capsule)
                return;

            capsule.height = Mathf.Lerp(crouchingCapsuleHeight, standingCapsuleHeight, stance);
            capsule.center = new Vector3(0, capsule.height / 2, 0);
        }
    }
}