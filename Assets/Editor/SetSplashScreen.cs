#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SetSplashScreen
{
    private const string SpritePath = "Assets/Resources/Sprites/UI/Splash_img.png";

    [InitializeOnLoadMethod]
    static void AutoRun()
    {
        if (SessionState.GetBool("SetSplashScreen_done", false)) return;
        SessionState.SetBool("SetSplashScreen_done", true);
        EditorApplication.delayCall += Run;
    }

    [MenuItem("OrbitDestroyer/Setup Splash Screen")]
    public static void Run()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(SpritePath))
                if (asset is Sprite s) { sprite = s; break; }
        }

        if (sprite == null)
        {
            Debug.LogError($"[SetSplashScreen] Sprite not found at '{SpritePath}'.");
            return;
        }

        PlayerSettings.SplashScreen.show          = true;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SplashScreen.backgroundColor = Color.black;

        var logo = PlayerSettings.SplashScreenLogo.Create(2f, sprite);
        PlayerSettings.SplashScreen.logos = new[] { logo };

        PlayerSettings.SplashScreen.drawMode =
            PlayerSettings.SplashScreen.DrawMode.AllSequential;
        PlayerSettings.SplashScreen.unityLogoStyle =
            PlayerSettings.SplashScreen.UnityLogoStyle.DarkOnLight;

        AssetDatabase.SaveAssets();
        Debug.Log("[SetSplashScreen] Splash screen configured — " +
                  "sprite='Splash_img', Unity logo=disabled, background=black. " +
                  "Verify: Edit > Project Settings > Player > Splash Image.");
    }
}
#endif
