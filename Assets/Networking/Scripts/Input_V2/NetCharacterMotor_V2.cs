using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetCharacterMotor_V2 : NetworkBehaviour
{
    public PlayerInputProxy input;
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


    [SerializeField] Transform yawTransform, pitchTransform;
    [SerializeField] float lookSpeed;
    [SerializeField] float groundedDrag, airborneDrag;

    [SerializeField] Vector3 aimSwayAmount, aimSwayTarget, aimTranslationAmount, aimTranslationTarget, aimSwayVelocity, aimTranslateVelocity, aimSwaySpeed, aimTranslateSpeed;
    [SerializeField] Vector2 aimTarg, aimDelta;
    [SerializeField] WeaponParameters wp;
    [SerializeField] Vector2 weaponBobTarget, weaponBobCurrent, weaponBobVelocity;
    [SerializeField] float currentBob;
    [SerializeField] bool bobRight;

    [SerializeField] float jumpVelocity;
    [SerializeField] float cameraYFollow, cameraYVelocity, cameraYTarget, cameraYAmount, cameraYSmoothTime;
    [SerializeField] Vector2 cameraYClamp;

    [SerializeField] float aimSwaySmoothTime, aimTranslateSmoothTime;

    [SerializeField] Transform vmCam;

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
    Vector2 lookInputCached, moveInputCached;
    [SerializeField] Vector3 rappelDirection;
    [SerializeField] float rappelSpeed;
    public float rappelDetachForce;

    [SerializeField] float collideImpulseMultipler;
    (bool jump, bool fire, bool sprint, bool crouch) boolInputs;
    [SerializeField] Transform interactPoint;
    [SerializeField] float interactDistance;
    [SerializeField] LayerMask interactableLayermask;
    [SerializeField] Zipline currentZipline;
    private void Start()
    {
        currentBob = 0.5f;
    }

    private void FixedUpdate()
    {
        //Neither the server nor host - do not execute
        if (!IsOwner)
            return;

        if (input)
        {
            lookInputCached = input.GetLookInput();
            moveInputCached = input.GetMoveInput();
            boolInputs = input.GetBooleanInputs();
        }


        grounded = GroundCheck();
        rb.isKinematic = !isAlive.Value && (moveState != MoveState.rappeling);
        if (rootObject.gameObject.activeSelf != isAlive.Value)
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
                Movement();
            }
            else
            {
                rb.drag = airborneDrag;
                jumped = false;
            }
            if (boolInputs.jump)
                jumped = false;

            if(moveState == MoveState.rappeling)
            {
                RappelMovement();
            }
        }
        else
        {
            if (boolInputs.jump && !attemptingRespawn)
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

        float xAimOld = aimTarg.x;
        float yAimOld = aimTarg.y;


        aimTarg.y += lookSpeed * Time.fixedDeltaTime * -lookInputCached.y;
        aimTarg.x += lookSpeed * Time.fixedDeltaTime * lookInputCached.x;

        aimTarg.y.Clamp(-89, 89);

        pitchTransform.localRotation = Quaternion.Euler(new Vector3(aimTarg.y, 0, 0));
        yawTransform.localRotation = Quaternion.Euler(new Vector3(0, aimTarg.x, 0));

        aimDelta.x = aimTarg.x - xAimOld;
        aimDelta.y = aimTarg.y - yAimOld;
        Vector3 lookSwayTarget = new(-aimDelta.y, aimDelta.x, aimDelta.x);
        lookSwayTarget.Scale(aimSwaySpeed);
        aimSwayAmount = Vector3.SmoothDamp(aimSwayAmount, lookSwayTarget, ref aimSwayVelocity, aimSwaySmoothTime);
        //vmCam.transform.localRotation = Quaternion.Euler(aimSwayAmount);
        Vector3 lookTranslateTarget = new(aimDelta.x, -aimDelta.y, aimDelta.x);
        lookTranslateTarget.Scale(aimTranslateSpeed);
        aimTranslationAmount = Vector3.SmoothDamp(aimTranslationAmount, lookTranslateTarget, ref aimTranslateVelocity, aimTranslateSmoothTime);
        //vmCam.transform.localPosition = aimTranslateAmount;

        weaponBobTarget = new Vector2(Mathf.Lerp(-wp.swayExtents.x, wp.swayExtents.x, wp.xBobCurve.Evaluate(currentBob)),
            Mathf.Lerp(-wp.swayExtents.y, wp.swayExtents.y, wp.yBobCurve.Evaluate(currentBob)));
        weaponBobCurrent = Vector2.SmoothDamp(weaponBobCurrent, weaponBobTarget * moveInputCached.sqrMagnitude, ref weaponBobVelocity, wp.weaponBobDampTime);

        cameraYTarget = rb.velocity.y * cameraYFollow;
        cameraYTarget.Clamp(cameraYClamp.x, cameraYClamp.y);
        cameraYAmount = Mathf.SmoothDamp(cameraYAmount, cameraYTarget, ref cameraYVelocity, cameraYSmoothTime);

        Vector3 viewRotComp = aimSwayAmount;
        Vector3 viewPosComp = aimTranslationAmount + (Vector3)weaponBobCurrent + Vector3.up * cameraYAmount;

        vmCam.transform.localRotation = Quaternion.Euler(aimSwayAmount);
        vmCam.transform.localPosition = viewPosComp;

    }
    void Movement()
    {
        rb.drag = groundedDrag;
        if (rb && input)
        {
            Vector2 clampedInput = new Vector2(Mathf.Clamp(moveInputCached.x, -1, 1), Mathf.Clamp(moveInputCached.y, -1, 1));
            Vector3 force = yawTransform.rotation * new Vector3(clampedInput.x, 0, clampedInput.y) * moveForce;
            Vector3 projectedMoveForce = Vector3.ProjectOnPlane(force, groundNormal);
            rb.AddForce(projectedMoveForce);


            if (boolInputs.jump && !jumped)
            {
                rb.AddForce(jumpVelocity * Vector3.up, ForceMode.VelocityChange);
                jumped = true;
            }
        }
    }
    void RappelMovement()
    {
        rb.velocity = rappelDirection * rappelSpeed;
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
        currentBob += Time.fixedDeltaTime * moveInputCached.sqrMagnitude * wp.bobSpeed * (bobRight ? 1 : -1);
        if (currentBob >= 1)
        {
            bobRight = false;
        }
        else if (currentBob <= 0)
        {
            bobRight = true;
        }
    }

    void CheckZipline()
    {
        Ray r = new()
        {
            direction = interactPoint.forward,
            origin = interactPoint.position
        };
        if (Physics.Raycast(r, out RaycastHit info, interactDistance, interactableLayermask, QueryTriggerInteraction.Collide))
        {
            if (info.collider.GetComponentInParent<Zipline>())
            {
                var zip = info.collider.GetComponentInParent<Zipline>();
                var zipDir = zip.GetZiplineVector(r.direction);
                rappelDirection = zipDir.rappelDirection;
                rappelSpeed = zipDir.rappelSpeed;
                AttachToZipline();
                currentZipline = zip;
            }
        }
    }

    public void AttachToZipline()
    {
        onRappel = true;
    }
    public void DetachZipline()
    {
        onRappel = false;
        rb.isKinematic = false;
        currentZipline = null;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(onRappel)
        DetachZipline();
    }
    public void Interact()
    {
        Debug.DrawRay(interactPoint.position, interactPoint.forward * interactDistance);
        CheckZipline();
    }

}