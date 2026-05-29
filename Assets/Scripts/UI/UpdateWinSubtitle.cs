using UnityEngine;
using TMPro;

public class UpdateWinSubtitle : MonoBehaviour
{
    void OnEnable()
    {
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);

        SetText("TitleText",    "You Win!");
        SetText("SubtitleText", "Level " + level + " cleared!");
        SetText("BonusText",    "+ " + (level * 50) + " Bonus!");
    }

    void SetText(string childName, string value)
    {
        var t = transform.Find(childName);
        if (t == null) return;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = value;
    }
}
