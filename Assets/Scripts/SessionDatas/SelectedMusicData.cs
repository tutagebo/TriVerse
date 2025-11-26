using UnityEngine;

[CreateAssetMenu(menuName = "Game/Selected Music Data", fileName = "SelectedMusicData")]
public class SelectedMusicData : ScriptableObject
{
    public int musicId;
    public int difficulty;

    // 将来的にここにオプション増やしてOK
    public float speed = 1.0f;
    public bool isMirror;

    public void Clear()
    {
        musicId = 0;
        difficulty = 0;
        speed = 1.0f;
        isMirror = false;
    }
}
