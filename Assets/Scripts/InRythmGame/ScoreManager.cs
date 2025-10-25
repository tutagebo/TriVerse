using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    private GameSettings gameSettings;
    private ScoreData scoreData;
    [SerializeField]
    private ComboRenderer comboRenderer;

    private const float MAX_SCORE = 1000000;
    public void Init(ComboRenderer comboRenderer, GameSettings gameSettings)
    {
        scoreData = new ScoreData();
        this.comboRenderer = comboRenderer;
        this.gameSettings = gameSettings;
    }
    public void AddPerfect()
    {
        scoreData.perfects++;
        scoreData.combo++;
        comboRenderer.SetCombo(scoreData.combo);
    }
    public void AddGreat()
    {
        scoreData.greats++;
        scoreData.combo++;
        comboRenderer.SetCombo(scoreData.combo);
    }
    public void AddGood()
    {
        scoreData.goods++;
        scoreData.combo++;
        comboRenderer.SetCombo(scoreData.combo);
    }
    public void AddMiss()
    {
        scoreData.misses++;
        if (scoreData.combo > scoreData.maxCombo)
        {
            scoreData.maxCombo = scoreData.combo;
        }
        comboRenderer.ResetCombo();
        scoreData.combo = 0;
    }
    public ScoreData GetScoreData()
    {
        return scoreData;
    }
    public int CountJudge(NotesItem[] notesItems)
    {
        int totalJudgeCount = 0;

        foreach (NotesItem note in notesItems)
        {
            switch (note.type)
            {
                case "tap":
                case "flick":
                    totalJudgeCount++;
                    break;

                case "hold":
                    {
                        // ホールドは始点1つ＋(1拍ごとのTick)を加算
                        // endBeat自体は判定対象外
                        double startBeat = Math.Floor(note.beat);
                        double validEnd = Math.Floor(note.endBeat - 1e-6);

                        // 今回は終点を除くので endBeat未満の整数までをカウント
                        int tickCount = (int)(validEnd - startBeat);
                        if (tickCount < 0) tickCount = 0;

                        // 始点を含める
                        totalJudgeCount += 1 + tickCount;
                        break;
                    }
            }
        }
        return totalJudgeCount;
    }
    public void calculateFinalScore(int totalJudgeCount, JudgeScoreTable judgeScoreTable)
    {
        // 最終スコア計算
        float totalScore = 0;
        totalScore += scoreData.perfects * judgeScoreTable.perfect * MAX_SCORE;
        totalScore += scoreData.greats * judgeScoreTable.great * MAX_SCORE;
        totalScore += scoreData.goods * judgeScoreTable.good * MAX_SCORE;
        totalScore += scoreData.misses * judgeScoreTable.miss * MAX_SCORE;
        totalScore /= totalJudgeCount;
        scoreData.score = Mathf.FloorToInt(totalScore);

        // ランク計算
        switch (true)
        {
            case true when scoreData.score >= gameSettings.rankSSS:
                scoreData.rank = RankType.SSS;
                break;
            case true when scoreData.score >= gameSettings.rankSS:
                scoreData.rank = RankType.SS;
                break;
            case true when scoreData.score >= gameSettings.rankS:
                scoreData.rank = RankType.S;
                break;
            case true when scoreData.score >= gameSettings.rankA:
                scoreData.rank = RankType.A;
                break;
            case true when scoreData.score >= gameSettings.rankB:
                scoreData.rank = RankType.B;
                break;
            case true when scoreData.score >= gameSettings.rankC:
                scoreData.rank = RankType.C;
                break;
            default:
                scoreData.rank = RankType.D;
                break;
        }
        return;
    }
}
