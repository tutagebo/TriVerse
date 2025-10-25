using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesGenerator : MonoBehaviour
{
    private NotesPool tapNotesPool;
    private NotesPool holdNotesPool;
    private NotesPool leftFlickNotesPool;
    private NotesPool rightFlickNotesPool;
    [SerializeField] private int laneID;
    GameSettings gameSettings;
    public void SetGameSettings(GameSettings settings)
    {
        gameSettings = settings;
    }
    public void SetNotesObjects(NotesPool tapNotes, NotesPool holdNotes, NotesPool leftFlickNotes, NotesPool rightFlickNotes)
    {
        tapNotesPool = tapNotes;
        holdNotesPool = holdNotes;
        leftFlickNotesPool = leftFlickNotes;
        rightFlickNotesPool = rightFlickNotes;
    }
    public void GenerateTapNotes(NotesID notesID, double beat)
    {
        Vector3 spawnPos = new Vector3(transform.position.x, gameSettings.noteSpawnHeight, transform.position.z);
        Notes note = tapNotesPool.Get();
        note.transform.position = spawnPos;
        note.GetComponent<TapNotes>()
            .SetInfo(notesID, tapNotesPool, beat)
            .SetGameSettings(gameSettings, spawnPos);
    }
    public void GenerateHoldNotes(NotesID notesID, double startBeat, double endBeat)
    {

        Vector3 spawnPos = new Vector3(transform.position.x, gameSettings.noteSpawnHeight, transform.position.z);
        Notes note = holdNotesPool.Get();
        note.transform.position = spawnPos;
        note.GetComponent<HoldNotes>()
            .SetInfo(notesID, holdNotesPool, startBeat, endBeat, gameSettings)
            .SetGameSettings(gameSettings, spawnPos);
    }
    public void GenerateLeftFlickNotes(NotesID notesID, double beat)
    {
        Vector3 spawnPos = new Vector3(transform.position.x, gameSettings.noteSpawnHeight, transform.position.z);
        Notes note = leftFlickNotesPool.Get();
        note.transform.position = spawnPos;
        note.GetComponent<FlickNotes>()
            .SetInfo(notesID, leftFlickNotesPool, beat)
            .SetGameSettings(gameSettings, spawnPos);
    }
    public void GenerateRightFlickNotes(NotesID notesID, double beat)
    {
    Vector3 spawnPos = new Vector3(transform.position.x, gameSettings.noteSpawnHeight, transform.position.z);
    Notes note = rightFlickNotesPool.Get();
    note.transform.position = spawnPos;
    note.GetComponent<FlickNotes>()
        .SetInfo(notesID, rightFlickNotesPool, beat)
        .SetGameSettings(gameSettings, spawnPos);
    }
}
