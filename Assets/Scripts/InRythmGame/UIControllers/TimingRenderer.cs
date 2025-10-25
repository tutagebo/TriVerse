using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingRenderer : MonoBehaviour
{
    [SerializeField]
    private Sprite[] timingSprites; // 0: fast, 1: late
    private SpriteRenderer spriteRenderer;
    private float zoomDuration = 0.1f;
    private float keepDuration = 0.2f;
    private Coroutine timingCoroutine;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
    }
    public void ShowTiming(bool isFast)
    {
        // fast/late表示
        if (timingCoroutine != null) StopCoroutine(timingCoroutine);
        Sprite timingSprite = isFast ? timingSprites[0] : timingSprites[1];
        timingCoroutine = StartCoroutine(ZoomIn(timingSprite));
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
