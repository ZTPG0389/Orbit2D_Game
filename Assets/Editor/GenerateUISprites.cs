#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class GenerateUISprites
{
    static string savePath = "Assets/Resources/Sprites/UI/";

    [MenuItem("Tools/Generate UI Sprites")]
    static void Generate()
    {
        Directory.CreateDirectory(savePath);
        CreateProgressBg();
        CreateProgressFill();
        CreateBackButton();
        AssetDatabase.Refresh();

        foreach (string name in new[] { "progress_bar_bg", "progress_bar_fill", "back_button" })
            SetSpriteImport(savePath + name + ".png");

        Debug.Log("[GenerateUISprites] Done — 3 sprites in " + savePath);
    }

    static void CreateProgressBg()
    {
        int w = 800, h = 40;
        Texture2D tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        Color bg          = new Color(0.04f, 0.08f, 0.2f,  1f);
        Color border      = new Color(0f,    0.6f,  0.9f,  0.8f);
        Color transparent = new Color(0, 0, 0, 0);
        int r = 20, bw = 2;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool inOuter = IsInRoundedRect(x, y, w, h, r);
            int  ix = x - bw, iy = y - bw;
            bool inInner = ix >= 0 && iy >= 0 &&
                           IsInRoundedRect(ix, iy, w - bw * 2, h - bw * 2, r - bw);

            if (!inOuter)      pixels[y * w + x] = transparent;
            else if (!inInner) pixels[y * w + x] = border;
            else               pixels[y * w + x] = bg;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SavePNG(tex, savePath + "progress_bar_bg.png");
    }

    static void CreateProgressFill()
    {
        int w = 800, h = 40;
        Texture2D tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        Color transparent = new Color(0, 0, 0, 0);
        int r = 20;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            if (!IsInRoundedRect(x, y, w, h, r))
            {
                pixels[y * w + x] = transparent;
                continue;
            }

            float t    = (float)x / w;
            Color fill = Color.Lerp(
                new Color(0.1f, 0.7f, 1f,   1f),
                new Color(0f,   0.5f, 0.9f, 1f), t);

            // Top edge glow (y = h-1 is visual top in Unity texture coords)
            if (y > h - 6)
                fill = Color.Lerp(fill, new Color(0.5f, 0.9f, 1f, 1f), 0.6f);

            pixels[y * w + x] = fill;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SavePNG(tex, savePath + "progress_bar_fill.png");
    }

    static void CreateBackButton()
    {
        int w = 400, h = 100;
        Texture2D tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        Color bg          = new Color(0.06f, 0.12f, 0.28f, 1f);
        Color border      = new Color(0f,    0.7f,  1f,    0.9f);
        Color transparent = new Color(0, 0, 0, 0);
        int r = 20, bw = 3;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool inOuter = IsInRoundedRect(x, y, w, h, r);
            int  ix = x - bw, iy = y - bw;
            bool inInner = ix >= 0 && iy >= 0 &&
                           IsInRoundedRect(ix, iy, w - bw * 2, h - bw * 2, r - bw);

            if (!inOuter)      pixels[y * w + x] = transparent;
            else if (!inInner) pixels[y * w + x] = border;
            else               pixels[y * w + x] = bg;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        SavePNG(tex, savePath + "back_button.png");
    }

    static bool IsInRoundedRect(int x, int y, int w, int h, int r)
    {
        if (x < 0 || x >= w || y < 0 || y >= h) return false;
        if (x >= r && x < w - r)                 return true;
        if (y >= r && y < h - r)                 return true;
        int cx = x < r ? r : w - r - 1;
        int cy = y < r ? r : h - r - 1;
        return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;
    }

    static void SavePNG(Texture2D tex, string path)
    {
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log("[GenerateUISprites] Saved: " + path);
    }

    static void SetSpriteImport(string assetPath)
    {
        var imp = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (imp == null) return;
        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled       = false;
        imp.SaveAndReimport();
    }
}
#endif
