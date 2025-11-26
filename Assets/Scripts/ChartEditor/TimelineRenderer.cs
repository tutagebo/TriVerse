using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System;

[RequireComponent(typeof(RectTransform))]
public class TimelineRenderer : MaskableGraphic, IScrollHandler
{
    // 尺と見た目
    [NonSerialized]
    public float pixelsPerBeat = 120f;
    int   subDiv = 4;         // 1拍を何分割（例: 4=16分）
    int groupCount = 3;
    float groupHeight = 96f;
    float laneHeight = 24f;
    float linePx = 1f;


    NotesItem[] chartNotes;

    // 描画範囲
    [NonSerialized]
    public double startBeat = 0;     // このGraphicの左端がどの拍か
    [NonSerialized]
    public double endBeat = 64;    // 右端
    
    Color lineDefault = new(0.5f, 0.5f, 0.5f, 1f);
    Color lineBeat = new(1f, 1f, 1f, 1f);
    Color judgeLine = new(1f, 0f, 0f, 1f);

    // 消す
    [SerializeField] private TextAsset chartJson;  // テスト用譜面データ
    ChartRoot chartData;
    // 消す

    protected override void Start()
    {
        ChartReader reader = new ChartReader();
        chartData = reader.Parse(chartJson);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r = rectTransform.rect;
        float width = r.width;
        float height = r.height;
        float middleY = height / 2f;
        float offset = pixelsPerBeat / (2f * subDiv);
        
        // --- 横線（レーン） ---
        DrawHorizontalLines(vh, width, middleY);
        // --- 縦線（拍・分割） ---
        DrawBeatLines(vh, width, middleY, offset);
        // --- ノーツ ---
        DrawNotes(chartData.chart.notes, vh, height, offset);
    }
    void DrawBeatLines(VertexHelper vh, float width, float middleY, float offset)
    {
        // 画面上下（少しはみ出す）
        float top =  middleY + (groupHeight + laneHeight) + 10f;
        float bottom = middleY - (groupHeight + laneHeight) - 10f;
        
        // 表示範囲をサブ拍刻みに変換
        int subStart = Mathf.FloorToInt((float)(startBeat * subDiv));
        int subEnd = Mathf.CeilToInt((float)(endBeat * subDiv));

        for (int i = subStart; i <= subEnd; i++)
        {
            // いまのサブ拍が何拍目か
            double beat = (double)i / subDiv;

            // 画面左端を 0 として x を出す（ココが肝）
            float x = (float)((beat - startBeat) * pixelsPerBeat + offset);
            if (x < -2f || x > width + 2f) continue; // 画面外はスキップ

            float px = linePx;
            Color col = lineDefault;

            // 線の種類（小節／拍／サブ拍）で太さ・色を切替
            bool isBeat = (i % subDiv == 0);
            bool isMeas = isBeat && Mathf.Approximately((float)(beat % 4.0), 0f);
            if (isBeat)
            {
                px = linePx * 1.5f;
                col = lineBeat;
            }
            if (isMeas)
            {
                px = linePx * 2f;
                col = lineBeat;
            }

            AddVLine(vh, x, bottom, top, px, col);
        }
        AddVLine(vh, offset, bottom - 10f, top + 10f, linePx, judgeLine); // 左端
    }
    
    public void OnScroll(PointerEventData e)
    {
        if(startBeat <= 0 && e.scrollDelta.y > 0) return;
        // e.scrollDelta.y は 上:+, 下:- （環境で逆な場合は符号を反転）
        float wheel = e.scrollDelta.y;

        // 1スクロールあたり何拍動かすか
        float panBeats = 0.25f;

        double span = endBeat - startBeat;           // 表示幅を維持
        startBeat -= wheel * panBeats;
        endBeat    = startBeat + span;
        Debug.Log($"Scroll: start={startBeat}, end={endBeat}");
        Invalidate();
    }

    void DrawHorizontalLines(VertexHelper vh, float width, float middleY)
    {
        // --- 横線（レーン） ---
        for (int group = 0; group < groupCount; group++)
        {
            float groupBase = (group - 1) * groupHeight + middleY;
            for (int lane = 0; lane < 3; lane++)
            {
                float laneY = groupBase + laneHeight * (lane - 1);
                AddHLine(vh, 0, width, laneY, linePx, lineDefault);
            }
        }
    }

    public void UpdateNotes(NotesItem[] notes)
    {
        chartNotes = notes;
        Invalidate();
    }

    void DrawNotes(NotesItem[] notes, VertexHelper vh, float canvasHeight, float offset)
    {
        foreach (NotesItem n in notes)
        {
            switch (n.type)
            {
                case "tap":
                    DrawTapNotes(vh, n, canvasHeight, offset);
                    break;
                case "flick":
                    DrawFlickNotes(vh, n, canvasHeight, offset);
                    break;
                case "hold":
                    DrawLongNotes(vh, n, canvasHeight, offset);
                    break;
            }
        }
    }

    void DrawTapNotes(VertexHelper vh, NotesItem notes, float canvasHeight, float offset)
    {
        if (notes.beat < startBeat || notes.beat > endBeat) return;   // 表示範囲外はスキップ

        // 位置計算
        float x = (float)((notes.beat - startBeat) * pixelsPerBeat + offset);
        float y = LaneToY(notes.group * 3 + notes.lane, canvasHeight);
        if (y < -10f || y > canvasHeight + 10f) return;

        // 小さな四角で点を描く
        float notePx = 10f;
        float half = notePx * 0.5f;
        Color noteColor = new(0.5f, 1f, 1f, 1f);

        var a = new Vector3(x - half, y - half);
        var b = new Vector3(x + half, y - half);
        var c = new Vector3(x + half, y + half);
        var d = new Vector3(x - half, y + half);
        AddQuad(vh, a, b, c, d, noteColor);
    }

    void DrawFlickNotes(VertexHelper vh, NotesItem notes, float canvasHeight, float offset)
    {
        if (notes.beat < startBeat || notes.beat > endBeat) return;   // 表示範囲外はスキップ

        // 位置計算
        float x = (float)((notes.beat - startBeat) * pixelsPerBeat + offset);
        float y = LaneToY(notes.group * 3 + 1, canvasHeight);
        if (y < -10f || y > canvasHeight + 10f) return;

        // 三角を描く
        float notePx = 10f;
        float half = notePx * 0.5f;
        Color noteColor = new(1f, 0.5f, 0f, 1f);
        float height = laneHeight;

        if (notes.direction == FlickDirection.left)
        {
            noteColor = new(0.5f, 0f, 1f, 1f);
            height = -laneHeight;
        }

        var a = new Vector3(x - half, y - height);
        var b = new Vector3(x + half, y - height);
        var c = new Vector3(x, y + height);
        AddTriangle(vh, a, b, c, noteColor);
    }

    void DrawLongNotes(VertexHelper vh, NotesItem notes, float canvasHeight, float offset)
    {
        if (notes.endBeat < startBeat || notes.beat > endBeat) return;   // 表示範囲外はスキップ
        // 位置計算
        float startX = (float)((notes.beat - startBeat) * pixelsPerBeat + offset);
        float endX = (float)((notes.endBeat - startBeat) * pixelsPerBeat + offset);
        float y = LaneToY(notes.group * 3 + notes.lane, canvasHeight);
        if (y < -10f || y > canvasHeight + 10f) return;

        // 長方形を描く
        float notePx = 10f;
        float half = notePx * 0.5f;
        Color noteColor = new(0.5f, 1f, 0.5f, 1f);

        var a = new Vector3(startX - half, y - half);
        var b = new Vector3(endX + half, y - half);
        var c = new Vector3(endX + half, y + half);
        var d = new Vector3(startX - half, y + half);
        AddQuad(vh, a, b, c, d, noteColor);
    }

    float LaneToY(int laneIndex, float height)
    {
        // laneIndex: 0..8 → (group=0..2, lane=0..2)
        int lanesPerGroup = 3;
        int g = laneIndex / lanesPerGroup;
        int l = laneIndex % lanesPerGroup;

        float middleY = height * 0.5f;
        float groupBase = (g - 1) * groupHeight + middleY;
        return groupBase + laneHeight * (l - 1);
    }
    
    public (int lane, double beat) LocalPosToLaneBeat(Vector2 localPos, float canvasHeight)
    {
        // === Beat計算 ===
        // OnPopulateMeshと同じoffset式で整合性を取る
        float offset = pixelsPerBeat / (2f * subDiv);
        double beat = (localPos.x - offset) / pixelsPerBeat + startBeat;

        // === Lane計算 ===
        const int lanesPerGroup = 3;
        float middleY = canvasHeight * 0.5f;

        // group のおおよそのインデックス
        float gApprox = (localPos.y - middleY) / groupHeight + 1f;
        int g = Mathf.Clamp(Mathf.RoundToInt(gApprox), 0, groupCount - 1);

        // グループ基準線
        float groupBase = (g - 1) * groupHeight + middleY;

        // group 内 lane のおおよそのインデックス
        float lApprox = (localPos.y - groupBase) / laneHeight + 1f;
        int l = Mathf.Clamp(Mathf.RoundToInt(lApprox), 0, 2);

        int laneIndex = g * lanesPerGroup + l;

        return (laneIndex, beat);
    }

    void AddVLine(VertexHelper vh, float x, float y0, float y1, float px, Color color)
    {
        float half = px * 0.5f;
        var a = new Vector3(x - half, y0);
        var b = new Vector3(x + half, y0);
        var c = new Vector3(x + half, y1);
        var d = new Vector3(x - half, y1);
        AddQuad(vh, a,b,c,d, color);
    }

    void AddHLine(VertexHelper vh, float x0, float x1, float y, float px, Color color)
    {
        float half = px * 0.5f;
        var a = new Vector3(x0, y - half);
        var b = new Vector3(x1, y - half);
        var c = new Vector3(x1, y + half);
        var d = new Vector3(x0, y + half);
        AddQuad(vh, a,b,c,d, color);
    }

    void AddQuad(VertexHelper vh, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
    {
        int i = vh.currentVertCount;
        vh.AddVert(a, color, Vector2.zero);
        vh.AddVert(b, color, Vector2.zero);
        vh.AddVert(c, color, Vector2.zero);
        vh.AddVert(d, color, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2);
        vh.AddTriangle(i, i + 2, i + 3);
    }
    
    void AddTriangle(VertexHelper vh, Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        int i = vh.currentVertCount;
        vh.AddVert(a, color, Vector2.zero);
        vh.AddVert(b, color, Vector2.zero);
        vh.AddVert(c, color, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2);
    }

    // パラメータ更新時に再描画
    public void Invalidate() => SetVerticesDirty();
}