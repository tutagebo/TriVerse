using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip music;
    public double leadInSec = 3.0;

    void Start() {
        audioSource.clip = music;
        double startDsp = AudioSettings.dspTime + leadInSec;
        audioSource.PlayScheduled(startDsp);
    }
}
