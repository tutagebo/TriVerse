using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNotes : Notes
{
    private double noteEndBeat; // ホールドノーツの終了拍位置
    [SerializeField]
    private LineRenderer line;
    private float length;
    private bool isHolding = false; // 判定ラインにいるかどうか
    void Update()
    {
        double nowBeat = TimingManager.GetNowBeat(gameSettings);
        if (noteBeat > nowBeat) // ノーツが判定ラインより上にいる場合
        {
            float dy = (float)((noteBeat - nowBeat) * speedScale);
            transform.position = judgePosition + Vector3.up * dy;
        } else if(noteEndBeat > nowBeat) { // 判定ライン上にいる場合は下端を固定
            transform.position = judgePosition;
            float dy = (float)((noteEndBeat - nowBeat) * speedScale);
            line.SetPosition(1, Vector3.up *  dy);
        } else if(isHolding == false) { // ノーツが判定ラインより下にいて、ホールドされていない場合
            float dy = (float)((noteEndBeat - nowBeat) * speedScale);
            transform.position = judgePosition + Vector3.up * dy;
        }
    }
    public Notes SetInfo(NotesID notesID, NotesPool pool, double startBeat, double endBeat, GameSettings settings)
    {
        speedScale = settings.noteSpeed;
        length = (float)((endBeat - startBeat) * speedScale);
        notesPool = pool;
        SetID(notesID);
        noteBeat = startBeat;
        noteEndBeat = endBeat;

        line.positionCount = 2;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.up * length);
        return this;
    }
    // 判定ライン上で下端の位置が固定されるように調整
    public void SetHoldingFlag(bool isHolding)
    {
        this.isHolding = isHolding;
    }
}
