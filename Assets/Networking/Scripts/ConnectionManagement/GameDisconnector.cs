using Eclipse.Networks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisconnector : MonoBehaviour
{
    public void DisconnectGameButton()
    {
        ConnectionManager.instance.DisconnectFromGame();
    }
}
