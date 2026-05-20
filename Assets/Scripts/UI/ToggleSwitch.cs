using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public Image         background;
    [SerializeField] public RectTransform knob;
    [SerializeField] public bool          isOn = true;

    public bool IsOn => isOn;
    public System.Action<bool> OnValueChanged;

    void Start()
    {
        UpdateVisual(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isOn = !isOn;
        UpdateVisual(true);
        OnValueChanged?.Invoke(isOn);
    }

    public void SetValue(bool value)
    {
        isOn = value;
        UpdateVisual(false);
    }

    void UpdateVisual(bool animate)
    {
        background.color = isOn
            ? new Color(0.18f, 0.8f, 0.44f)
            : new Color(0.4f,  0.4f, 0.4f);

        float targetX = isOn ? 18f : -18f;

        if (animate)
            StartCoroutine(MoveKnob(targetX));
        else
            knob.anchoredPosition = new Vector2(targetX, 0);
    }

    IEnumerator MoveKnob(float targetX)
    {
        float startX = knob.anchoredPosition.x;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            knob.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), 0);
            yield return null;
        }
        knob.anchoredPosition = new Vector2(targetX, 0);
    }
}
