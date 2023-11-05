using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DebugPlayerInit : NetworkBehaviour
{
    NetworkVariable<float> colour_r = new(writePerm: NetworkVariableWritePermission.Owner), colour_g = new(writePerm: NetworkVariableWritePermission.Owner), colour_b = new(writePerm: NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"{IsOwner}, {IsOwnedByServer}, {OwnerClientId}");
        if (IsOwner)
        {
            RandomColour();
            var spawnPos = Random.insideUnitCircle * 5;
            transform.position = new(spawnPos.x, 20, spawnPos.y);
        }
    }
    private void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.color = new(colour_r.Value, colour_g.Value, colour_b.Value);
    }
    public void RandomColour()
    {
        Color c = Random.ColorHSV();
        colour_b.Value = c.b;
        colour_r.Value = c.r;
        colour_g.Value = c.g;
    }
}
