using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.Netcode;
using UnityEngine;

public class NetCharacterMotor : NetworkBehaviour
{
    
    public ClientInput input;
    public Rigidbody rb;
    public float dragForce = 2;
    public float moveForce = 10;
    public NetworkVariable<bool> isAlive = new();
    public Transform rootObject;
    bool attemptingRespawn;
    Vector3 groundNormal;
    [SerializeField] bool grounded;
    [SerializeField] float groundCheckDistance, groundCheckRadius;
    [SerializeField] float walkableGroundDotThreshold;
    [SerializeField] LayerMask groundCheckMask;

    [SerializeField] float groundedDrag, airborneDrag;

    [SerializeField] WeaponParameters wp;
    [SerializeField] Vector2 weaponBobTarget, weaponBobCurrent, weaponBobVelocity;
    [SerializeField] float currentBob;
    [SerializeField] bool bobRight;

    [SerializeField] float jumpVelocity;
    [SerializeField] float cameraYFollow, cameraYVelocity, cameraYTarget, cameraYAmount, cameraYSmoothTime;
    [SerializeField] Vector2 cameraYClamp;


    public enum MoveState
    {
        /// <summary>
        /// The player is currently on the ground
        /// </summary>
        grounded = 0,
        /// <summary>
        /// The player is currently in the air
        /// </summary>
        airborne = 1,
        /// <summary>
        /// Player is currently on a rappel
        /// </summary>
        rappeling = 2,
        /// <summary>
        /// Player is currently sliding
        /// </summary>
        sliding = 4,
        /// <summary>
        /// Player is currently grappling
        /// </summary>
        grappling = 8
    }
    public MoveState moveState;
    [SerializeField] bool sliding;
    [SerializeField] bool onRappel, onGrapple;
    bool jumped;
    private void Start()
    {
        currentBob = 0.5f;
    }

    private void FixedUpdate()
    {
        //Neither the server nor host - do not execute
        if (!IsOwner)
            return;
        grounded = GroundCheck();
        rb.isKinematic = !isAlive.Value;
        if(rootObject.gameObject.activeSelf != isAlive.Value)
        {
            ChangePlayerAliveStateServerRPC(isAlive.Value);
        }
        if (isAlive.Value)
        {
            if (onGrapple)
                moveState = MoveState.grappling;
            else if (onRappel)
                moveState = MoveState.rappeling;
            else if (!grounded)
                moveState = MoveState.airborne;
            else if (sliding)
                moveState = MoveState.sliding;
            else if (grounded)
                moveState = MoveState.grounded;


            rb.useGravity = !(moveState == MoveState.rappeling || moveState == MoveState.grappling);

            ViewDynamics();

            if (moveState == MoveState.grounded)
            {
                rb.drag = groundedDrag;
                if (rb && input)
                {
                    Vector2 clampedInput = new Vector2(Mathf.Clamp(input.loc_moveInput.x, -1, 1), Mathf.Clamp(input.loc_moveInput.y, -1, 1));
                    Vector3 force = input.yawTransform.rotation * new Vector3(clampedInput.x, 0, clampedInput.y) * moveForce;
                    Vector3 projectedMoveForce = Vector3.ProjectOnPlane(force, groundNormal);
                    rb.AddForce(projectedMoveForce);


                    if (input.loc_jumpInput && !jumped)
                    {
                        rb.AddForce(jumpVelocity * Vector3.up, ForceMode.VelocityChange);
                        jumped = true;
                    }
                }
            }
            else
            {
                rb.drag = airborneDrag;
                jumped = false;
            }
            if (!input.loc_jumpInput)
                jumped = false;
        }
        else
        {
            if (input.loc_jumpInput && !attemptingRespawn)
            {
                attemptingRespawn = true;
                ChangePlayerAliveStateServerRPC(true);
            }
        }
    }
    /// <summary>
    /// Calculates and composites the rotation and translation 
    /// </summary>
    void ViewDynamics()
    {
        OscillateBobAmount();


        weaponBobTarget = new Vector2(Mathf.Lerp(-wp.swayExtents.x, wp.swayExtents.x, wp.xBobCurve.Evaluate(currentBob)),
            Mathf.Lerp(-wp.swayExtents.y, wp.swayExtents.y, wp.yBobCurve.Evaluate(currentBob)));
        weaponBobCurrent = Vector2.SmoothDamp(weaponBobCurrent, weaponBobTarget * input.loc_moveInput.sqrMagnitude, ref weaponBobVelocity, wp.weaponBobDampTime);

        cameraYTarget = rb.velocity.y * cameraYFollow;
        cameraYTarget.Clamp(cameraYClamp.x, cameraYClamp.y);
        cameraYAmount = Mathf.SmoothDamp(cameraYAmount, cameraYTarget, ref cameraYVelocity, cameraYSmoothTime);

        Vector3 viewRotComp = input.aimSwayAmount;
        Vector3 viewPosComp = input.aimTranslateAmount + (Vector3)weaponBobCurrent + Vector3.up * cameraYAmount;

        input.vmCam.transform.localRotation = Quaternion.Euler(input.aimSwayAmount);
        input.vmCam.transform.localPosition = viewPosComp;

    }

    [ServerRpc]
    private void ChangePlayerAliveStateServerRPC(bool state)
    {
        isAlive.Value = state;
        ReplicateAliveStateClientRPC(state);
    }
    [ClientRpc]
    public void ReplicateAliveStateClientRPC(bool state)
    {
        rootObject.gameObject.SetActive(state);
    }
    bool GroundCheck()
    {
        Ray r = new(transform.position + (Vector3.up * (groundCheckRadius * 1.1f)), Vector3.down);
        if (Physics.SphereCast(r, groundCheckRadius, out RaycastHit hitinfo, groundCheckDistance, groundCheckMask))
        {
            float groundDot = Vector3.Dot(hitinfo.normal, transform.up);
            if (groundDot > walkableGroundDotThreshold)
            {
                //Ground is hit and walkable
                groundNormal = hitinfo.normal;
                return true;
            }
            else
            {
                //Ground is hit but not walkable
                return false;
            }
        }
        else
        {
            //Ground is not hit.
            return false;
        }
    }
    
    void OscillateBobAmount()
    {
        currentBob +=  Time.fixedDeltaTime * input.loc_moveInput.sqrMagnitude * wp.bobSpeed * (bobRight ? 1 : -1);
        if(currentBob >= 1)
        {
            bobRight = false;
        }
        else if(currentBob <= 0)
        {
            bobRight = true;
        }
    }
}
