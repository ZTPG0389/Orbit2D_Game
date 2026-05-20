using UnityEngine;
using TMPro;

public class StarRatingUI : MonoBehaviour
{
    public static StarRatingUI Instance { get; private set; }

    [SerializeField] TMP_Text starText;

    void Awake() => Instance = this;

    public void ShowStars(int lives)
    {
        if (starText == null) return;
        int stars = lives >= 3 ? 3 : lives == 2 ? 2 : 1;
        starText.text = stars == 3 ? "⭐⭐⭐"
                      : stars == 2 ? "⭐⭐☆"
                      : "⭐☆☆";
    }
}
