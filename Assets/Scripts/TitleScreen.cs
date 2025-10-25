using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(TransitionSelectMusic());
        }
    }

    IEnumerator TransitionSelectMusic()
    {
        yield return FadeRenderer.Instance.FadeCoroutine(0, 1, 1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("SelectMusicScene");
    }
}
