using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public void OnClickStart()
    {
        StartCoroutine(TransitionMainGame());
    }

    IEnumerator TransitionMainGame()
    {
        yield return FadeRenderer.Instance.FadeCoroutine(0, 1, 1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }
}
