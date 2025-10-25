using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "Rhythm/MusicData")]
public class MusicDataSO : ScriptableObject
{
    [Tooltip("ユニークID（GUID推奨）")]
    public string id;
    public string title;
    public string artist;

    [Header("Assets")]
    public AudioClip audio;
    public Sprite jacket;

    [Header("Charts")]
    public TextAsset[] charts;

    [Header("Levels")]
    public int basicLevel;
    public int hardLevel;
    public int expertLevel;
}