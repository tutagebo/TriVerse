using System;
using UnityEngine;

public class ScoreCalculator
{
    public static int CalculateScore(int perfectCount, int greatCount, int goodCount, int missCount)
    {
        int totalNotes = perfectCount + greatCount + goodCount + missCount;
        if (totalNotes == 0) return 0;

        // 各判定のスコア配分
        int perfectScore = 1000;
        int greatScore = 800;
        int goodScore = 500;
        int missScore = 0;

        // 合計スコアを計算
        int totalScore = (perfectCount * perfectScore) +
                         (greatCount * greatScore) +
                         (goodCount * goodScore) +
                         (missCount * missScore);

        // 最大スコアを計算
        int maxScore = totalNotes * perfectScore;

        // 最終スコアを割合で計算し、整数に変換
        int finalScore = (int)((totalScore / (float)maxScore) * 1000000);

        return finalScore;
    }

    
}
