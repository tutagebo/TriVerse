using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    private TapEffectPool tapEffectPool;

    public void Init(GameSettings settings)
    {
        tapEffectPool.Init(settings);
    }

    public void PlayTapEffect(int lane, int group)
    {
        tapEffectPool.PlayEffect(lane, group);
    }
}
