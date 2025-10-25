using System;
using UnityEngine;

public class ChartReader
{
    public ChartRoot Parse(TextAsset chartFile)
    {
        if (chartFile == null) { Debug.LogError("JSONが見つかりません"); return null; }
        ChartRoot chartRoot;

        try
        {
            chartRoot = JsonUtility.FromJson<ChartRoot>(chartFile.text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScoreReader] JSONパース例外 ({chartFile.name}) : {e}");
            return null;
        }

        if (chartRoot == null)
        {
            Debug.LogError($"[ScoreReader] JSONパースに失敗しました ({chartFile.name})");
            return null;
        }

        // 簡易バリデーション
        if (chartRoot.timing == null || chartRoot.chart == null)
        {
            Debug.LogError($"[ScoreReader] JSON構造が不足しています ({chartFile.name})");
            return null;
        }
        if (chartRoot.timing.bpmPoints == null || chartRoot.timing.bpmPoints.Length == 0)
        {
            Debug.LogWarning($"[ScoreReader] bpmPoints が空です（少なくとも beat=0 を推奨）({chartFile.name})");
        }
        if (chartRoot.chart.notes == null)
        {
            Debug.LogWarning($"[ScoreReader] notes が null です ({chartFile.name})");
            chartRoot.chart.notes = Array.Empty<NotesItem>();
        }
        if (chartRoot.chart.events == null)
        {
            chartRoot.chart.events = Array.Empty<ChartEvent>();
        }

        // OK
        return SetNotesID(chartRoot);
    }
    private ChartRoot SetNotesID(ChartRoot chart)
    {
        if (chart == null || chart.chart == null || chart.chart.notes == null) return chart;
        for (int i = 0; i < chart.chart.notes.Length; i++)
        {
            chart.chart.notes[i].id = i;
        }
        return chart;
    }
}
