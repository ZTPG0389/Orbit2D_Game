using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateLevelSelectSprites
{
    const string SavePath = "Assets/Resources/Sprites/UI/";

    [MenuItem("Tools/Generate Level Select Sprites")]
    static void Generate()
    {
        Directory.CreateDirectory(SavePath);

        CreateRoundedButton();
        CreateLockIcon();
        CreateStarFilled();
        CreateStarEmpty();
        CreateGlowBorder();
        CreateBackground();

        AssetDatabase.Refresh();

        // Reimport every PNG as Sprite so Resources.Load<Sprite>() works at runtime.
        string[] names = { "level_button", "lock_icon", "star_filled", "star_empty", "glow_border", "space_bg" };
        foreach (string n in names)
        {
            string assetPath = SavePath + n + ".png";
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;
            importer.textureType     = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        Debug.Log("[GenerateLevelSelectSprites] All sprites saved to " + SavePath);
    }

    // ── 1. Level button background ────────────────────────────────
    static void CreateRoundedButton()
    {
        int size = 200;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px   = new Color[size * size];
        var btn  = new Color(0.12f, 0.32f, 0.72f, 1f);
        var none = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            px[y * size + x] = InRoundedRect(x, y, size, size, 30) ? btn : none;

        Save(tex, px, SavePath + "level_button.png");
    }

    // ── 2. Lock icon ──────────────────────────────────────────────
    static void CreateLockIcon()
    {
        int size = 128;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px   = new Color[size * size];
        var gold = new Color(1f, 0.71f, 0f, 1f);
        var dark = new Color(0.02f, 0.05f, 0.15f, 1f);
        var none = new Color(0, 0, 0, 0);

        for (int i = 0; i < px.Length; i++) px[i] = none;

        // Body rectangle
        for (int y = 10; y < 60; y++)
        for (int x = 35; x < 93; x++)
            px[y * size + x] = gold;

        // Shackle: hollow arc — ring between innerR and outerR, upper half only
        int cx = 64, cy = 60, rOuter = 28, rInner = 20;
        for (int y = cy; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            if (d >= rInner && d <= rOuter)
                px[y * size + x] = gold;
        }

        // Keyhole circle
        int kcx = 64, kcy = 35, kr = 9;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x - kcx) * (x - kcx) + (y - kcy) * (y - kcy));
            if (d <= kr)
                px[y * size + x] = dark;
        }

        Save(tex, px, SavePath + "lock_icon.png");
    }

    // ── 3. Filled star (cyan) ─────────────────────────────────────
    static void CreateStarFilled()
    {
        int size = 64;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px   = new Color[size * size];
        var cyan = new Color(0f, 0.87f, 1f, 1f);
        var none = new Color(0, 0, 0, 0);

        var c = new Vector2(32, 32);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            px[y * size + x] = InStar(x, y, c, 28f, 12f, 5) ? cyan : none;

        Save(tex, px, SavePath + "star_filled.png");
    }

    // ── 4. Empty star (dark grey) ─────────────────────────────────
    static void CreateStarEmpty()
    {
        int size = 64;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px   = new Color[size * size];
        var grey = new Color(0.20f, 0.20f, 0.32f, 0.70f);
        var none = new Color(0, 0, 0, 0);

        var c = new Vector2(32, 32);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            px[y * size + x] = InStar(x, y, c, 28f, 12f, 5) ? grey : none;

        Save(tex, px, SavePath + "star_empty.png");
    }

    // ── 5. Glow border ────────────────────────────────────────────
    static void CreateGlowBorder()
    {
        int size = 200, bw = 4, r = 30;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px   = new Color[size * size];
        var glow = new Color(0f, 0.70f, 1f, 0.85f);
        var none = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool outer = InRoundedRect(x, y, size, size, r);
            // Shift coords inward by bw to test against the inner (smaller) rect
            bool inner = InRoundedRect(x - bw, y - bw, size - bw * 2, size - bw * 2, r - bw);
            px[y * size + x] = (outer && !inner) ? glow : none;
        }

        Save(tex, px, SavePath + "glow_border.png");
    }

    // ── 6. Space background ───────────────────────────────────────
    static void CreateBackground()
    {
        // Using 270×480 (1/4 of 1080×1920) keeps file size reasonable at runtime;
        // Unity stretches it to fill the canvas anyway.
        int w = 270, h = 480;
        var tex  = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px   = new Color[w * h];
        var dark = new Color(0.02f, 0.04f, 0.12f, 1f);

        for (int i = 0; i < px.Length; i++) px[i] = dark;

        // Scatter star dots
        var rng = new System.Random(42);
        for (int s = 0; s < 80; s++)
        {
            int sx = rng.Next(0, w);
            int sy = rng.Next(0, h);
            float b = (float)rng.NextDouble() * 0.55f + 0.30f;
            var sc = new Color(b, b, b + 0.10f, 1f);
            px[sy * w + sx] = sc;
            if (sx + 1 < w)  px[sy * w + sx + 1] = sc * 0.45f;
            if (sy + 1 < h)  px[(sy + 1) * w + sx] = sc * 0.45f;
        }

        Save(tex, px, SavePath + "space_bg.png");
    }

    // ── Helpers ───────────────────────────────────────────────────

    // Returns true if (x,y) lies inside a rounded rectangle at origin (0,0)
    // with dimensions w×h and corner radius r.
    static bool InRoundedRect(int x, int y, int w, int h, int r)
    {
        if (x < 0 || x >= w || y < 0 || y >= h) return false;
        if (x >= r && x <= w - r) return true;   // middle vertical strip
        if (y >= r && y <= h - r) return true;   // middle horizontal strip
        // Corner: find nearest corner center and check distance
        int cx = x < r ? r : w - r;
        int cy = y < r ? r : h - r;
        return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;
    }

    // Returns true if (px,py) lies inside a regular N-pointed star.
    // The star is rotated so the first point faces straight up (positive Y).
    static bool InStar(int px, int py, Vector2 c, float outerR, float innerR, int points)
    {
        // Subtract PI/2 so angle=0 in sector space corresponds to the top of the texture
        float angle   = Mathf.Atan2(py - c.y, px - c.x) - Mathf.PI * 0.5f;
        float dist    = Vector2.Distance(new Vector2(px, py), c);
        float sector  = Mathf.PI * 2f / points;
        float norm    = ((angle % sector) + sector) % sector;   // [0, sector)
        float half    = sector * 0.5f;
        float t       = norm / half;                             // [0, 2)
        float r       = t <= 1f
            ? Mathf.Lerp(outerR, innerR, t)
            : Mathf.Lerp(innerR, outerR, t - 1f);
        return dist <= r;
    }

    static void Save(Texture2D tex, Color[] px, string path)
    {
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        Debug.Log("[GenerateLevelSelectSprites] Saved: " + path);
    }
}
