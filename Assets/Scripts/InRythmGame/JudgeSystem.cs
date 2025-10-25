using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class JudgeSystem : MonoBehaviour
{
    private List<NotesItem> notesItems = new List<NotesItem>();
    private Dictionary<NotesID, Notes> notesObjects = new Dictionary<NotesID, Notes>();
    private GameSettings gameSettings;

    [SerializeField]
    private JudgeRenderer judgeRenderer;
    [SerializeField]
    private ComboRenderer comboRenderer;
    private ScoreManager scoreManager;

    // ============== 追加: イベント分割データ構造 ==============
    private enum NoteEventType { Tap, Flick, HoldStart, HoldTick }
    private enum NoteEventState { Pending, Judged }

    private struct NoteEvent
    {
        public int eventId;                 // 一意ID（配列インデックスでも可）
        public NotesID sourceId;            // どのNotesItem由来か（Release管理用）
        public NoteEventType type;
        public int lane;
        public int group;
        public FlickDirection direction;    // Flickのみ使用
        public double beat;                 // 評価Beat
        public double timeSec;              // 評価秒（dsp基準）
        public double deadlineSec;          // Miss確定の締切（= timeSec + missMS補正）
        public NoteEventState state;        // Pending → Judged
    }

    // すべての判定イベント（「単一の真実」）
    private List<NoteEvent> noteEvents = new List<NoteEvent>();

    // 各ノーツ（sourceId）に紐づく未処理イベント残数（0になったらRelease）
    private Dictionary<NotesID, int> sourcePendingCount = new Dictionary<NotesID, int>();

    // lane+group から Pending イベントを素早く引くためのインデックス
    private Dictionary<(int lane, int group), List<int>> pendingIndex = new Dictionary<(int,int), List<int>>();

    // 近傍探索の時間幅（早押しスルー緩和用）：判定窓は gameSettings の perfect/great/good/miss を使用
    private double windowEarlyMs => gameSettings.missMS; // 早すぎはスルーしたい幅（早押し窓外は無視）

    // ========================= [EndCleanup] ホールド終点の削除スケジュール =========================
    private struct HoldEndSentinel
    {
        public NotesID sourceId;
        public int lane;
        public int group;
        public double endBeat;
        public double endTimeSec;  // dsp基準
        public bool done;          // 一度だけ実行
    }
    private readonly List<HoldEndSentinel> holdEndSentinels = new List<HoldEndSentinel>();
    // =======================================================================

    public void Init(NotesItem[] chart, ScoreManager scoreManager, GameSettings settings)
    {
        this.scoreManager = scoreManager;
        this.scoreManager.Init(comboRenderer, settings);
        gameSettings = settings;
        notesItems = new List<NotesItem>(chart);

        // 譜面をイベントへ展開
        BuildEventsFromChart(notesItems);

        // pendingインデックス構築
        RebuildPendingIndex();
    }

    public void RegisterNotesObject(Notes notes)
    {
        notesObjects.Add(notes.id, notes);
    }

    // ========================= 譜面→イベント展開 =========================
    private void BuildEventsFromChart(List<NotesItem> items)
    {
        noteEvents.Clear();
        sourcePendingCount.Clear();
        holdEndSentinels.Clear(); // [EndCleanup] クリア

        int eid = 0;
        foreach (NotesItem n in items)
        {
            switch (n.type)
            {
                case "tap":
                {
                    NoteEvent t = MakeEvent(eid++, n, NoteEventType.Tap, n.beat);
                    noteEvents.Add(t);
                    IncSourcePending(n.id);
                    break;
                }
                case "flick":
                {
                    NoteEvent f = MakeEvent(eid++, n, NoteEventType.Flick, n.beat);
                    noteEvents.Add(f);
                    IncSourcePending(n.id);
                    break;
                }
                case "hold":
                {
                    // 始点
                    NoteEvent s = MakeEvent(eid++, n, NoteEventType.HoldStart, n.beat);
                    noteEvents.Add(s);
                    IncSourcePending(n.id);

                    // 0拍起点の整数拍ごとにティック（startBeat < t < endBeat）※終点では判定しない
                    int startGridNext = (int)Math.Ceiling(Math.Max(n.beat, 0.0));
                    const double EPS = 1e-6;
                    int endLimitExclusive = (int)Math.Floor(n.endBeat - EPS); // endBeatちょうどは除外
                    for (int t = startGridNext; t <= endLimitExclusive; t++)
                    {
                        if (t > n.beat) // startBeat より後の整数拍のみ
                        {
                            NoteEvent tick = MakeEvent(eid++, n, NoteEventType.HoldTick, t);
                            noteEvents.Add(tick);
                            IncSourcePending(n.id);
                        }
                    }

                    // [EndCleanup] 終点到達時に「判定無しで削除のみ」を行うためのセンチネルを登録
                    double endTimeSec = TimingManager.BeatToSeconds(n.endBeat, gameSettings.bpmPoints);
                    holdEndSentinels.Add(new HoldEndSentinel {
                        sourceId = n.id,
                        lane = n.lane,
                        group = n.group,
                        endBeat = n.endBeat,
                        endTimeSec = endTimeSec,
                        done = false
                    });

                    // [EndCleanup] 終点まで GameObject を残すため、
                    // ここで SourcePending を 1 増やして「終点センチネル分の未処理」を積んでおく
                    IncSourcePending(n.id);
                    break;
                }
            }
        }

        // Beat昇順（任意だがデバッグしやすい）
        noteEvents.Sort((a, b) => a.beat.CompareTo(b.beat));

        for (int i = 0; i < noteEvents.Count; i++)
        {
            NoteEvent e = noteEvents[i];
            e.eventId = i;
            noteEvents[i] = e;
        }
    }

    private NoteEvent MakeEvent(int eventId, NotesItem src, NoteEventType type, double beat)
    {
        double timeSec = TimingManager.BeatToSeconds(beat, gameSettings.bpmPoints);

        double deadlineSec = timeSec + (gameSettings.missMS - gameSettings.userLatencyMs) / 1000.0; // user補正を合わせたMiss締切

        return new NoteEvent {
            eventId = eventId,
            sourceId = src.id,
            type = type,
            lane = src.lane,
            group = src.group,
            direction = src.direction, // tap/holdでは未使用
            beat = beat,
            timeSec = timeSec,
            deadlineSec = deadlineSec,
            state = NoteEventState.Pending
        };
    }

    private void IncSourcePending(NotesID id)
    {
        if (!sourcePendingCount.ContainsKey(id)) sourcePendingCount[id] = 0;
        sourcePendingCount[id]++;
    }

    private void DecSourcePendingAndMaybeRelease(NotesID id)
    {
        if (!sourcePendingCount.ContainsKey(id)) return;
        sourcePendingCount[id]--;
        if (sourcePendingCount[id] <= 0)
        {
            sourcePendingCount.Remove(id);
            // 最後のイベントが確定したのでGameObject側を解放
            if (notesObjects.TryGetValue(id, out var obj) && obj.isReleased == false)
            {
                obj.Release();
                notesObjects.Remove(id);
            }
            // notesItems からも掃除（FindIndex安全化）
            int idx = notesItems.FindIndex(n => n.id.Equals(id));
            if (idx != -1) notesItems.RemoveAt(idx);
        }
    }

    private void RebuildPendingIndex()
    {
        pendingIndex.Clear();
        for (int i = 0; i < noteEvents.Count; i++)
        {
            if (noteEvents[i].state != NoteEventState.Pending) continue;
            var key = (noteEvents[i].lane, noteEvents[i].group);
            if (!pendingIndex.TryGetValue(key, out var list))
            {
                list = new List<int>();
                pendingIndex[key] = list;
            }
            list.Add(i);
        }
    }

    private void RemoveFromPendingIndex(int eventIdx)
    {
        NoteEvent e = noteEvents[eventIdx];
        var key = (e.lane, e.group);
        if (pendingIndex.TryGetValue(key, out var list))
        {
            list.Remove(eventIdx);
            if (list.Count == 0) pendingIndex.Remove(key);
        }
    }

    // ========================= 期限切れスイープ（Miss確定） =========================
    public void UpdateJudge()
    {
        // Miss判定用（スイーパ）
        double nowSec = TimingManager.GetNowDsp(gameSettings);

        // Pending の中から締切超過をMissへ
        // ※ コストが気になる場合は min-heap/カーソルで時間順に走査
        for (int i = 0; i < noteEvents.Count; i++)
        {
            NoteEvent e = noteEvents[i];
            if (e.state != NoteEventState.Pending) continue;
            // 期限超過
            if (nowSec > e.deadlineSec)
            {
                JudgeEvent(i, JudgeType.Miss);
            }
        }

        // ========================= [EndCleanup] ホールド終点の削除だけ行う =========================
        for (int i = 0; i < holdEndSentinels.Count; i++)
        {
            if (holdEndSentinels[i].done) continue;
            if (nowSec >= holdEndSentinels[i].endTimeSec)
            {
                // 視覚効果なし・スコア加算なし・判定表示なし → 純粋に削除のみ
                // ただし GameObject を終点まで残すために積んでいたセンチネル分を解放
                var h = holdEndSentinels[i];
                holdEndSentinels[i] = new HoldEndSentinel
                {
                    sourceId = h.sourceId,
                    lane = h.lane,
                    group = h.group,
                    endBeat = h.endBeat,
                    endTimeSec = h.endTimeSec,
                    done = true
                };

                // センチネル分の pending を1つ減らす（これで初めて Release になる）
                DecSourcePendingAndMaybeRelease(h.sourceId);
            }
        }
    }

    // ========================= 判定実行 =========================

    // 判定実行 押したキーはlaneIDに変換して渡す
    // 左から 0,1,2 同様にグループも左から 0,1,2
    public JudgeType ExecuteJudge(int laneID, int groupID)
    {
        double nowSec = TimingManager.GetNowDsp(gameSettings);

        // lane+groupのPending候補から、Tap or HoldStart の「時間が近い順」を探索
        int candidateIdx = GetBestCandidateEventIndex(laneID, groupID, nowSec,
            allowedTypes: new[] { NoteEventType.Tap, NoteEventType.HoldStart });

        if (candidateIdx < 0) return JudgeType.None;

        return JudgeEventByDelta(candidateIdx, nowSec);
    }

    public JudgeType JudgeFlick(int groupID, FlickDirection direction)
    {
        double nowSec = TimingManager.GetNowDsp(gameSettings);

        // laneは不問、groupとdirection一致の Flick を探索
        // Flickはレーン固定でない仕様なら、全lane候補から group+dir で最良を拾う
        int bestIdx = -1;
        double bestAbsMs = double.MaxValue;

        foreach (var kv in pendingIndex)
        {
            var (lane, group) = kv.Key;
            if (group != groupID) continue;
            foreach (int i in kv.Value)
            {
                NoteEvent e = noteEvents[i];
                if (e.state != NoteEventState.Pending) continue;
                if (e.type != NoteEventType.Flick) continue;
                if (e.direction != direction) continue;

                double ms = (nowSec - e.timeSec) * 1000.0 + gameSettings.userLatencyMs;
                double abs = Math.Abs(ms);

                // 早押しでmissMSより早いならスルー（候補外）
                if (ms < 0 && abs > windowEarlyMs) continue;

                if (abs < bestAbsMs)
                {
                    bestAbsMs = abs;
                    bestIdx = i;
                }
            }
        }

        if (bestIdx < 0) return JudgeType.None;
        return JudgeEventByDelta(bestIdx, nowSec);
    }

    // ホールドノーツの判定
    // 1拍ごとに呼び出す
    // 押しっぱなしならPerfect、離してたらMiss
    public void HoldNotesKeepJudge(List<int> holdingLanes, int groupID, int nowBeat)
    {
        // 該当Beatの HoldTick をすべて確定させる
        // 0拍起点の整数拍ごとのTickをイベントとして持っているので、そのBeat一致のPendingだけを見る
        foreach (var kv in pendingIndex.ToList())
        {
            var (lane, group) = kv.Key;
            if (group != groupID) continue;

            // lane単位で走査
            List<int> list = kv.Value.ToList();
            foreach (int idx in list)
            {
                NoteEvent e = noteEvents[idx];
                if (e.state != NoteEventState.Pending) continue;
                if (e.type != NoteEventType.HoldTick) continue;

                // Beat一致のTickだけ判定
                if (Math.Abs(e.beat - nowBeat) > 1e-6) continue;

                if (holdingLanes.Contains(lane))
                {
                    JudgeEvent(idx, JudgeType.Perfect);
                }
                else
                {
                    JudgeEvent(idx, JudgeType.Miss);
                }
            }
        }
    }

    // ========================= 低レベル判定確定処理 =========================

    // Δ時間から Perfect/Great/Good/Miss を決めて確定
    private JudgeType JudgeEventByDelta(int eventIdx, double nowSec)
    {
        NoteEvent e = noteEvents[eventIdx];
        double ms = (nowSec - e.timeSec) * 1000.0 + gameSettings.userLatencyMs;
        double abs = Math.Abs(ms);
        bool isFast = ms < 0;

        // 早押しでmissMSより外ならスルー（判定無し）
        if (isFast && abs > gameSettings.missMS) return JudgeType.None;

        JudgeType jt;
        if (abs <= gameSettings.perfectMS) jt = JudgeType.Perfect;
        else if (abs <= gameSettings.greatMS) jt = JudgeType.Great;
        else if (abs <= gameSettings.goodMS) jt = JudgeType.Good;
        else jt = JudgeType.Miss;

        JudgeEvent(eventIdx, jt, isFast);
        return jt;
    }

    private void JudgeEvent(int eventIdx, JudgeType jt, bool isFast=false)
    {
        NoteEvent e = noteEvents[eventIdx];
        if (e.state != NoteEventState.Pending) return; // 二重判定防止

        // スコア加算・表示
        switch (jt)
        {
            case JudgeType.Perfect:
                judgeRenderer.ShowJudge(JudgeType.Perfect, isFast);
                scoreManager.AddPerfect();
                break;
            case JudgeType.Great:
                judgeRenderer.ShowJudge(JudgeType.Great, isFast);
                // TODO: タイミング加算
                scoreManager.AddGreat();
                break;
            case JudgeType.Good:
                judgeRenderer.ShowJudge(JudgeType.Good, isFast);
                // TODO: タイミング加算
                scoreManager.AddGood();
                break;
            case JudgeType.Miss:
                judgeRenderer.ShowJudge(JudgeType.Miss, isFast);
                scoreManager.AddMiss();
                break;
        }

        // 状態遷移
        e.state = NoteEventState.Judged;
        noteEvents[eventIdx] = e;

        // pendingインデックスから除去
        RemoveFromPendingIndex(eventIdx);

        // 由来ノーツの残イベント数を減算、0ならRelease
        DecSourcePendingAndMaybeRelease(e.sourceId);
    }

    // lane+group の Pending 候補から、allowedTypes の中で時間が最も近いものを返す
    // 見つからなければ -1
    private int GetBestCandidateEventIndex(int lane, int group, double nowSec, NoteEventType[] allowedTypes)
    {
        var key = (lane, group);
        if (!pendingIndex.TryGetValue(key, out var list)) return -1;

        int bestIdx = -1;
        double bestAbsMs = double.MaxValue;

        foreach (int i in list)
        {
            NoteEvent e = noteEvents[i];
            if (e.state != NoteEventState.Pending) continue;
            if (!allowedTypes.Contains(e.type)) continue;

            double ms = (nowSec - e.timeSec) * 1000.0 + gameSettings.userLatencyMs;
            double abs = Math.Abs(ms);

            // 早押しでmissMSより早いものは候補外（スルー）にする
            if (ms < 0 && abs > windowEarlyMs) continue;

            if (abs < bestAbsMs)
            {
                bestAbsMs = abs;
                bestIdx = i;
            }
        }
        return bestIdx;
    }
}
