using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapEffectController : MonoBehaviour
{
    [SerializeField]
    private LineRenderer line;
    private float lifeTime = 0.2f;
    private const float START_HEIGHT = -5f;

    public void Init(GameSettings settings)
    {
        gameObject.SetActive(false);
        line.SetPosition(0, new Vector3(transform.position.x, START_HEIGHT, 0f));
        line.SetPosition(1, new Vector3(transform.position.x, 0f, 0f));
    }

    public void PlayEffect()
    {
        gameObject.SetActive(true);
        StartCoroutine(ExecuteFade());
    }

    private IEnumerator ExecuteFade()
    {
        float nowTime = 0f;

        while (nowTime < lifeTime)
        {
            nowTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, nowTime / lifeTime);
            Color startColor = line.startColor;
            startColor.a = alpha;
            line.material.color = startColor;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
