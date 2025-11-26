using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesTypeButton : MonoBehaviour
{
    public NotesType notesType { get; private set; }
    public void OnClick_TapButton()
    {
        notesType = NotesType.Tap;
    }

    public void OnClick_FlickButton()
    {
        notesType = NotesType.Flick;
    }

    public void OnClick_HoldButton()
    {
        notesType = NotesType.Hold;
    }
}
