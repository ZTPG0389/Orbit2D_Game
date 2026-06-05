using System.Collections;
using TMPro;
using UnityEngine;

public class BoomText : MonoBehaviour
{
    [SerializeField] float floatDistance = 0.6f;

    TMP_Text _label;
    Coroutine _anim;

    void Awake() => _label = GetComponent<TMP_Text>();

    public void Play(Vector3 worldPos)
    {
        transform.position = worldPos;
        gameObject.SetActive(true);
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(worldPos));
    }

    IEnumerator Animate(Vector3 origin)
    {
        const float DUR_SCALE_UP = 0.25f;
        const float DUR_SETTLE   = 0.15f;
        const float DUR_FADE     = 0.40f;
        const float TOTAL        = DUR_SCALE_UP + DUR_SETTLE + DUR_FADE; // 0.8 s

        Color c = _label.color;
        c.a = 1f;
        _label.color         = c;
        transform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < TOTAL)
        {
            elapsed += Time.unscaledDeltaTime;
            float globalT = Mathf.Clamp01(elapsed / TOTAL);

            // Float upward throughout the full duration
            transform.position = origin + Vector3.up * (floatDistance * globalT);

            if (elapsed < DUR_SCALE_UP)
            {
                float s = Mathf.Lerp(0f, 1.3f, elapsed / DUR_SCALE_UP);
                transform.localScale = new Vector3(s, s, 1f);
            }
            else if (elapsed < DUR_SCALE_UP + DUR_SETTLE)
            {
                float s = Mathf.Lerp(1.3f, 1.0f, (elapsed - DUR_SCALE_UP) / DUR_SETTLE);
                transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                transform.localScale = Vector3.one;
                c.a = Mathf.Lerp(1f, 0f, (elapsed - DUR_SCALE_UP - DUR_SETTLE) / DUR_FADE);
                _label.color = c;
            }

            yield return null;
        }

        gameObject.SetActive(false);
        _anim = null;
        BoomEffectPool.Instance?.Return(this);
    }
}
