using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip tapSound;
    [SerializeField]
    AudioClip perfectSound;
    [SerializeField]
    AudioClip greatSound;
    [SerializeField]
    AudioClip flickSound;
    [SerializeField]
    AudioClip flickAirSound;
    public void LaneTap()
    {
        audioSource.PlayOneShot(tapSound);
    }
    public void Judge(JudgeType judgeType)
    {
        if (judgeType == JudgeType.Perfect)
        {
            audioSource.PlayOneShot(perfectSound);
            return;
        }
        else
        {
            audioSource.PlayOneShot(greatSound);
            return;
        }
    }
    public void FlickJudge()
    {
        audioSource.PlayOneShot(flickSound);
    }
    public void FlickAir()
    {
        audioSource.PlayOneShot(flickAirSound);
    }
}
