using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JacketController : MonoBehaviour
{
    [SerializeField]
    private RawImage jacketImage;
    [SerializeField]
    private RawImage difficultyFrame;
    [SerializeField]
    private Texture[] difficultyFrameTextures;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI artistText;
    [SerializeField]
    private TextMeshProUGUI levelText;
    private MusicDataSO nowMusicData;
    public void ChangeMusicData(MusicDataSO musicData)
    {
        nowMusicData = musicData;
        // ジャケット画像の変更処理等
        jacketImage.texture = musicData.jacket.texture;
        titleText.text = musicData.title;
        artistText.text = musicData.artist;
    }
    public void ChangeDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Basic:
                difficultyFrame.texture = difficultyFrameTextures[0];
                levelText.text = nowMusicData.basicLevel.ToString();
                break;
            case Difficulty.Hard:
                difficultyFrame.texture = difficultyFrameTextures[1];
                levelText.text = nowMusicData.hardLevel.ToString();
                break;
            case Difficulty.Expert:
                difficultyFrame.texture = difficultyFrameTextures[2];
                levelText.text = nowMusicData.expertLevel.ToString();
                break;
        }
    }
}
