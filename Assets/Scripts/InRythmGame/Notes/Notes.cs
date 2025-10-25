using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notes : MonoBehaviour
{
    protected GameSettings gameSettings;
    public NotesID id { get; private set; }  // ノーツの一意ID
    protected double noteBeat;                 // ノーツの拍
    protected float speedScale;           // ユーザー設定倍率
    protected Vector3 judgePosition; // 判定位置
    protected NotesPool notesPool;
    public bool isReleased { get; private set; } = false;

    /* // debug
    int beforeBeat;
    // debug */

    void Update()
    {
        double nowBeat = TimingManager.GetNowBeat(gameSettings);
        float dy = (float)((noteBeat - nowBeat) * speedScale);
        transform.position = judgePosition + Vector3.up * dy;

        /* // debug 
        if (Mathf.FloorToInt((float)(noteBeat - nowBeat)) != beforeBeat)
        {
            Debug.Log(judgePosition);
            Debug.Log(Mathf.FloorToInt((float)(noteBeat - nowBeat)));
        }
        beforeBeat = Mathf.FloorToInt((float)(nowBeat));
        // debug */
    }
    public void SetGameSettings(GameSettings settings, Vector3 spawnPos)
    {
        gameSettings = settings;
        speedScale = gameSettings.noteSpeed;
        judgePosition = new Vector3(spawnPos.x, gameSettings.noteJudgeHeight, spawnPos.z);
        GameObject.Find("Judge").GetComponent<JudgeSystem>().RegisterNotesObject(this);
    }
    public void SetID(NotesID notesID)
    {
        id = notesID;
        isReleased = false;
    }
    public void Reset()
    {
        id = 0;
        isReleased = false;
        transform.position = new Vector3(0, 10, 0);
    }
    public void Release()
    {
        Debug.Log($"Release Note ID: {id} at Beat: {noteBeat}");
        if (isReleased) return;
        notesPool.Release(this);
        isReleased = true;
    }
}
