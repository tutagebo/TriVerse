using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapEffectPool : MonoBehaviour
{
    [SerializeField]
    private GameObject tapEffectPrefab;
    private const int LANE_COUNT = 9;
    private GameObject[] tapEffects = new GameObject[LANE_COUNT];
    public void Init(GameSettings settings)
    {
        for (int i = 0; i < LANE_COUNT; i++)
        {
            (int lane, int group) = LaneCalculator.IndexToLane(i);
            tapEffects[i] = Instantiate(tapEffectPrefab, LaneCalculator.LaneToPosition(lane, group, 0f, 0f), Quaternion.identity, transform);
            tapEffects[i].GetComponent<TapEffectController>().Init(settings);
        }
    }

    public void PlayEffect(int lane, int group)
    {
        Debug.Log($"Play Tap Effect: lane {lane}, group {group}");
        int index = LaneCalculator.LaneToIndex(lane, group);
        tapEffects[index].GetComponent<TapEffectController>().PlayEffect();
    }
}
