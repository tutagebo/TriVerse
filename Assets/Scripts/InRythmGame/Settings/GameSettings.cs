using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("ノーツ速度")]
    [Range(0.5f, 3f)]
    public float noteSpeed = 1.0f;

    [Header("判定ウィンドウ(ms)")]
    public double perfectMS = 35;
    public double greatMS = 60;
    public double goodMS = 90;
    public double missMS = 120;

    [Header("判定スコアテーブル")]
    public JudgeScoreTable judgeScoreTable = new JudgeScoreTable
    {
        perfect = 1.0f,
        great = 0.85f,
        good = 0.5f,
        miss = 0.0f
    };

    [Header("Rank判定閾値")]
    public int rankSSS = 997000;
    public int rankSS = 990000;
    public int rankS = 970000;
    public int rankA = 900000;
    public int rankB = 750000;
    public int rankC = 600000;

    [Header("内部設定")]
    public float noteSpawnHeight = 6.0f;
    public float noteJudgeHeight = -4.0f;

    [Header("音楽設定")]
    public float musicVolume = 1.0f;

    // 譜面の設定
    public double startGameDsp { get; private set; }
    public double startMusicDsp { get; private set; }
    public BpmPoint[] bpmPoints { get; private set; }
    public int userLatencyMs = 0;     // ユーザーレイテンシ（ms）
    public void SetStartDspTime(double gameStart, double leadIn)
    {
        startGameDsp = gameStart;
        startMusicDsp = gameStart + leadIn;
    }
    public void SetBpmPoints(BpmPoint[] points)
    {
        bpmPoints = points;
    }
    public void SetUserSettings(int offset, int speed)
    {
        userLatencyMs = offset;
        noteSpeed = speed / 4.0f;
    }
}
