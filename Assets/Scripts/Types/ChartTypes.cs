using System;
using UnityEngine;

[Serializable]
public class ChartRoot {
    public int version;
    public Timing timing;
    public Chart chart;
}

[Serializable]
public class Timing {
    public int offsetMs;
    public int resolution;
    public BpmPoint[] bpmPoints;
    public int endBeat;
}

[Serializable]
public class BpmPoint {
    public double beat;
    public double bpm;
}

[Serializable]
public class Chart {
    public string difficulty;
    public int lanes;
    public NotesItem[] notes;
    public ChartEvent[] events;
}

[Serializable]
public class NotesItem
{
    public int lane;       // 0..(lanes-1)
    public int group;      // レーン束のグループ番号
    public double beat;    // 拍位置
    public string type;    // "tap" / "hold" など（本例はtapのみ）
    public double endBeat; // hold用
    public FlickDirection direction;  // flick用
    [NonSerialized]
    public NotesID id;    // ノーツの一意ID
    
}

[Serializable]
public class ChartEvent
{
    public double beat;
    public string kind; // "scroll" / "fx" など
    public string name;
    public float mult;
}

public readonly struct NotesID
{
    public readonly int Value;
    public NotesID(int value) => Value = value;
    // int との暗黙的変換（キャスト不要で使える）
    public static explicit operator int(NotesID id) => id.Value;
    public static implicit operator NotesID(int value) => new NotesID(value);

    public static NotesID operator ++(NotesID id) => new NotesID(id.Value + 1);
    public override string ToString() => Value.ToString();
}
