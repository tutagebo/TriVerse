using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeRenderer : MonoBehaviour
{
    public static FadeRenderer Instance { get; private set; }

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Image image;

    [Header("フェード設定")]
    public Color fadeColor = Color.black;

    void Awake()
    {
        // すでに存在する場合は新しいものを破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // インスタンス登録 & シーン切り替えで破棄しない
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ===== フェード用UIを自動生成 =====
        // Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // 一番上に表示されるように大きい値
        canvasObj.AddComponent<GraphicRaycaster>();

        // CanvasGroup
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // 初期は透明

        // Image（黒背景）
        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObj.transform, false);
        image = imgObj.AddComponent<Image>();
        image.color = fadeColor;
        image.raycastTarget = false; // フェード中はクリック無効化

        // RectTransform設定（全画面に広げる）
        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ============= フェード制御メソッド =============
    // いらんかもしれん完成まで使わんかったら消す
    public void FadeIn(float duration)
    {
        StartCoroutine(FadeCoroutine(1, 0, duration));
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeCoroutine(0, 1, duration));
        image.raycastTarget = false;
    }

    public IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        image.raycastTarget = true; // フェード中はクリック無効化
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
        image.raycastTarget = false; // フェード終了後はクリック有効化
    }
}
