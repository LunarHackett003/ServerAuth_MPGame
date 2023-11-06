using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputProxy : NetworkBehaviour
{
    public static PlayerInputProxy instance;
    [SerializeField] NetCharacterMotor_V2 ncm2;
    public bool characterRequested;
    public DefaultCharacterScriptable dcs;
    [SerializeField] Vector2 lookInput, moveInput;
    [SerializeField] bool jumpInput, fireInput, sprintInput, crouchInput;
    [SerializeField] Controls controls;
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
    public Vector2 GetLookInput()
    {
        return lookInput;
    }
    public (bool jump, bool fire, bool sprint, bool crouch) GetBooleanInputs()
    {
        return (jumpInput, fireInput, sprintInput, crouchInput);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            instance = this;
            SubscribeInput();
        }
    }
    private void OnEnable()
    {
        if (!IsOwner)
            return;

       controls = new();
       controls.Enable();
    }
    private void OnDisable()
    {
        if (!IsOwner)
            return;
        controls.Disable();
        controls.Dispose();
    }
    public void SubscribeInput()
    {

        controls = new();
        controls.Enable();
    }

    private void Update()
    {
        if (!IsOwner || !ncm2)
            return;
        
        moveInput = controls.Default.Move.ReadValue<Vector2>();
        lookInput = controls.Default.Look.ReadValue<Vector2>();
        jumpInput = controls.Default.Jump.ReadValue<float>() > 0.3f;
    }

    public void RequestPlayerCharacter()
    {
        if (IsOwner && !characterRequested)
        {
            RequestPlayerServerRPC();
            characterRequested = true;
        }
    }
    [ServerRpc()]
    public void RequestPlayerServerRPC(ServerRpcParams rpcParams = default)
    {
        GameObject newCharacter = Instantiate(dcs.defaultCharacter);
        newCharacter.GetComponent<NetCharacterMotor_V2>().input = this;
        ncm2 = newCharacter.GetComponent<NetCharacterMotor_V2>();
        newCharacter.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
    }
}
