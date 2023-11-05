using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimedSceneSwitch : MonoBehaviour
{
    public float sceneSwitchTime;
    public float timeLeft;
    public SceneReference targetScene;
    public bool onStart;
    public TextMeshProUGUI sceneLoadTimer;
    public string timeLeftString;
    private void Start()
    {
        if (onStart)
            if (targetScene != null)
                StartCoroutine(SwitchScenes());
    }
    public void StartSceneSwitch()
    {
        if (targetScene != null)
            StartCoroutine(SwitchScenes());
    }
    IEnumerator SwitchScenes()
    {
        timeLeft = sceneSwitchTime;
        while (timeLeft > 0)
        {
            timeLeft -= Time.fixedDeltaTime;
            if (sceneLoadTimer)
                sceneLoadTimer.text = $"{timeLeftString}{timeLeft}";
            yield return new WaitForFixedUpdate();
        }
        SceneManager.LoadScene(targetScene.BuildIndex);
    }
}
