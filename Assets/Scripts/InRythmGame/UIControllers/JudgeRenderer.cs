using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JudgeRenderer : MonoBehaviour
{
    [SerializeField]
    private Sprite[] judgeSprites;
    [SerializeField]
    private TimingRenderer timingRenderer;
    private SpriteRenderer spriteRenderer;
    private float zoomDuration = 0.1f;
    private float keepDuration = 0.2f;
    private Coroutine judgeCoroutine;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
    }
    public void ShowJudge(JudgeType judgeID, bool isFast = false)
    {
        if (judgeCoroutine != null) StopCoroutine(judgeCoroutine);
        Sprite judgeSprite = judgeSprites[(int)judgeID];
        judgeCoroutine = StartCoroutine(ZoomIn(judgeSprite));
        if (judgeID == JudgeType.Perfect || judgeID == JudgeType.Miss) return;
        timingRenderer.ShowTiming(isFast);
    }

    IEnumerator ZoomIn(Sprite sprite)
    {
        transform.localScale = Vector3.zero;
        spriteRenderer.sprite = sprite;
        float time = 0f;
        while (time < zoomDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one / 2, time / zoomDuration);
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(keepDuration);
        spriteRenderer.sprite = null;
    }
}
