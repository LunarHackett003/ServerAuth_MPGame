using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientInput : NetworkBehaviour
{
    public Vector2 loc_moveInput, loc_lookInput;
    public bool loc_fireInput, loc_jumpInput;
    public Controls controls;
    public bool useThisAimTransform;
    public Transform pitchTransform,yawTransform;
    public float lookSpeed;
    public GameObject cam;
    public GameObject vmCam;
    public Vector3 aimSwaySpeed;
    public Vector3 aimSwayAmount, aimSwayDamp, aimSwayVelocity;
    public Vector3 aimTranslateSpeed, aimTranslateAmount, aimTranslateVelocity;
    public float aimSwaySmoothTime;
    public float aimTranslateSmoothTime;
    public float xAim, yAim;
    public float xAimDelta, yAimDelta;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            SubscribeInput();
        }
        else
        {
            //Not the owner, disable the cameras
            cam.SetActive(false);
            vmCam.SetActive(false);
        }
    }
    private void Start()
    {
        Camera.main.GetComponent<AudioListener>().enabled = false;   
    }
    void SubscribeInput()
    {
        controls = new();
        controls.Enable();
    }


    private void Update()
    {
        //You are not the owner; do not execute this code.
        if (!IsOwner)
            return;


        loc_moveInput = controls.Default.Move.ReadValue<Vector2>();
        loc_lookInput = controls.Default.Look.ReadValue<Vector2>();
        loc_jumpInput = controls.Default.Jump.ReadValue<float>() > 0.3f;
    }
    private void FixedUpdate()
    {
        if (useThisAimTransform)
        {
            float xAimOld = xAim;
            float yAimOld = yAim;


            yAim += lookSpeed * Time.fixedDeltaTime * -loc_lookInput.y;
            xAim += lookSpeed * Time.fixedDeltaTime * loc_lookInput.x;

            yAim.Clamp(-89, 89);

            pitchTransform.localRotation = Quaternion.Euler(new Vector3(yAim, 0, 0));
            yawTransform.localRotation = Quaternion.Euler(new Vector3(0, xAim, 0));

            xAimDelta = xAim - xAimOld;
            yAimDelta = yAim - yAimOld;
            Vector3 lookSwayTarget = new(-yAimDelta, xAimDelta, xAimDelta);
            lookSwayTarget.Scale(aimSwaySpeed);
            aimSwayAmount = Vector3.SmoothDamp(aimSwayAmount, lookSwayTarget, ref aimSwayVelocity, aimSwaySmoothTime);
            //vmCam.transform.localRotation = Quaternion.Euler(aimSwayAmount);
            Vector3 lookTranslateTarget = new(xAimDelta, -yAimDelta, xAimDelta);
            lookTranslateTarget.Scale(aimTranslateSpeed);
            aimTranslateAmount = Vector3.SmoothDamp(aimTranslateAmount, lookTranslateTarget, ref aimTranslateVelocity, aimTranslateSmoothTime);
            //vmCam.transform.localPosition = aimTranslateAmount;
        }
        else
        {

        }
    }
}
