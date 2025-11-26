using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using UnityEngine.UI;

public class ResultRenderer : MonoBehaviour
{
    [SerializeField]
    GlobalData globalData;
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI comboText;
    [SerializeField]
    private TextMeshProUGUI perfectText;
    [SerializeField]
    private TextMeshProUGUI greatText;
    [SerializeField]
    private TextMeshProUGUI goodText;
    [SerializeField]
    private TextMeshProUGUI missText;
    [SerializeField]
    private RawImage rankDisplay;
    [SerializeField]
    private Texture[] rankTextures;
    void Start()
    {
        ScoreData scoreData = globalData.scoreData;
        // スコアデータを使ってリザルト画面を表示
        scoreText.text = scoreData.score.ToString();
        comboText.text = scoreData.maxCombo.ToString();
        perfectText.text = scoreData.perfects.ToString();
        greatText.text = scoreData.greats.ToString();
        goodText.text = scoreData.goods.ToString();
        missText.text = scoreData.misses.ToString();

        // ランク表示
        rankDisplay.texture = rankTextures[(int)scoreData.rank];
        // TODO: FULL COMBOやALL PERFECTの表示

        // フェードイン
        FadeRenderer.Instance.FadeIn(1f);
    }
}
