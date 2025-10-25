using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingButton : MonoBehaviour
{
    [SerializeField]
    private GameObject settingPanel;
    [SerializeField]
    private GameSettings gameSettings;
    [SerializeField]
    private SliderChange offsetSlider;
    [SerializeField]
    private SliderChange speedSlider;

    public void OnClickSetting()
    {
        offsetSlider.SetOffsetSlider(gameSettings.userLatencyMs);
        speedSlider.SetSpeedSlider(gameSettings.noteSpeed);
        // 設定ボタンがクリックされたときの処理
        settingPanel.SetActive(true);
    }
}
