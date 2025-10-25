using UnityEngine;

[CreateAssetMenu(fileName = "MusicDatabase", menuName = "Rhythm/MusicDatabase")]
public class MusicDatabase : ScriptableObject
{
    public MusicDataSO[] songs;

    public MusicDataSO GetById(string id)
    {
        foreach (var s in songs)
            if (s.id == id) return s;
        return null;
    }
}
