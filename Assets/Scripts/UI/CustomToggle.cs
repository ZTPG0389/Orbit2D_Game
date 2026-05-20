using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    public RectTransform knob;
    public Image         background;
    public Color         onColor  = new Color(0.18f, 0.8f, 0.44f);
    public Color         offColor = new Color(0.4f,  0.4f, 0.4f);
    public Vector2       onPos    = new Vector2( 18f, 0f);
    public Vector2       offPos   = new Vector2(-18f, 0f);

    private bool isOn = true;
    public bool IsOn => isOn;
    public System.Action<bool> OnValueChanged;

    public void SetValue(bool value)
    {
        isOn = value;
        UpdateVisual();
    }

    public void Toggle()
    {
        isOn = !isOn;
        UpdateVisual();
        OnValueChanged?.Invoke(isOn);
    }

    void UpdateVisual()
    {
        knob.anchoredPosition = isOn ? onPos : offPos;
        background.color      = isOn ? onColor : offColor;
    }
}
