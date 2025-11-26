using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BeatLabelRenderer : MonoBehaviour
{
    public TimelineRenderer grid;          // グリッドを参照
    public RectTransform labelParent;  // ラベルを置く親（Canvasの中）
    public TMP_Text labelPrefab;       // TextMeshProUGUI のプレハブ

    readonly List<TMP_Text> pool = new();

    void LateUpdate()
    {
        // グリッド範囲を参照
        double start = grid.startBeat;
        double end = grid.endBeat;
        float ppb = grid.pixelsPerBeat;
        float width = ((RectTransform)grid.transform).rect.width;

        // 一度全て非表示にして再利用
        foreach (var t in pool) t.gameObject.SetActive(false);

        int labelIndex = 0;
        for (int i = Mathf.FloorToInt((float)start); i <= Mathf.CeilToInt((float)end); i++)
        {
            // X位置
            float x = (float)((i - start) * ppb);
            if (x < 0 || x > width) continue;

            TMP_Text txt;
            if (labelIndex >= pool.Count)
            {
                txt = Instantiate(labelPrefab, labelParent);
                pool.Add(txt);
            }
            else txt = pool[labelIndex];

            txt.gameObject.SetActive(true);
            txt.rectTransform.anchoredPosition = new Vector2(x, -10f); // 少し下げる
            txt.text = i.ToString();
            txt.fontSize = 20f;
            labelIndex++;
        }
    }
}
