using UnityEngine;

[CreateAssetMenu(fileName = "GlobalData", menuName = "Data/GlobalData")]
public class GlobalData : ScriptableObject
{
    [Header("選曲データ")]
    public MusicDataSO selectedMusic;
    public Difficulty selectedDifficulty;

    // プレイ結果
    public ScoreData scoreData;
    public void Clear()
    {
        selectedMusic = null;
        selectedDifficulty = Difficulty.Expert; // enumの初期値があるならこれ
        scoreData = new ScoreData();
    }
}
