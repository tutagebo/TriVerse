using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickNotes : Notes
{
    [SerializeField]
    FlickDirection flickDirection;
    public Notes SetInfo(NotesID notesID, NotesPool pool, double beat)
    {
        notesPool = pool;
        SetID(notesID);
        noteBeat = beat;
        return this;
    }
}
