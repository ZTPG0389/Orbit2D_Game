using System.Collections;
using UnityEngine;
using TMPro;

public class EnemyWarning : MonoBehaviour
{
    public static EnemyWarning Instance;

    [SerializeField] TextMeshProUGUI warningText;

    void Awake()
    {
        Instance = this;
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public void ShowWarning(int edge) => StartCoroutine(FlashWarning(edge));

    IEnumerator FlashWarning(int edge)
    {
        if (warningText == null) yield break;

        string side = edge == 0 ? "TOP"
                    : edge == 1 ? "BOTTOM"
                    : edge == 2 ? "LEFT" : "RIGHT";
        warningText.text  = "⚠ WARNING! " + side;
        warningText.color = Color.red;

        // Move warning text to the approaching edge
        var rt = warningText.GetComponent<RectTransform>();
        if (rt != null)
        {
            switch (edge)
            {
                case 0: rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                        rt.anchoredPosition = new Vector2(0f, -60f);  break;
                case 1: rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                        rt.anchoredPosition = new Vector2(0f,  60f);  break;
                case 2: rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                        rt.anchoredPosition = new Vector2(80f,  0f);  break;
                case 3: rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                        rt.anchoredPosition = new Vector2(-80f, 0f);  break;
            }
        }

        warningText.gameObject.SetActive(true);

        // Flash 3 times (6 half-steps × 0.3 s = 1.8 s total)
        for (int i = 0; i < 6; i++)
        {
            warningText.alpha = i % 2 == 0 ? 1f : 0f;
            yield return new WaitForSeconds(0.3f);
        }

        warningText.gameObject.SetActive(false);
    }
}
