using Eclipse.Networks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuGameConnector : MonoBehaviour
{
    public TMP_InputField jc_inputField;
    public string joinCode;
    public void CreateGameButton()
    {
        ConnectionManager.instance.CreateGame();
    }
    public void PlayLocalButton()
    {
        ConnectionManager.instance.StartLocalGame();
    }
    public void JoinGameButton()
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("Cannot Join; Join Code is empty.");
        }
        else
        {
            ConnectionManager.instance.targetJoinCode = joinCode;
            ConnectionManager.instance.JoinGame();
        }
    }
    public void SetJoinCode(string joinCode)
    {
        this.joinCode = joinCode;
    }
}
