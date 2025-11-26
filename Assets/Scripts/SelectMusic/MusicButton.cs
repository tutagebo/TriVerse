using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour
{
    private JacketController jacketController;
    public void Init(MusicDataSO musicData, JacketController jacketCtrl)
    {
        jacketController = jacketCtrl;
        // ボタンの表示内容初期化等
        // 例: ボタンのテキストに曲名とアーティスト名を設定
        Text buttonText = GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = musicData.title;
        }

        // ボタンクリック時の処理登録
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnClickMusic(musicData));
        }
    }

    private void OnClickMusic(MusicDataSO song)
    {
        GlobalData.Instance.SetSelectedMusic(song);
        jacketController.ChangeMusicData(song);
        jacketController.ChangeDifficulty(Difficulty.Expert);
        // Debug.Log($"選択: {song.title} / {song.artist}");
    }
}
