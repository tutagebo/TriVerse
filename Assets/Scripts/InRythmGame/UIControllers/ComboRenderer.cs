using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComboRenderer : MonoBehaviour
{
    public TextMeshPro comboText;
    public void SetCombo(int combo)
    {
        if (combo <= 1)
        {
            comboText.text = "";
            comboText.enabled = false;
            return;
        }
        comboText.enabled = true;
        comboText.text = "Combo\n" + combo.ToString();
    }
    public void ResetCombo()
    {
        comboText.text = "";
        comboText.enabled = false;
    }
}
