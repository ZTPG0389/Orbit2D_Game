using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingScoreUI : MonoBehaviour
{
    public static FloatingScoreUI Instance;

    [SerializeField] GameObject popupPrefab;

    void Awake() => Instance = this;

    public void ShowScore(Vector3 worldPos, int points)
    {
        if (popupPrefab == null) return;
        GameObject obj = Instantiate(popupPrefab);
        obj.transform.position = worldPos + Vector3.up * 0.5f;
        TMP_Text txt = obj.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = "+" + points;
        StartCoroutine(AnimatePopup(obj));
    }

    IEnumerator AnimatePopup(GameObject obj)
    {
        TMP_Text txt      = obj.GetComponentInChildren<TMP_Text>();
        float    t        = 0f;
        Vector3  startPos = obj.transform.position;

        while (t < 0.8f)
        {
            t += Time.deltaTime;
            obj.transform.position = startPos + Vector3.up * (t * 1.5f);
            if (txt != null)
            {
                Color c = txt.color;
                c.a       = 1f - (t / 0.8f);
                txt.color = c;
            }
            yield return null;
        }

        Destroy(obj);
    }
}
