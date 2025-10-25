using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SliderChange : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI sliderText;
    [SerializeField]
    private Slider slider;

    public void OnChangeSpeed()
    {
        float noteSpeed = slider.value / 4.0f;
        sliderText.text = noteSpeed.ToString("F2");
    }
    public void SetSpeedSlider(float speed)
    {
        sliderText.text = speed.ToString("F2");
        slider.value = Mathf.Floor(speed * 4.0f);
    }
    public void OnChangeOffset()
    {
        sliderText.text = slider.value.ToString() + " ms";
    }
    public void SetOffsetSlider(int offset)
    {
        sliderText.text = offset.ToString() + " ms";
        slider.value = offset;
    }
}
