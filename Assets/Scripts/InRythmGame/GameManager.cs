using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip music;
    [SerializeField]
    private NotesManager notesManager;
    [SerializeField]
    private JudgeSystem judgeSystem;
    [SerializeField]
    private SoundManager soundManager;
    [SerializeField]
    private GameSettings gameSettings;
    [SerializeField]
    private BindController bindController;
    [SerializeField]
    private bool isDebug = false; // デバッグモード
    [SerializeField]
    private int debugStartBeat = 0; // デバッグ用に任意の拍から開始する
    private ScoreManager scoreManager;
    private double startDspTime;
    private ChartRoot chartData;
    private int endBeat;
    private int beforeBeat = -999; // 充分に小さい値で初期化
    private List<int> holdingLanes = new List<int>(); // 押しっぱなしのレーン情報
    private bool isGameStarted = false;
    private int notesJudgeCount = 0;
    void Start()
    {
        // GlobalDataから選択された曲と難易度を取得
        MusicDataSO selectedMusic = GlobalData.Instance.GetSelectedMusic();
        Difficulty selectedDifficulty = GlobalData.Instance.GetSelectedDifficulty();
        
        TextAsset chartJson = selectedMusic.charts[(int)selectedDifficulty];

        // NotesManagerの各NotesGeneratorにGameSettingsをセット
        notesManager.InitNotesGenerators(gameSettings);
        // チャートデータの読み込み
        ChartReader reader = new ChartReader();
        chartData = reader.Parse(chartJson);
        audioSource.clip = music;
        // デバッグ用に任意の拍から開始
        if (isDebug)
        {
            StartDebug(debugStartBeat);
            return;
        }
        // ゲーム開始
        StartGame();
    }
    void Update()
    {
        if (!isGameStarted) return;
        // TODO: 設定で効果音消せるようにする
        JudgeType flickJudge = JudgeType.None;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            flickJudge = judgeSystem.JudgeFlick(bindController.GetNowPos(), FlickDirection.left);
            bindController.MoveLeft();
            soundManager.FlickAir();    // 効果音
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            flickJudge = judgeSystem.JudgeFlick(bindController.GetNowPos(), FlickDirection.right);
            bindController.MoveRight();
            soundManager.FlickAir();    // 効果音
        }
        if (flickJudge != JudgeType.None && flickJudge != JudgeType.Miss)
            soundManager.FlickJudge();
        holdingLanes.Clear();
        JudgeType judgeResult = JudgeType.None;
        // TODO: 判定が重なったときいいほうのを採用するようにする
        if (Input.GetKeyDown(KeyCode.A))
        {
            judgeResult = judgeSystem.ExecuteJudge(0, bindController.GetNowPos());
            soundManager.LaneTap();    // 効果音
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            judgeResult = judgeSystem.ExecuteJudge(1, bindController.GetNowPos());
            soundManager.LaneTap();    // 効果音
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            judgeResult = judgeSystem.ExecuteJudge(2, bindController.GetNowPos());
            soundManager.LaneTap();    // 効果音
        }
        if (judgeResult != JudgeType.None && judgeResult != JudgeType.Miss)
            soundManager.Judge(judgeResult);
        // 自然Miss判定の更新
        judgeSystem.UpdateJudge();
        // 拍ごとの更新
        int currentBeat = (int)TimingManager.GetNowBeat(gameSettings);
        if (beforeBeat < currentBeat)
        {
            if(Input.GetKey(KeyCode.A)) holdingLanes.Add(0);
            if(Input.GetKey(KeyCode.S)) holdingLanes.Add(1);
            if(Input.GetKey(KeyCode.D)) holdingLanes.Add(2);
            UpdateOnBeat(currentBeat);
            if (currentBeat >= endBeat) StartCoroutine(FinishGame());
        }
        beforeBeat = currentBeat;
    }
    // 1拍ごとの処理
    void UpdateOnBeat(int currentBeat)
    {
        judgeSystem.HoldNotesKeepJudge(holdingLanes, bindController.GetNowPos(), currentBeat);
    }
    void StartGame()
    {
        // 終了拍の取得
        endBeat = chartData.timing.endBeat;
        // スコアマネージャーはJudgeSystemで初期化
        scoreManager = new ScoreManager();
        notesJudgeCount = scoreManager.CountJudge(chartData.chart.notes);
        Debug.Log($"判定数: {notesJudgeCount}");
        const double beat = 12.0;
        // 拍 * ( 60秒 / BPM ) でリードイン秒数を計算
        double leadInSec = beat * 60.0 / chartData.timing.bpmPoints[0].bpm;
        startDspTime = AudioSettings.dspTime;
        gameSettings.SetStartDspTime(startDspTime, leadInSec);
        gameSettings.SetBpmPoints(chartData.timing.bpmPoints);
        // JudgeSystemにデータをセット
        judgeSystem.Init(chartData.chart.notes, scoreManager, gameSettings);
        // フェードイン
        FadeRenderer.Instance.FadeIn(1f);
        // 音楽再生
        audioSource.PlayScheduled(gameSettings.startMusicDsp - chartData.timing.offsetMs * 0.001);
        // ノーツスケジューリング
        notesManager.GenerateStart(chartData, gameSettings);
        isGameStarted = true;
    }
    // デバッグ用に任意の拍から開始する
    // 譜面エディタができたら削除予定
    void StartDebug(int startBeat)
    {
        const double leadInBeats = 8.0; // デバッグでもそのままでOK
        double leadInSec = leadInBeats * 60.0 / chartData.timing.bpmPoints[0].bpm;

        startDspTime = AudioSettings.dspTime;
        gameSettings.SetStartDspTime(startDspTime, leadInSec);
        gameSettings.SetBpmPoints(chartData.timing.bpmPoints);

        double startDelta = TimingManager.BeatToSeconds(startBeat, chartData.timing.bpmPoints);

        // 1) 再生位置をシーク（AudioClipのLoad Typeは "Decompress on Load" 推奨）
        audioSource.clip = music;
        audioSource.time = (float)startDelta; // timeSamples でもOK

        // 2) 通常のリードイン後の未来時刻へスケジュール
        double scheduleAt = gameSettings.startMusicDsp - chartData.timing.offsetMs * 0.001;
        // 念のため最短でも50ms未来に
        double minFuture = AudioSettings.dspTime + 0.05;
        if (scheduleAt < minFuture) scheduleAt = minFuture;

        audioSource.PlayScheduled(scheduleAt);

        // 3) ノーツ（startBeat以前を除外したチャート）で初期化
        ChartRoot debugChart = notesManager.GenerateDebugChart(chartData, gameSettings, startBeat);
        // スコアマネージャーはJudgeSystemで初期化
        scoreManager = new ScoreManager();
        judgeSystem.Init(debugChart.chart.notes, scoreManager, gameSettings);
        notesManager.GenerateStart(debugChart, gameSettings);

        isGameStarted = true;
    }
    IEnumerator FinishGame()
    {
        // フェードアウト
        yield return FadeRenderer.Instance.FadeCoroutine(0f, 1f, 1f);
        isGameStarted = false;
        Debug.Log("ゲーム終了");
        scoreManager.calculateFinalScore(notesJudgeCount, gameSettings.judgeScoreTable);
        ScoreData scoreData = scoreManager.GetScoreData();
        Debug.Log($"Perfect: {scoreData.perfects}\n Great: {scoreData.greats}\n Good: {scoreData.goods}\n Miss: {scoreData.misses}\n Combo: {scoreData.maxCombo}");

        GlobalData.Instance.SetSelectedMusic(null);
        GlobalData.Instance.SetScoreData(scoreData);
        // 結果画面へ遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
    }
}
