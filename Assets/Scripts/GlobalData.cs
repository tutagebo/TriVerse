using UnityEngine;

public class GlobalData : MonoBehaviour
{
    // シングルトンインスタンス
    public static GlobalData Instance { get; private set; }

    // 渡したいデータ
    private MusicDataSO SelectedMusic;
    private Difficulty selectedDifficulty;
    private ScoreData scoreData;

    void Awake()
    {
        // すでに存在する場合は新しいものを破棄
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // インスタンス登録 & シーン切り替えで破棄しない
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public MusicDataSO GetSelectedMusic() => SelectedMusic;
    public void SetSelectedMusic(MusicDataSO music)
    {
        SelectedMusic = music;
    }
    public Difficulty GetSelectedDifficulty() => selectedDifficulty;
    public void SetSelectedDifficulty(Difficulty difficulty)
    {
        selectedDifficulty = difficulty;
    }
    public ScoreData GetScoreData() => scoreData;
    public void SetScoreData(ScoreData scoreData)
    {
        this.scoreData = scoreData;
    }
}

