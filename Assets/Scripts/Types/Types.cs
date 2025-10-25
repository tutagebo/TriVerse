using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreData
{
    public RankType rank;
    public int score;
    public int combo;
    public int maxCombo;
    public int fast;
    public int late;
    public int perfects;
    public int greats;
    public int goods;
    public int misses;
};

public struct JudgeScoreTable
{
    public float perfect;
    public float great;
    public float good;
    public float miss;
}

public struct MusicData
{
    public string title;
    public string artist;
    public string chartFolderName;
    public string musicFileName;
    public string jacketFileName;
}

public struct ResultData
{
    public ScoreData scoreData;
    public int score;
    public int rank;
    public float accuracy;
}
