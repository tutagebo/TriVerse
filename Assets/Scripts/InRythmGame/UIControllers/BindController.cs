using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindController : MonoBehaviour
{
    public float shakeDuration = 0.15f; // 揺れの長さ
    public float shakeStrength = 0.1f;  // 揺れの強さ
    public float shakeSpeed = 40f;      // 揺れの速さ
    private int nowPos = 1;
    public void MoveLeft()
    {
        if (nowPos > 0) nowPos--;
        gameObject.transform.position = (nowPos - 1) * new Vector3(4.0f, 0, 0);
        StopAllCoroutines(); // 連打しても上書きされるように
        StartCoroutine(ShakeOnce());
    }
    public void MoveRight()
    {
        if (nowPos < 2) nowPos++;
        gameObject.transform.position = (nowPos - 1) * new Vector3(4.0f, 0, 0);
        StopAllCoroutines(); // 連打しても上書きされるように
        StartCoroutine(ShakeOnce());
    }
    IEnumerator ShakeOnce()
    {
        Vector3 original = transform.localPosition;
        float timer = 0f;

        while (timer < shakeDuration)
        {
            // 振動の減衰（だんだん弱まる）
            float decay = 1f - (timer / shakeDuration);

            // Mathf.Sin で左右に振る（時間×速度）
            float offset = Mathf.Sin(Time.time * shakeSpeed) * shakeStrength * decay;

            transform.localPosition = original + Vector3.right * offset;

            timer += Time.deltaTime;
            yield return null;
        }

        // 終了後、元の位置に戻す
        transform.localPosition = original;
    }
    public int GetNowPos()
    {
        return nowPos;
    }
}
