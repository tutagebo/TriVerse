using System.Collections.Generic;
using UnityEngine;

public class FlickEffectPool : MonoBehaviour
{
    [SerializeField] private ParticleSystem leftFlickEffectPrefab;
    [SerializeField] private ParticleSystem rightFlickEffectPrefab;
    private int poolSize = 3; // 同時3つ想定なので5くらい

    private GameSettings gameSettings;
    private readonly List<ParticleSystem> leftPool  = new List<ParticleSystem>();
    private readonly List<ParticleSystem> rightPool = new List<ParticleSystem>();
    public void Init(GameSettings settings)
    {
        gameSettings = settings;
        // 左フリック
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem fx = Instantiate(leftFlickEffectPrefab, transform);
            fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            leftPool.Add(fx);
        }
        // 右フリック
        for (int i = 0; i < poolSize; i++)
        {
            ParticleSystem fx = Instantiate(rightFlickEffectPrefab, transform);
            fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rightPool.Add(fx);
        }
    }
    public void PlayFlick(int group, FlickDirection direction)
    {
        Vector3 playPos = LaneCalculator.LaneToPosition(1, group, gameSettings.noteJudgeHeight, 0f);
        ParticleSystem particleSystem = GetAvailableEffect(direction);

        particleSystem.transform.position = playPos;
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 状態リセット
        particleSystem.Play();
    }
    private ParticleSystem GetAvailableEffect(FlickDirection direction)
    {
        List<ParticleSystem> pool = (direction == FlickDirection.left) ? leftPool : rightPool;
        foreach (ParticleSystem fp in pool)
        {
            if (!fp.isPlaying) return fp;
        }
        // すべて使用中なら先頭を返す
        return pool[0];
    }
}