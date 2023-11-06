using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalLayerCorrection : NetworkBehaviour
{
    [SerializeField] Transform localPlayerRoot;
    [SerializeField] string localLayer;
    [SerializeField] string invisibleLayer;
    [SerializeField] GameObject[] objectsToHide;
    [SerializeField]
    GameObject[] objectsToSetInvisible;
    [SerializeField] GameObject[] hideOnRemote;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


        if (IsOwner && localPlayerRoot)
        {
            FixThisShit();
        }
    }
    public void FixThisShit()
    {
        if (IsOwner && localPlayerRoot)
        {
            foreach (var item in localPlayerRoot.GetComponentsInChildren<Transform>(true))
            {
                item.gameObject.layer = LayerMask.NameToLayer(localLayer);
            }
            foreach (var item in objectsToHide)
            {
                item.SetActive(false);
            }
            foreach (var item in objectsToSetInvisible)
            {
                foreach (var item2 in item.GetComponentsInChildren<Transform>(true))
                {
                    item2.gameObject.layer = LayerMask.NameToLayer(invisibleLayer);
                }
            }
        }
    }
}
