using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyWarning : MonoBehaviour
{
    public static EnemyWarning Instance;

    // Assign a ⚠ warning sprite in the Inspector.
    [SerializeField] Image warningImage;

    void Awake()
    {
        Instance = this;
        if (warningImage != null) warningImage.gameObject.SetActive(false);
    }

    // Returns IEnumerator so EnemySpawner can write:
    //   yield return StartCoroutine(EnemyWarning.Instance.ShowWarning(edge))
    // Execution resumes in SpawnEnemy() only AFTER the full flash sequence ends.
    public IEnumerator ShowWarning(int edge)
    {
        if (warningImage == null) yield break;

        // Position always centred — a centred icon is more visible than an
        // edge indicator on a small mobile screen.
        var rt = warningImage.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        // Force scale before showing so no animation or Inspector reset can shrink it.
        warningImage.rectTransform.localScale = new Vector3(3f, 1.5f, 1f);

        // Step 1 — show image
        warningImage.gameObject.SetActive(true);

        // Step 2 — play alert sound before first flash so audio leads visual
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Alert);

        // Step 3 — vibrate on warning (second buzz fires when ship actually spawns)
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif

        // Step 4 — flash 3 times: alpha 1→0→1→0→1→0, every 0.3 s
        // 6 half-steps × 0.3 s = 1.8 s total.
        // EnemySpawner.SpawnEnemy() spawns the ship the moment this loop ends.
        for (int i = 0; i < 6; i++)
        {
            warningImage.color = new Color(1f, 1f, 1f, i % 2 == 0 ? 1f : 0f);
            yield return new WaitForSeconds(0.3f);
        }

        // Step 5 — hide image; control returns to SpawnEnemy()
        warningImage.gameObject.SetActive(false);
    }
}
