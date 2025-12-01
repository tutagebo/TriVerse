using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    private TapEffectPool tapEffectPool;
    [SerializeField]
    private FlickEffectPool flickEffectPool;

    public void Init(GameSettings settings)
    {
        tapEffectPool.Init(settings);
        flickEffectPool.Init(settings);
    }

    public void PlayTapEffect(int lane, int group)
    {
        tapEffectPool.PlayEffect(lane, group);
    }
    public void PlayFlickEffect(int group, FlickDirection direction)
    {
        flickEffectPool.PlayFlick(group, direction);
    }
}
