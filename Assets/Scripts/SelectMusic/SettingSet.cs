using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingSet : MonoBehaviour
{
    [SerializeField]
    private GameObject settingPanel;
    [SerializeField]
    private GameSettings gameSettings;
    [SerializeField]
    private Slider offsetSlider;
    [SerializeField]
    private Slider speedSlider;

    public void OnClickSetting()
    {
        gameSettings.SetUserSettings((int)offsetSlider.value, (int)speedSlider.value);
        // 設定ボタンがクリックされたときの処理
        settingPanel.SetActive(false);
    }
}
