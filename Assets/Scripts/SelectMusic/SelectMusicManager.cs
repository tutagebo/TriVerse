using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMusicManager : MonoBehaviour
{
    [SerializeField]
    private GlobalData globalData;

    [SerializeField]
    private MusicDatabase musicDatabase;

    [Header("UI")]
    [SerializeField]
    private Transform contentRoot;     // ScrollView/Viewport/Content
    [SerializeField]
    private MusicButton buttonPrefab;
    [SerializeField]
    private JacketController jacketController;

    private void Start()
    {
        GenerateButtons();
        FadeRenderer.Instance.FadeIn(1f);
    }

    public void GenerateButtons()
    {
        if (musicDatabase == null || musicDatabase.songs == null) return;

        // 既存をクリア（差し替え時のため）
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
        // とりあえず最初の曲をジャケットに表示
        jacketController.ChangeMusicData(musicDatabase.songs[0]);
        globalData.selectedMusic = musicDatabase.songs[0];
        // 他の難易度を実装していないため、とりあえずExpertに固定
        jacketController.ChangeDifficulty(Difficulty.Expert);
        globalData.selectedDifficulty = Difficulty.Expert;
        // ボタン生成
        foreach (MusicDataSO song in musicDatabase.songs)
        {
            MusicButton item = Instantiate(buttonPrefab, contentRoot);
            item.Init(song, jacketController, globalData);
        }
    }
}
