using UnityEngine;

public class BackgroundPlanet : MonoBehaviour
{
    public static BackgroundPlanet Instance;
    [SerializeField] SpriteRenderer bgRenderer;
    [SerializeField] Sprite[] planetSprites;

    void Awake() => Instance = this;

    void Start() => FitToScreen();

    public void SetPlanetForLevel(int level)
    {
        if (bgRenderer == null || planetSprites == null) return;
        int index = (level - 1) % planetSprites.Length;
        bgRenderer.sprite = planetSprites[index];
        FitToScreen();
    }

    void FitToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        if (bgRenderer.sprite == null) return;
        float spriteWidth = bgRenderer.sprite.bounds.size.x;
        float spriteHeight = bgRenderer.sprite.bounds.size.y;

        // Cover mode — use larger scale so no black bars
        float scaleX = camWidth / spriteWidth;
        float scaleY = camHeight / spriteHeight;
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
        transform.position = new Vector3(0f, 0f, 10f);
    }
}
