using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesManager : MonoBehaviour
{
    [SerializeField]
    private GameObject tapNotesPrefab;
    [SerializeField]
    private GameObject holdNotesPrefab;
    [SerializeField]
    private GameObject leftFlickNotesPrefab;
    [SerializeField]
    private GameObject rightFlickNotesPrefab;
    public GameObject[] notesGenerators;
    public int GROUP_LENGTH = 3;
    Vector3 DISPLAY_OUTSIDE = new Vector3(0, -15, 0);
    const double SPAWN_AHEAD_TIME = 5.0; // 秒
    const int TAP_PREWARM = 90;
    const int HOLD_PREWARM = 30;
    const int FLICK_PREWARM = 20;
    public void GenerateStart(ChartRoot chartData, GameSettings gameSettings)
    {
        StartCoroutine(SpawnNotesAtDsp(chartData, gameSettings));
    }
    public ChartRoot GenerateDebugChart(ChartRoot chartData, GameSettings gameSettings, int startBeat)
    {
        var notesList = new List<NotesItem>(chartData.chart.notes);
        notesList.RemoveAll(n => n.beat < startBeat);
        notesList.ForEach(n => {
            n.beat -= startBeat;
            if (n.type == "hold") n.endBeat -= startBeat;
        });
        chartData.chart.notes = notesList.ToArray();
        return chartData;
    }
    private IEnumerator SpawnNotesAtDsp(ChartRoot chartData, GameSettings gameSettings)
    {
        Queue<NotesItem> notesQueue = new Queue<NotesItem>(chartData.chart.notes);
        while (notesQueue.Count > 0)
        {
            NotesItem spawnNote = notesQueue.Dequeue();
            int laneID = spawnNote.lane + spawnNote.group * GROUP_LENGTH;
            // laneIDをチェック
            if (!IsLaneValid(laneID))
            {
                Debug.LogWarning($"[NotesManager] laneID {laneID} が無効です");
                continue;
            }
            // 存在チェック
            GameObject generatorObject = notesGenerators[laneID];
            if (generatorObject == null)
            {
                Debug.LogWarning($"[NotesManager] laneID {laneID} のGameObjectがnullです");
                yield break;
            }
            NotesGenerator notesGenerator = generatorObject.GetComponent<NotesGenerator>();
            if (notesGenerator == null)
            {
                Debug.LogWarning($"[NotesManager] NotesGeneratorが見つかりません（laneID={laneID}）");
                yield break;
            }
            //　ここまで
            // ノーツの拍位置から dsp 時刻を計算
            double targetDsp = TimingManager.BeatToSeconds(spawnNote.beat, chartData.timing.bpmPoints);
            // 指定dsp時刻まで待機
            while (TimingManager.GetNowDsp(gameSettings) < targetDsp - SPAWN_AHEAD_TIME) yield return null;
            // ノーツ生成
            if (spawnNote.type == "tap")
                notesGenerator.GenerateTapNotes(spawnNote.id, spawnNote.beat);
            else if (spawnNote.type == "hold")
                notesGenerator.GenerateHoldNotes(spawnNote.id, spawnNote.beat, spawnNote.endBeat);
            else if (spawnNote.type == "flick" && spawnNote.direction == FlickDirection.left)
                notesGenerator.GenerateLeftFlickNotes(spawnNote.id, spawnNote.beat);
            else if (spawnNote.type == "flick" && spawnNote.direction == FlickDirection.right)
                notesGenerator.GenerateRightFlickNotes(spawnNote.id, spawnNote.beat);
            else Debug.LogWarning($"[NotesManager] 未対応のノーツタイプです: {spawnNote.type}");
        }
    }

    // laneID が有効かどうか（GameManager からの安全確認用）
    public bool IsLaneValid(int laneID)
    {
        return notesGenerators != null && laneID >= 0 && laneID < notesGenerators.Length;
    }
    public void InitNotesGenerators(GameSettings gameSettings)
    {
        // タップノーツプールの初期化
        GameObject tapGameObject = new GameObject("TapNotesPool");
        tapGameObject.transform.SetParent(transform, false);
        NotesPool tapNotesPool = tapGameObject.AddComponent<NotesPool>();
        tapNotesPool.Initialize(tapNotesPrefab.GetComponent<TapNotes>(), DISPLAY_OUTSIDE);
        tapNotesPool.Prewarm(TAP_PREWARM);

        // ホールドノーツプールの初期化
        GameObject holdGameObject = new GameObject("HoldNotesPool");
        holdGameObject.transform.SetParent(transform, false);
        NotesPool holdNotesPool = holdGameObject.AddComponent<NotesPool>();
        holdNotesPool.Initialize(holdNotesPrefab.GetComponent<HoldNotes>(), DISPLAY_OUTSIDE);
        holdNotesPool.Prewarm(HOLD_PREWARM);

        // フリックノーツプールの初期化
        GameObject leftFlickGameObject = new GameObject("LeftFlickNotesPool");
        leftFlickGameObject.transform.SetParent(transform, false);
        NotesPool leftFlickNotesPool = leftFlickGameObject.AddComponent<NotesPool>();
        leftFlickNotesPool.Initialize(leftFlickNotesPrefab.GetComponent<FlickNotes>(), DISPLAY_OUTSIDE);
        leftFlickNotesPool.Prewarm(FLICK_PREWARM);

        // フリックノーツプールの初期化
        GameObject rightFlickGameObject = new GameObject("RightFlickNotesPool");
        rightFlickGameObject.transform.SetParent(transform, false);
        NotesPool rightFlickNotesPool = rightFlickGameObject.AddComponent<NotesPool>();
        rightFlickNotesPool.Initialize(rightFlickNotesPrefab.GetComponent<FlickNotes>(), DISPLAY_OUTSIDE);
        rightFlickNotesPool.Prewarm(FLICK_PREWARM);

        foreach (var genObj in notesGenerators)
        {
            var gen = genObj.GetComponent<NotesGenerator>();
            if (gen == null) continue;
            gen.SetGameSettings(gameSettings);
            gen.SetNotesObjects(tapNotesPool, holdNotesPool, leftFlickNotesPool, rightFlickNotesPool);
        }
    }
}
