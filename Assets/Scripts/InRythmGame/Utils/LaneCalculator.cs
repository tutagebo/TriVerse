using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneCalculator
{
    const float GROUP_GAP = 4f;
    const float LANE_GAP = 1f;
    const int LANES_PER_GROUP = 3;
    static public Vector3 LaneToPosition(int lane, int group, float yPosition, float zPosition)
    {
        float xPosition = (lane-1) * LANE_GAP + (group-1) * GROUP_GAP;
        return new Vector3(xPosition, yPosition, zPosition);
    }
    static public int LaneToIndex(int lane, int group)
    {
        return lane + group * LANES_PER_GROUP;
    }
    /// <returns>(lane, group)</returns>
    static public (int, int) IndexToLane(int index)
    {
        return (index % LANES_PER_GROUP, index / LANES_PER_GROUP);
    }
}
