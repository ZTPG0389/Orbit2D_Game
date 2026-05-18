# gam-002 — Orbit Drop — Unity 3D Mobile Game (Android & iOS)

## Metadata
- **ID**: gam-002
- **Type**: feature
- **Status**: specification
- **Complexity**: HIGH
- **Platform**: Android & iOS
- **Created**: 2026-05-12

---

## Planning

### Description
Convert the fully playable HTML5 Canvas prototype `prototypes/04-orbit-drop.html` — and its Three.js 3D successor `prototypes/05-orbit-drop-3d.html` — into a production-quality Unity 2022.3 LTS **3D** mobile game targeting **Android** (Google Play Store) and **iOS** (Apple App Store). The game mechanics — multi-plane orbital physics, tangent-release trajectory, target collision, combo scoring, progressive levels — are replicated from the 3D prototype in a native Unity 3D (URP) project with a perspective camera the player rotates by dragging. Unity handles 3D rendering, input, physics, audio, and platform builds. All game systems are implemented as clean, modular C# scripts.

### Goal
Ship a native mobile build of Orbit Drop 3D on both Android (APK / AAB) and iOS (Xcode project / IPA) that faithfully reproduces the Three.js prototype's 3D gameplay — balls orbit a central planet on multiple spatial planes, the player rotates a perspective camera by dragging, and releases balls toward floating target rings in 3D space. The build adds mobile-grade polish (safe-area handling, sound effects, DOTween animations, 60 fps performance on mid-range devices) and is structured for future expansion (more orbital planes, leaderboards, IAP).

### Objectives
- Replicate all core gameplay mechanics from `05-orbit-drop-3d.html` inside Unity 3D (URP) with no gameplay regressions
- Implement a perspective camera controlled by one-finger drag (spherical coordinates: theta, phi, distance)
- Implement five orbital planes defined by normal-axis vectors with Gram-Schmidt basis vectors for 3D position and tangent velocity
- Implement clean scene hierarchy: **Boot → MainMenu → Game** with additive UI overlays for Level Complete and Game Over
- Write all game logic as modular, single-responsibility C# MonoBehaviours and ScriptableObjects
- Achieve 60 fps on Android API 21+ (mid-range) and iOS 12+ devices
- Handle all screen aspect ratios (16:9 through 21:9 on Android; iPhone SE through iPhone 15 Pro Max on iOS)
- Implement PlayerPrefs-based best-score persistence
- Deliver signed Android APK/AAB and an Xcode project ready for TestFlight archive

### Deliverables
- Unity 3D (URP) project at `UnityProjects/OrbitDrop3D/`
- Android APK / AAB signed with a debug keystore
- Xcode project at `UnityProjects/OrbitDrop3D/Builds/iOS/`
- All C# scripts, prefabs, scenes, materials, 3D meshes, and shaders inside `Assets/`
- Working 3D orbital-plane mechanics with `CameraOrbitController.cs`, `OrbitController3D.cs`, `OrbiterBall3D.cs`, `TargetRing3D.cs`, `PlanetCore3D.cs`
- This SDD document moved to `backlog/done/` after all test cases pass

---

## Specification

### Complexity Score: HIGH

### Complexity Rationale
New Unity 3D (URP) project from scratch; 18 C# scripts across Core, Gameplay, and UI layers; 3 scenes with full navigation flow; 5 prefabs with compound GameObject hierarchies; Android and iOS platform-specific build configuration; safe-area handling for notch/Dynamic Island devices; DOTween integration; TrailRenderer, ParticleSystem, and LineRenderer usage; PlayerPrefs persistence; physical device testing on two OS families; **plus**: perspective camera spherical-coordinate controller, five 3D orbital planes with Gram-Schmidt basis vector math, screen-space closest-orbiter tap detection via `Camera.WorldToScreenPoint`, SphereCollider trigger on 3D TargetRing, 3D starfield particle system, torus orbit-path meshes aligned via `Quaternion.FromToRotation`, per-target PointLight, URP 3D post-processing (Bloom + Vignette), and decorative planet ring.

---

### Project Folder Structure

```
UnityProjects/OrbitDrop3D/
└── Assets/
    ├── Scenes/
    │   ├── Boot.unity
    │   ├── MainMenu.unity
    │   └── Game.unity
    ├── Scripts/
    │   ├── Core/
    │   │   ├── GameManager.cs
    │   │   ├── LevelManager.cs
    │   │   └── ScoreManager.cs
    │   ├── Gameplay/
    │   │   ├── CameraOrbitController.cs   ← 3D camera drag (spherical coords)
    │   │   ├── OrbitController3D.cs       ← 3D orbital plane + basis vector math
    │   │   ├── OrbiterBall3D.cs           ← MeshRenderer + TrailRenderer + Rigidbody
    │   │   ├── BallLauncher.cs            ← screen-space closest-orbiter selection
    │   │   ├── TargetRing3D.cs            ← SphereCollider trigger, random orientation
    │   │   └── PlanetCore3D.cs            ← planet mesh, decorative ring, rotation
    │   ├── UI/
    │   │   ├── MainMenuUI.cs
    │   │   ├── GameHUD.cs
    │   │   ├── LevelCompleteUI.cs
    │   │   ├── GameOverUI.cs
    │   │   └── SafeAreaHandler.cs
    │   └── Utils/
    │       ├── ParticleManager.cs
    │       └── AudioManager.cs
    ├── Prefabs/
    │   ├── OrbiterBall3D.prefab
    │   ├── TargetRing3D.prefab
    │   ├── HitBurst3D.prefab
    │   └── ScorePopup.prefab
    ├── ScriptableObjects/
    │   └── LevelConfig.asset  (per level data)
    ├── Materials/
    │   ├── PlanetMat.mat          ← URP Lit: color #1a4aaa, emission #091d55
    │   ├── OrbiterTrail.mat       ← additive blending for glowing trail
    │   ├── TargetRingMat.mat      ← gold #ffd700, emissive #ffaa00
    │   ├── OrbitPathMat.mat       ← torus path ring, 20% opacity
    │   └── StarfieldMat.mat       ← unlit billboard points for starfield
    ├── Meshes/
    │   └── (procedural — created at runtime by scripts)
    ├── Shaders/
    │   └── (URP standard — no custom shaders required)
    ├── Fonts/
    │   └── Exo2-Bold SDF.asset  (TMP font asset)
    └── Audio/
        ├── sfx_launch.wav
        ├── sfx_hit.wav
        ├── sfx_miss.wav
        ├── sfx_level_complete.wav
        └── bgm_loop.ogg
```

---

### Implementation Steps (Task Breakdown)

---

#### Phase 1 — Unity Project Setup (Steps 1–9)

**Step 1 — Create Unity Project**
- Open Unity Hub → New Project → template: **2D (URP)** → name: `OrbitDrop`
- Unity version: **2022.3 LTS**
- Save project to `c:\Games\UnityProjects\OrbitDrop\`

**Step 2 — Configure Player Settings**
- Go to **Edit → Project Settings → Player**
- Company Name: `Zehntech Technologies Inc.`
- Product Name: `Orbit Drop`
- Android Bundle Identifier: `com.zehntech.orbitdrop`
- iOS Bundle Identifier: `com.zehntech.orbitdrop`
- Version: `1.0.0`, Bundle Version Code: `1`

**Step 3 — Lock Portrait Orientation**
- **Player Settings → Resolution and Presentation**
- Default Orientation: `Portrait`
- Uncheck all other orientations for both Android and iOS

**Step 4 — Set Target Frame Rate & Quality**
- Create a `Bootstrap.cs` script in `Scripts/Core/` called at app start:
  ```csharp
  Application.targetFrameRate = 60;
  QualitySettings.vSyncCount  = 0;
  Screen.sleepTimeout         = SleepTimeout.NeverSleep;
  ```
- Attach to a persistent GameObject in Boot scene

**Step 5 — Configure Android Build Support**
- **File → Build Settings → Android**
- Minimum API Level: **Android 5.0 (API 21)**
- Target API Level: **API 33**
- Scripting Backend: **IL2CPP**
- Target Architectures: **ARM64** (check only)
- Internet Access: **Not Required**
- Write Permission: **Internal** (PlayerPrefs only)

**Step 6 — Configure iOS Build Support**
- **File → Build Settings → iOS**
- Target minimum iOS Version: **12.0**
- Architecture: **ARM64**
- Camera Usage Description: *(leave blank — camera not used)*
- Requires Persistent WiFi: **No**

**Step 7 — Import TextMeshPro**
- **Window → Package Manager → TextMeshPro**
- Import TMP Essential Resources when prompted
- All UI text will use `TMP_Text` components

**Step 8 — Import DOTween**
- Download DOTween (HOTween v2) from the Asset Store or `dotween.demigiant.com`
- Import into project
- Run **Tools → DOTween Utility Panel → Setup DOTween**
- Enable `DOTween.Init()` call inside Bootstrap.cs

**Step 9 — Create Folder Structure**
- Create all folders listed in the Project Folder Structure section above
- Delete default Unity sample assets (SampleScene, etc.)

---

#### Phase 2 — Sprites & Materials (Steps 10–18)

**Step 10 — Create Background Sprite**
- Create `NebulaGradient.png` (390×780): vertical gradient dark navy `#020208` → `#060820` → `#090412`
- Import as Sprite (2D), Pixels Per Unit: 100
- Create `NebulaBackground.mat` using URP Sprite Unlit shader
- Assign to a full-screen Quad or Sprite Renderer positioned behind all other objects at z = 10

**Step 11 — Create Starfield Particle System**
- In Game scene: add **ParticleSystem** GameObject named `Starfield`
- Shape: Box covering full camera view
- Emission: 0 rate over time, 80 burst at start
- Particle lifetime: infinite (`Max Particles`: 80)
- Sprite: `StarParticle.png` (small white circle)
- Color over lifetime: alpha sine oscillation for twinkle effect
- Start speed: 0 (static stars)
- Layer: Background

**Step 12 — Create Planet Sprite & Material**
- Create `Planet.png` (256×256): radial gradient circle `#6eb5ff` (centre-offset top-left) → `#2563eb` → `#0c1a6b`
- Create `AtmosphereRing.png` (320×320): transparent centre, soft blue ring at outer edge
- `PlanetGlow.mat`: URP Lit 2D shader with bloom-compatible emission

**Step 13 — Create OrbiterBall Sprite**
- Create `OrbiterBall.png` (64×64): white-to-transparent radial circle
- Three tint colours applied at runtime via `SpriteRenderer.color`: `#00D2FF`, `#A29BFE`, `#FD79A8`
- `OrbiterTrail.mat`: additive blending material for TrailRenderer (makes trail glow)

**Step 14 — Create TargetRing Sprites**
- `TargetRingOuter.png` (128×128): hollow circle, thin white stroke
- `TargetRingInner.png` (80×80): smaller hollow circle
- `TargetRingMat.mat`: URP Sprite Unlit, supports runtime color tint (gold `#FFD700`)
- Crosshair lines: rendered via `LineRenderer` component on the prefab

**Step 15 — Import Audio Assets**
- Place placeholder `.wav`/`.ogg` files in `Assets/Audio/`
- Configure AudioClip import: **Load Type** = `Decompress on Load` for SFX, `Streaming` for BGM

**Step 16 — Import & Configure Font**
- Import **Exo 2 Bold** (Google Fonts, free) as TrueType
- In **Window → TextMeshPro → Font Asset Creator**: generate SDF font asset
- Save as `Exo2-Bold SDF.asset` in `Assets/Fonts/`
- Set as default font in **Project Settings → TextMeshPro**

**Step 17 — URP Post-Processing Setup**
- Add **Volume** component to camera with **Bloom** effect
- Bloom Threshold: 0.8, Intensity: 1.2 — makes glow materials bloom naturally
- **Vignette**: Intensity 0.22 for cinematic border

**Step 18 — Layers & Sorting Layers**
- Define Sorting Layers (bottom → top): `Background`, `Nebula`, `Planet`, `Targets`, `Orbiters`, `Shots`, `Particles`, `UI`
- Define Layers for Physics2D: `Ball`, `Target`, `Wall`

---

#### Phase 3 — Scenes (Steps 19–22)

**Step 19 — Boot.unity Scene**
- Contains a single GameObject `Bootstrap` with `Bootstrap.cs`
- `Bootstrap.cs`: sets frame rate, initialises DOTween, loads `PlayerPrefs` best score into `ScoreManager`, then calls `SceneManager.LoadSceneAsync("MainMenu")`
- No visual content — 1–2 frame scene, invisible to user
- Add to Build Settings as index 0

**Step 20 — MainMenu.unity Scene**
- **Camera**: Orthographic, background `#020208`, size set by `CameraSetup.cs`
- **Background**: `NebulaGradient` Sprite Renderer + `Starfield` ParticleSystem
- **Planet**: `PlanetCore` GameObject (planet sprite + atmosphere ring child) — idle DOTween scale pulse
- **Canvas**: Screen Space — Overlay, scaled with screen, SafeAreaHandler padding
  - Title: TMP "Orbit Drop" (large, white)
  - Best Score: TMP text (conditional, gold)
  - LAUNCH button: RectTransform, TMP label, `MainMenuUI.cs` listener
  - 5 instruction rows (icon emoji + TMP text)
- Add to Build Settings as index 1

**Step 21 — Game.unity Scene**
- **Camera** (tagged MainCamera): Orthographic, `CameraSetup.cs` sizes it to show full play area
- **Background**: Starfield + Nebula sprite (same as MainMenu)
- **Planet** (centre): `PlanetCore.cs`
- **Managers** (empty GameObjects):
  - `GameManager` — `GameManager.cs`
  - `LevelManager` — `LevelManager.cs`
  - `ScoreManager` — `ScoreManager.cs`
  - `BallLauncher` — `BallLauncher.cs`
  - `ParticleManager` — `ParticleManager.cs`
  - `AudioManager` — `AudioManager.cs`
- **Canvas (HUD)**: `GameHUD.cs`, SafeAreaHandler
- **Canvas (Overlays)**: Level Complete panel + Game Over panel (hidden by default)
- Add to Build Settings as index 2

**Step 22 — Build Settings Scene Order**
- Index 0: `Scenes/Boot`
- Index 1: `Scenes/MainMenu`
- Index 2: `Scenes/Game`
- Platform: Android (switch to iOS for iOS build)

---

#### Phase 4 — Core Scripts (Steps 23–25)

**Step 23 — GameManager.cs**
```
Location: Assets/Scripts/Core/GameManager.cs
Pattern:  Singleton MonoBehaviour
```
- `public static GameManager Instance`
- `public enum GameState { MainMenu, Playing, LevelComplete, GameOver }`
- `public GameState State { get; private set; }`
- `public int Level { get; private set; }`
- `public int Lives { get; private set; }` — starts at 3
- Methods:
  - `StartGame()` — resets all state, calls `LevelManager.SpawnLevel(1)`, sets State → Playing
  - `NextLevel()` — increments Level, calls `LevelManager.SpawnLevel(Level)`, State → Playing
  - `LoseLife()` — decrements Lives; if 0 → `EndGame()`; else re-spawn orbiters after 0.7 s
  - `EndGame()` — State → GameOver, saves best score
  - `OnLevelComplete()` — State → LevelComplete, adds bonus, shows banner, invokes `NextLevel()` after 2 s
- Events: `OnStateChanged`, `OnLivesChanged`, `OnLevelChanged`

**Step 24 — LevelManager.cs**
```
Location: Assets/Scripts/Core/LevelManager.cs
```
- `[System.Serializable] struct LevelConfig { int targetCount; int orbiterCount; float[] orbiterSpeeds; float[] orbiterRadii; }`
- `LevelConfig GetConfig(int level)`:
  - `targetCount = Mathf.Min(2 + level, 7)`
  - `orbiterCount = Mathf.Min(1 + level / 2, 3)`
  - Speeds from array `{0.018f, 0.024f, 0.030f, 0.038f, 0.046f, 0.055f}` indexed by `level-1`
- `void SpawnLevel(int level)`:
  - Destroy all existing orbiters and targets
  - Call `SpawnOrbiters(config)` and `SpawnTargets(config)`
- `void SpawnOrbiters(LevelConfig config)`:
  - Instantiate `OrbiterBall.prefab` for each orbiter
  - Set angle `= (2π / count) × i`, alternating speed sign
  - Assign colour from `Color[] ballColors`
- `void SpawnTargets(LevelConfig config)`:
  - For each target: random `Vector2` within camera bounds with planet clearance + overlap checks (max 200 attempts per slot)
  - Instantiate `TargetRing.prefab`, assign position

**Step 25 — ScoreManager.cs**
```
Location: Assets/Scripts/Core/ScoreManager.cs
Pattern:  Singleton MonoBehaviour
```
- `public int Score { get; private set; }`
- `public int BestScore { get; private set; }` — loaded from `PlayerPrefs.GetInt("od_best", 0)` on Awake
- `public int ComboCount { get; private set; }`
- `public int Lives { get; private set; }`
- `void AddHitScore()` — `ComboCount++; Score += 100 * ComboCount;` fire `OnScoreChanged`
- `void AddLevelBonus(int level)` — `Score += level * 500;`
- `void ResetCombo()` — `ComboCount = 0`
- `void SaveBestScore()` — if `Score > BestScore`: `BestScore = Score; PlayerPrefs.SetInt("od_best", BestScore); PlayerPrefs.Save()`
- `void ResetForNewGame()` — `Score = 0; ComboCount = 0`
- Events: `OnScoreChanged(int)`, `OnComboChanged(int)`

---

#### Phase 5 — Gameplay Scripts (Steps 26–35)

**Step 26 — OrbitController.cs**
```
Location: Assets/Scripts/Gameplay/OrbitController.cs
Attach to: OrbiterBall.prefab
```
- `public float Angle { get; private set; }` — radians
- `public float Radius;` — orbit radius in world units
- `public float Speed;` — radians per second (negative = clockwise)
- `public bool IsShot { get; private set; }`
- `void Update()` — if not shot: `Angle += Speed * Time.deltaTime;` update `transform.position = planet.position + new Vector3(cos(Angle) × Radius, sin(Angle) × Radius, 0)`
- `Vector2 GetTangentVelocity()`:
  - Direction: `(-Mathf.Sin(Angle) * Mathf.Sign(Speed), Mathf.Cos(Angle) * Mathf.Sign(Speed))`
  - Speed magnitude: `Radius * Mathf.Abs(Speed) * 2.8f * (1 / Time.fixedDeltaTime)`  *(tuned to match HTML prototype feel)*
  - Returns `direction * magnitude`
- `void MarkShot()` — `IsShot = true`

**Step 27 — OrbiterBall.cs**
```
Location: Assets/Scripts/Gameplay/OrbiterBall.cs
Attach to: OrbiterBall.prefab (same GameObject as OrbitController)
```
- References: `OrbitController orbit`, `SpriteRenderer sr`, `TrailRenderer trail`, `LineRenderer arrow`, `Rigidbody2D rb`
- `public Color BallColor` — set by LevelManager on spawn; applies to `sr.color`, `trail.startColor`, `arrow.startColor`
- `void Update()`: if not shot — call `UpdateDirectionArrow()`
- `void UpdateDirectionArrow()`:
  - Compute tangent unit vector from `orbit`
  - Set `arrow.SetPosition(0, transform.position + tangent * 0.4f)`
  - Set `arrow.SetPosition(1, transform.position + tangent * 1.0f)`
  - Arrowhead: second LineRenderer or custom 3-point triangle
- `void Release()`:
  - `orbit.MarkShot()`
  - `trail.emitting = true`
  - `rb.gravityScale = 0`
  - `rb.velocity = orbit.GetTangentVelocity()`
  - Arrow LineRenderer disabled
  - `GetComponent<Collider2D>().enabled = true` (collision active after release)
- `void Update()` shot branch: destroy self when off camera bounds

**Step 28 — BallLauncher.cs**
```
Location: Assets/Scripts/Gameplay/BallLauncher.cs
Attach to: BallLauncher manager GameObject in Game scene
```
- `List<OrbiterBall> activeBalls` — populated by LevelManager on spawn
- `void Update()`:
  - Guard: `GameManager.Instance.State != GameState.Playing` → return
  - Detect tap: `Input.GetMouseButtonDown(0)` (Unity maps touch[0] to mouse on mobile)
  - Convert screen tap to world: `Camera.main.ScreenToWorldPoint(Input.mousePosition)`
  - Find closest un-shot orbiter by distance
  - Call `closest.Release()`
- `void CheckAllShotsExited()`: called each frame when all orbiters are shot and no balls remain in `activeBalls` — if targets remain, calls `GameManager.Instance.LoseLife()`

**Step 29 — TargetRing.cs**
```
Location: Assets/Scripts/Gameplay/TargetRing.cs
Attach to: TargetRing.prefab
```
- `public bool IsHit { get; private set; }`
- References: `SpriteRenderer outerRing`, `SpriteRenderer innerRing`, `LineRenderer crosshairH`, `LineRenderer crosshairV`, `CircleCollider2D col` (trigger, active only after game starts)
- `void Start()`: begin DOTween pulse sequence:
  ```csharp
  transform.DOScale(1.08f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
  ```
- `void OnTriggerEnter2D(Collider2D other)`:
  - Guard: `IsHit` → return; check tag `"Ball"`
  - `IsHit = true`
  - Kill DOTween, hide all renderers
  - Notify: `ParticleManager.Instance.SpawnHitBurst(transform.position, ballColor)`
  - Notify: `ScoreManager.Instance.AddHitScore()`; spawn score popup
  - Notify: `LevelManager.Instance.OnTargetHit(this)`
- `void SetHit()` — public method, same as above for manual trigger (used in tests)

**Step 30 — PlanetCore.cs**
```
Location: Assets/Scripts/Gameplay/PlanetCore.cs
Attach to: Planet GameObject in Game and MainMenu scenes
```
- `void Start()`:
  - Idle pulse: `transform.DOScale(1.04f, 1.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)`
  - Atmosphere ring slow rotation: `atmosphereRing.transform.DORotate(new Vector3(0,0,360), 8f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear)`
- `public Transform Position` — used by OrbitController to calculate ball positions

**Step 31 — ParticleManager.cs**
```
Location: Assets/Scripts/Utils/ParticleManager.cs
Pattern:  Singleton with object pool
```
- `[SerializeField] GameObject hitBurstPrefab` — ParticleSystem with 18 burst particles
- `[SerializeField] GameObject scorePopupPrefab` — TMP_Text + DOTween upward float
- `Queue<GameObject> burstPool` — pre-warmed with 6 instances at Start
- `void SpawnHitBurst(Vector2 pos, Color color)`:
  - Dequeue or Instantiate burst, position it, set particle colour, Play(), auto-return after duration
- `void SpawnScorePopup(Vector2 pos, int points)`:
  - Dequeue or Instantiate popup, set TMP text to `"+" + points`, position above hit
  - DOTween: move up 1.5 units over 0.9 s + alpha fade out, then return to pool

**Step 32 — HitBurst.prefab**
- Root: GameObject with `ParticleSystem`
- Particle count: 18 burst
- Start speed: `Random(2, 6)` world units
- Start lifetime: `Random(0.4, 0.8)` s
- Gravity modifier: 0.3 (slight fall for feel)
- Start size: `Random(0.05, 0.18)`
- Color over lifetime: solid → transparent
- `Stop Action`: Disable (for pool re-use)

**Step 33 — ScorePopup.prefab**
- Root: GameObject with `TMP_Text` (gold `#F9CA24`, bold, size 1.2)
- `Canvas` component in World Space mode
- Sorting Layer: `Particles`
- Auto-destroyed / returned to pool by `ParticleManager` after animation completes

**Step 34 — AudioManager.cs**
```
Location: Assets/Scripts/Utils/AudioManager.cs
Pattern:  Singleton
```
- `AudioSource sfxSource, bgmSource`
- `AudioClip[] sfxClips` — mapped to enum `SFX { Launch, Hit, Miss, LevelComplete }`
- `void PlaySFX(SFX type)` — plays one-shot on `sfxSource`
- `void PlayBGM()` — plays looping BGM on `bgmSource`
- Volume levels saved / loaded from `PlayerPrefs`

**Step 35 — CameraSetup.cs**
```
Location: Assets/Scripts/Core/CameraSetup.cs  (attach to Main Camera)
```
- On Awake: compute orthographic size so that the 9:16 reference area is fully visible
  ```csharp
  float aspect      = (float)Screen.width / Screen.height;
  float targetAspect = 9f / 16f;
  if (aspect >= targetAspect)
      cam.orthographicSize = referenceHeight / 2f;
  else
      cam.orthographicSize = referenceHeight / 2f * (targetAspect / aspect);
  ```
- Ensures wide phones (21:9) show more horizontal space, narrow phones see full vertical

---

#### Phase 6 — UI Scripts (Steps 36–41)

**Step 36 — GameHUD.cs**
```
Location: Assets/Scripts/UI/GameHUD.cs
Attach to: HUD Canvas root in Game scene
```
- References (all TMP_Text): `scoreText`, `levelText`, `bestText`, `targetCountText`, `comboText`
- References (Image[]): `lifeIcons` — 3 heart sprites (full/empty toggled)
- Subscribes to `ScoreManager.OnScoreChanged`, `GameManager.OnLivesChanged`, `GameManager.OnLevelChanged`
- `void UpdateScore(int s)` — `scoreText.text = s.ToString(); DOTween punch scale on scoreText`
- `void UpdateLives(int l)` — toggle heart image alpha
- `void UpdateTargetCount(int n)` — `targetCountText.text = "TARGETS: " + n`
- `void ShowCombo(int c)` — if `c > 1`: show comboText `"×" + c + " COMBO!"` in pink, else hide
- `void UpdateLevel(int l)` — `levelText.text = "LEVEL " + l`

**Step 37 — LevelCompleteUI.cs**
```
Location: Assets/Scripts/UI/LevelCompleteUI.cs
Attach to: LevelComplete panel (hidden by default, alpha 0, scale 0)
```
- References: `TMP_Text levelText`, `TMP_Text bonusText`, `CanvasGroup group`
- `void Show(int level, int bonus)`:
  - Set texts
  - DOTween Sequence: `group.DOFade(1, 0.25f)` + `transform.DOScale(1, 0.25f).SetEase(Ease.OutBack)`
  - Auto-hide after 2 s: `group.DOFade(0, 0.3f).SetDelay(1.7f)`
- `void Hide()` — immediate alpha 0, scale 0

**Step 38 — GameOverUI.cs**
```
Location: Assets/Scripts/UI/GameOverUI.cs
Attach to: GameOver panel (hidden by default)
```
- References: `TMP_Text scoreText`, `TMP_Text bestText`, `Button retryBtn`, `Button menuBtn`, `CanvasGroup group`
- `void Show(int score, int best)`:
  - `scoreText.text = score.ToString()`
  - `bestText.text = score >= best && score > 0 ? "🏆 NEW BEST!" : "🏆 BEST: " + best`
  - DOTween fade in: `group.DOFade(1, 0.35f).SetEase(Ease.OutQuad)`
- `retryBtn.onClick` → `GameManager.Instance.StartGame()` + `Hide()`
- `menuBtn.onClick` → `SceneManager.LoadScene("MainMenu")`

**Step 39 — MainMenuUI.cs**
```
Location: Assets/Scripts/UI/MainMenuUI.cs
```
- On Start: `bestScoreText.text = "🏆 Best: " + ScoreManager.Instance.BestScore`
- Hide best score text if `BestScore == 0`
- `playButton.onClick` → `SceneManager.LoadScene("Game")`
- Planet idle animation via `PlanetCore.cs` (shared script)

**Step 40 — SafeAreaHandler.cs**
```
Location: Assets/Scripts/UI/SafeAreaHandler.cs
Attach to: Every Canvas root that needs safe-area padding
```
- On Awake and on `Screen.safeArea` change:
  ```csharp
  Rect safeArea     = Screen.safeArea;
  Vector2 anchorMin = safeArea.position / new Vector2(Screen.width, Screen.height);
  Vector2 anchorMax = (safeArea.position + safeArea.size) / new Vector2(Screen.width, Screen.height);
  rectTransform.anchorMin = anchorMin;
  rectTransform.anchorMax = anchorMax;
  ```
- Handles iPhone notch, Dynamic Island, Android punch-hole cameras

**Step 41 — Bootstrap.cs**
```
Location: Assets/Scripts/Core/Bootstrap.cs
Attach to: Bootstrap GameObject in Boot scene
```
- `void Awake()`:
  ```csharp
  Application.targetFrameRate = 60;
  QualitySettings.vSyncCount  = 0;
  Screen.sleepTimeout         = SleepTimeout.NeverSleep;
  DOTween.Init(true, true, LogBehaviour.Verbose);
  ```
- `void Start()`: `SceneManager.LoadSceneAsync("MainMenu")`

---

#### Phase 7 — Scene Assembly (Steps 42–46)

**Step 42 — Assemble Game Scene — World Layer**
- Set Camera: Orthographic, background `#020208`, `CameraSetup` attached
- Place `NebulaGradient` Sprite Renderer at position `(0, 0, 10)`, order -10
- Place `Starfield` ParticleSystem at `(0, 0, 9)`
- Place `Planet` GameObject at `(0, 0, 0)` — `PlanetCore.cs` attached, atmosphere ring as child
- Verify Sorting Layer assignments match layers defined in Step 18

**Step 43 — Assemble Game Scene — Manager GameObjects**
- Create empty GameObjects: `GameManager`, `LevelManager`, `ScoreManager`, `BallLauncher`, `ParticleManager`, `AudioManager`, `CameraSetup`
- Attach corresponding scripts
- Assign serialised references in Inspector (prefabs, AudioClips, Camera)
- Mark `GameManager`, `ScoreManager`, `ParticleManager`, `AudioManager` as `DontDestroyOnLoad` singletons

**Step 44 — Assemble Game Scene — HUD Canvas**
- Canvas: Screen Space — Overlay, Canvas Scaler: Scale With Screen Size, Reference 1080×1920, Match: 0.5
- Attach `SafeAreaHandler.cs` to root RectTransform
- Place TMP texts and Image arrays per `GameHUD.cs` field layout
- Level Complete panel: anchored centre, initial scale `(0,0,1)`, CanvasGroup alpha 0
- Game Over panel: full-screen dim layer + card, CanvasGroup alpha 0

**Step 45 — Assemble MainMenu Scene**
- Same background and planet setup as Game Scene
- Canvas with `MainMenuUI.cs`: title, best score, LAUNCH button, instruction rows
- Safe area handled by `SafeAreaHandler.cs`

**Step 46 — Wire All Inspector References**
- Verify every `[SerializeField]` field is assigned in the Inspector
- No missing references (check Console before build)
- Prefabs: `OrbiterBall.prefab`, `TargetRing.prefab`, `HitBurst.prefab`, `ScorePopup.prefab` all assigned in `LevelManager` and `ParticleManager`

---

#### Phase 8 — OrbiterBall & TargetRing Prefabs (Steps 47–50)

**Step 47 — OrbiterBall.prefab Hierarchy**
```
OrbiterBall (root)
├── SpriteRenderer     — OrbiterBall.png sprite, Sorting Layer: Orbiters
├── TrailRenderer      — OrbiterTrail.mat (additive), time: 0.25 s
├── LineRenderer       — direction arrow (2 positions), width 0.04 world units
├── Rigidbody2D        — Body Type: Kinematic initially; switched to Dynamic on release; Gravity Scale: 0
├── CircleCollider2D   — Trigger: true; enabled only after release
└── Scripts:           OrbitController.cs + OrbiterBall.cs
```

**Step 48 — TargetRing.prefab Hierarchy**
```
TargetRing (root)
├── OuterRing          — SpriteRenderer, TargetRingOuter.png, gold tint, Sorting Layer: Targets
├── InnerRing          — SpriteRenderer, TargetRingInner.png, gold tint
├── CrosshairH         — LineRenderer, horizontal, alpha 0.45
├── CrosshairV         — LineRenderer, vertical, alpha 0.45
├── CircleCollider2D   — Trigger: true, radius matches OuterRing
└── Scripts:           TargetRing.cs
```

**Step 49 — Configure Physics2D**
- **Edit → Project Settings → Physics 2D**
- Gravity: `(0, 0)` — no gravity in this game
- Layer Collision Matrix: enable only `Ball` ↔ `Target`; disable all other pairs (prevents performance overhead)

**Step 50 — Ball Tags & Layers**
- Tag `"Ball"` applied to OrbiterBall prefab root
- Tag `"Target"` applied to TargetRing prefab root
- Physics Layer `Ball` on OrbiterBall, Layer `Target` on TargetRing

---

#### Phase 9 — Android Build (Steps 51–54)

**Step 51 — Create Android Keystore**
- **Edit → Project Settings → Player → Publishing Settings**
- Create a new Keystore: `OrbitDrop.keystore`
- Key alias: `orbitdrop`, password stored securely (not committed to repo)
- Store in `c:\Games\UnityProjects\OrbitDrop\` (outside Assets — not version-controlled)

**Step 52 — Android Build Configuration**
- Confirm Scripting Backend: IL2CPP, ARM64 only
- Compression: LZ4HC
- **File → Build Settings → Build** → select `Builds/Android/` folder
- Choose **Build App Bundle (.aab)** for Play Store, **Build APK** for direct install testing

**Step 53 — Android Smoke Test**
- Install APK on a physical Android device (or emulator API 21+):
  ```
  adb install -r OrbitDrop.apk
  ```
- Verify: app launches, portrait orientation locked, 60 fps in Profiler (USB)
- Verify: touch input fires ball release correctly
- Verify: safe area — status bar does not overlap HUD on punch-hole camera phones

**Step 54 — Android Performance Profile**
- Connect Unity Profiler over USB
- Play 3 levels — confirm no frame spikes above 20 ms (GPU or CPU)
- Confirm particle pool avoids GC allocations (no spikes in Memory Profiler)

---

#### Phase 10 — iOS Build (Steps 55–58)

**Step 55 — Switch Platform to iOS**
- **File → Build Settings → iOS → Switch Platform**
- Confirm all settings from Step 6 are present after switch

**Step 56 — Build Xcode Project**
- **File → Build Settings → Build** → select `Builds/iOS/`
- Unity generates a full Xcode project at that path

**Step 57 — Configure Xcode Project**
- Open `OrbitDrop.xcodeproj` in Xcode 15+
- **Signing & Capabilities**: set Development Team and Bundle Identifier `com.zehntech.orbitdrop`
- Deployment Target: iOS 12.0
- Build for a connected physical iPhone (simulator cannot fully test touch + performance)

**Step 58 — iOS Smoke Test**
- Build & Run on a physical iPhone
- Verify: portrait orientation locked, no landscape flicker on rotation
- Verify: safe area handler pads HUD below Dynamic Island / notch
- Verify: 60 fps via Instruments (Time Profiler)
- Verify: `PlayerPrefs` persists between app kills

---

#### Phase 11 — Polish & QA (Steps 59–61)

**Step 59 — Screen Aspect Ratio QA**
- Test on: 16:9 (older Android), 18:9, 19.5:9 (most modern Android), 20:9 (Samsung Ultra), 19.5:9 iPad
- Confirm: all UI elements visible, no text clipped, planet centred

**Step 60 — App Lifecycle Testing**
- Home button press mid-game → resume → ball is where it was, timer paused
- Phone call received mid-game → app pauses correctly, no crash on resume
- Implement `OnApplicationPause(bool)` in `GameManager` to pause `Time.timeScale`

**Step 61 — Final Asset Audit**
- Confirm no placeholder sprites remain
- Confirm all AudioClips are assigned
- Confirm no `Debug.Log` calls remain in Release builds (`#if UNITY_EDITOR` guards)
- Confirm `PlayerPrefs.Save()` is called on `OnApplicationPause` and `OnApplicationQuit`

---

#### Phase 12 — 3D Conversion (Steps 62–78)

This phase upgrades the project from 2D URP to full 3D URP, implementing all mechanics demonstrated in `prototypes/05-orbit-drop-3d.html`.

**Step 62 — Switch to 3D URP Project Template**
- In Unity Hub, create the project using the **3D (URP)** template (not 2D URP)
- Camera: switch Main Camera from Orthographic to **Perspective**, FOV: 55°
- Remove all 2D SpriteRenderer and CircleCollider2D from gameplay objects — replaced by MeshRenderer and SphereCollider in this phase
- Confirm URP 3D render pipeline asset is assigned in **Edit → Project Settings → Graphics**

**Step 63 — Configure Perspective Camera**
```
Location: Main Camera in Game.unity
```
- Field of View: **55°**
- Near Clip: **0.1**, Far Clip: **200**
- Background: Solid Color `#000005` (near-black)
- Initial position: computed by `CameraOrbitController.cs` from spherical coords `(theta=0.55, phi=1.15, dist=11.5)`
- Clear Flags: **Solid Color**

**Step 64 — CameraOrbitController.cs**
```
Location: Assets/Scripts/Gameplay/CameraOrbitController.cs
Attach to: Main Camera in Game scene
```
- Fields:
  ```csharp
  public float Theta = 0.55f;      // horizontal angle (radians)
  public float Phi   = 1.15f;      // vertical angle (radians)
  public float Dist  = 11.5f;      // distance from origin
  public float DragSensitivity = 0.0075f;
  public bool  IsDragging { get; private set; }
  ```
- `void Update()`:
  - On pointer down: record `startPos`, `totalMoved = 0`
  - On pointer drag: `delta = (current - prev) * DragSensitivity`; `Theta += delta.x`; `Phi = Mathf.Clamp(Phi - delta.y, 0.10f, Mathf.PI - 0.10f)`; accumulate `totalMoved`
  - Compute position: `x = Dist * sin(Phi) * cos(Theta)`, `y = Dist * cos(Phi)`, `z = Dist * sin(Phi) * sin(Theta)`
  - `transform.position = new Vector3(x, y, z)` then `transform.LookAt(Vector3.zero)`
- **Tap vs drag**: if `totalMoved < 10f` on pointer up → it is a tap; set `IsDragging = false` and notify `BallLauncher`

**Step 65 — Define 5 Orbital Plane Normals**
```
Location: Assets/Scripts/Gameplay/OrbitController3D.cs (static readonly array)
```
Five orbital plane normal vectors (matching the Three.js prototype exactly):
```csharp
public static readonly Vector3[] PlaneNormals = new Vector3[]
{
    new Vector3(0, 1, 0),                                        // equatorial
    new Vector3(1, 0, 0),                                        // side
    Vector3.Normalize(new Vector3(0.6f,  0.8f, 0f)),             // tilted front
    Vector3.Normalize(new Vector3(-0.6f, 0.8f, 0f)),             // tilted back
    Vector3.Normalize(new Vector3(0.577f, 0.577f, 0.577f)),      // diagonal
};
```
Each orbiter is assigned a plane index by `LevelManager` at spawn.

**Step 66 — OrbitController3D.cs**
```
Location: Assets/Scripts/Gameplay/OrbitController3D.cs
Attach to: OrbiterBall3D.prefab
```
- Gram-Schmidt basis vector construction:
  ```csharp
  void ComputeBasis(Vector3 axis, out Vector3 u, out Vector3 v)
  {
      Vector3 reference = Mathf.Abs(axis.x) < 0.9f ? Vector3.right : Vector3.up;
      u = Vector3.Normalize(reference - axis * Vector3.Dot(reference, axis));
      v = Vector3.Cross(axis, u);
  }
  ```
- `void Update()` (not shot):
  ```csharp
  Angle += Speed * Time.deltaTime;
  transform.position = u * Mathf.Cos(Angle) * Radius + v * Mathf.Sin(Angle) * Radius;
  ```
- `Vector3 GetTangentVelocity()`:
  ```csharp
  float sign = Mathf.Sign(Speed);
  Vector3 tangent = (-Mathf.Sin(Angle) * sign * u + Mathf.Cos(Angle) * sign * v).normalized;
  return tangent * Mathf.Abs(Speed) * Radius * 8.5f;
  ```
  *(Speed scale 8.5 tuned to match the Three.js prototype trajectory feel)*

**Step 67 — OrbiterBall3D.cs**
```
Location: Assets/Scripts/Gameplay/OrbiterBall3D.cs
Attach to: OrbiterBall3D.prefab
```
- References: `OrbitController3D orbit`, `MeshRenderer mr`, `TrailRenderer trail`, `Rigidbody rb`
- `public Color BallColor` — applied via `mr.material.color` (URP Lit material tint)
- Trail: `trail.startColor = BallColor; trail.endColor = Color.clear;` — 18 positions, time 0.25 s
- `void Release()`:
  ```csharp
  orbit.enabled = false;
  rb.isKinematic = false;
  rb.velocity = orbit.GetTangentVelocity();
  trail.emitting = true;
  GetComponent<SphereCollider>().enabled = true;
  ```
- Off-camera culling: `if (Vector3.Distance(transform.position, Vector3.zero) > 25f) Destroy(gameObject)`

**Step 68 — BallLauncher.cs (3D update)**
```
Location: Assets/Scripts/Gameplay/BallLauncher.cs
```
- Closest-orbiter selection uses **screen-space projection** (not world distance):
  ```csharp
  float minDist = float.MaxValue;
  OrbiterBall3D closest = null;
  foreach (var ball in activeBalls)
  {
      if (ball.IsShot) continue;
      Vector3 screen = Camera.main.WorldToScreenPoint(ball.transform.position);
      float d = Vector2.Distance(new Vector2(screen.x, screen.y), tapScreenPos);
      if (d < minDist) { minDist = d; closest = ball; }
  }
  if (closest != null) closest.Release();
  ```
- Only fires when `CameraOrbitController.IsDragging == false` and `totalMoved < 10px`
- `CheckAllShotsExited()` — if all balls shot and none remain in `activeBalls` and targets remain → `GameManager.Instance.LoseLife()`

**Step 69 — TargetRing3D.cs**
```
Location: Assets/Scripts/Gameplay/TargetRing3D.cs
Attach to: TargetRing3D.prefab
```
- `SphereCollider col` (trigger, radius 0.42) — detects 3D ball entry
- Random orientation on spawn: `transform.rotation = Random.rotation`
- Pulse animation: `transform.DOScale(1.08f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)`
- `void OnTriggerEnter(Collider other)` (3D — not `OnTriggerEnter2D`):
  - Guard `IsHit`; check tag `"Ball"`
  - `IsHit = true`; disable MeshRenderer and SphereCollider; disable child PointLight
  - Call `ParticleManager.Instance.SpawnHitBurst3D(transform.position, ballColor)`
  - Call `ScoreManager.Instance.AddHitScore()` and notify `LevelManager.Instance.OnTargetHit(this)`
- Child PointLight: color `#FFCC00`, intensity `0.70`, range `3.2`

**Step 70 — PlanetCore3D.cs**
```
Location: Assets/Scripts/Gameplay/PlanetCore3D.cs
Attach to: Planet GameObject in Game scene
```
- Planet mesh: Unity Sphere primitive with `PlanetMat` (URP Lit, Albedo `#1a4aaa`, Emission `#091d55`)
- `void Update()`: `transform.Rotate(Vector3.up, 2f * Time.deltaTime)` — slow Y-axis spin
- Decorative ring child: torus mesh (created via ProBuilder or procedural script) with these parameters:
  - Major radius: `1.85`, tube radius: `0.048`
  - `transform.rotation = Quaternion.Euler(90f - 72f, 0, 0)` — 18° tilt from equatorial
- Idle pulse: `transform.DOScale(1.04f, 1.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)`

**Step 71 — Orbit Path Torus Rings**
```
Location: Created by LevelManager.SpawnLevel() for each active orbital plane
```
- For each orbital plane:
  - Create a torus mesh ring (major radius = orbiter radius, tube radius = 0.018 world units)
  - Material: `OrbitPathMat.mat` — URP Unlit, white, alpha 0.20
  - Align to plane: `ring.transform.rotation = Quaternion.FromToRotation(Vector3.up, planeNormal)`
- Rings are destroyed and re-created each level (no pooling needed — max 5 per level)

**Step 72 — HitBurst3D.prefab**
```
Location: Assets/Prefabs/HitBurst3D.prefab
```
- ParticleSystem with Shape: **Sphere** (not Cone) — sparks burst in all 3D directions
- Burst count: **24 particles** at time 0
- Start speed: `Random(5, 8)` world units
- Start lifetime: `Random(0.32, 0.72)` s
- Limit Velocity over Lifetime: drag factor `0.92` (simulates the HTML prototype's `vel.multiplyScalar(0.92)`)
- Start size: `Random(0.04, 0.15)`
- Color over lifetime: ball colour → transparent
- `Stop Action`: Disable (pool-friendly)

**Step 73 — Per-Target PointLight**
- Each `TargetRing3D` prefab has a child **Light** component:
  - Type: **Point**, Color: `#FFCC00`, Intensity: `0.70`, Range: `3.2`
  - Shadows: disabled (performance)
  - Disabled alongside MeshRenderer when `IsHit = true`
- Maximum 7 targets per level → maximum 7 active PointLights, within URP Forward+ budget

**Step 74 — 3D Lighting Setup**
In Game scene:
- **Ambient Light**: via **Window → Rendering → Lighting → Environment**
  - Ambient Color: approximately `new Color(0.067f, 0.133f, 0.4f) * 2.2f`
- **Directional Light** (primary sun):
  - Color: `#6699ff`, Intensity: `3.2`
  - Rotation: ~`(−30°, 60°, 0°)` so light comes from upper-right front
- **Point Light** (rim / fill):
  - Color: `#3344cc`, Intensity: `2.0`
  - Position: `(−7, −5, −7)` world units

**Step 75 — 3D Starfield**
- ParticleSystem named `Starfield3D`:
  - Shape: **Sphere**, Radius: `100`, **Emit from Shell** only
  - Burst: 1500 particles at Start, `Start Lifetime`: infinity (never die)
  - `Start Speed`: 0 (static stars — no movement)
  - Renderer: Billboard sprite or Point sprite (`StarParticle.png`)
  - `Max Particles`: 1500
  - Color: white with random alpha 0.5–1.0
  - Stop Emitting after burst (single fire on Start)

**Step 76 — URP 3D Post-Processing**
- **Volume** component (global layer `PostProcessing`):
  - **Bloom**: Threshold `0.8`, Intensity `1.4` — makes emissive planet and target materials glow
  - **Vignette**: Intensity `0.22`, Smoothness `0.5`
- Assign URP renderer with **Post Processing** enabled in `UniversalRenderPipelineAsset`
- Camera: enable **Post Processing** checkbox on the Camera component

**Step 77 — OrbiterBall3D & TargetRing3D Prefab Hierarchies**
```
OrbiterBall3D (root)
├── MeshRenderer + MeshFilter  — Unity Sphere primitive (scale 0.32), URP Lit material tinted at runtime
├── TrailRenderer              — OrbiterTrail.mat (additive), 18 positions, time 0.25 s
├── Rigidbody                  — isKinematic=true initially; Use Gravity: false; Collision Detection: Continuous
├── SphereCollider             — Trigger: true; enabled only after Release(); radius 0.16
└── Scripts:                   OrbitController3D.cs + OrbiterBall3D.cs

TargetRing3D (root)
├── MeshRenderer + MeshFilter  — Torus mesh, TargetRingMat (gold #ffd700, emissive #ffaa00)
├── SphereCollider             — Trigger: true, radius 0.42
├── Light (Point)              — color #FFCC00, intensity 0.70, range 3.2
└── Scripts:                   TargetRing3D.cs
```

**Step 78 — Final 3D Scene Assembly & Smoke Test**
- Place planet at `(0, 0, 0)`; camera starts at spherical `(theta=0.55, phi=1.15, dist=11.5)`
- Run in Editor: verify balls orbit on all five planes, camera drags smoothly, tap fires the closest ball
- Verify URP Bloom glows on emissive targets, trail, and planet
- Verify `lifeLock` guard prevents double life-loss when all balls exit simultaneously
- Unity Profiler: confirm 60 fps in Editor with 3 orbiters + 7 targets + 1500 starfield particles + 7 PointLights
- Build Android APK from 3D project; install and verify one-finger drag (camera) + tap-to-fire on device

---

### Code Changes Required

| File | Action | Description |
|------|--------|-------------|
| `Assets/Scenes/Boot.unity` | create | Bootstrap scene — frame rate init, DOTween init, async load MainMenu |
| `Assets/Scenes/MainMenu.unity` | create | Title screen with planet, best score, LAUNCH button |
| `Assets/Scenes/Game.unity` | create | Full gameplay scene with all manager GameObjects and canvases |
| `Assets/Scripts/Core/Bootstrap.cs` | create | Step 4, 41 — app init, DOTween setup, scene load |
| `Assets/Scripts/Core/GameManager.cs` | create | Step 23 — singleton state machine: Playing/LevelComplete/GameOver |
| `Assets/Scripts/Core/LevelManager.cs` | create | Step 24 — level config, orbiter & target spawning |
| `Assets/Scripts/Core/ScoreManager.cs` | create | Step 25 — score, combo, lives, PlayerPrefs persistence |
| `Assets/Scripts/Core/CameraSetup.cs` | create | Step 35 — orthographic size for all aspect ratios |
| `Assets/Scripts/Gameplay/OrbitController.cs` | create | Step 26 — circular orbit update, tangent velocity |
| `Assets/Scripts/Gameplay/OrbiterBall.cs` | create | Step 27 — trail, direction arrow, Release() on tap |
| `Assets/Scripts/Gameplay/BallLauncher.cs` | create | Step 28 — input handling, closest-orbiter selection |
| `Assets/Scripts/Gameplay/TargetRing.cs` | create | Step 29 — pulse animation, trigger collision, OnHit |
| `Assets/Scripts/Gameplay/PlanetCore.cs` | create | Step 30 — planet idle pulse, atmosphere ring rotation |
| `Assets/Scripts/Utils/ParticleManager.cs` | create | Step 31 — pooled hit burst + score popup spawner |
| `Assets/Scripts/Utils/AudioManager.cs` | create | Step 34 — SFX and BGM playback via enum |
| `Assets/Scripts/UI/GameHUD.cs` | create | Step 36 — score, level, lives, target count, combo HUD |
| `Assets/Scripts/UI/LevelCompleteUI.cs` | create | Step 37 — DOTween show/hide level complete overlay |
| `Assets/Scripts/UI/GameOverUI.cs` | create | Step 38 — score display, retry/menu buttons |
| `Assets/Scripts/UI/MainMenuUI.cs` | create | Step 39 — best score display, LAUNCH button |
| `Assets/Scripts/UI/SafeAreaHandler.cs` | create | Step 40 — notch/Dynamic Island canvas padding |
| `Assets/Prefabs/OrbiterBall.prefab` | create | Step 47 — ball + trail + arrow + collider hierarchy |
| `Assets/Prefabs/TargetRing.prefab` | create | Step 48 — nested ring sprites + crosshairs + collider |
| `Assets/Prefabs/HitBurst.prefab` | create | Step 32 — 18-particle burst system |
| `Assets/Prefabs/ScorePopup.prefab` | create | Step 33 — floating TMP score text |
| `Assets/Sprites/*.png` | create | Steps 10–14 — all game sprites |
| `Assets/Materials/*.mat` | create | Steps 12–14 — planet glow, trail, target, nebula materials |
| `Assets/Fonts/Exo2-Bold SDF.asset` | create | Step 16 — TMP font asset |
| `Assets/Audio/*.wav / *.ogg` | create | Step 15 — launch, hit, miss, level complete, BGM |
| `Assets/Scripts/Gameplay/CameraOrbitController.cs` | create | Step 64 — spherical camera drag (theta/phi/dist), tap vs drag distinction |
| `Assets/Scripts/Gameplay/OrbitController3D.cs` | create | Steps 65–66 — 5 plane normals, Gram-Schmidt basis, 3D position + tangent velocity |
| `Assets/Scripts/Gameplay/OrbiterBall3D.cs` | create | Step 67 — MeshRenderer ball, 3D TrailRenderer, Rigidbody release |
| `Assets/Scripts/Gameplay/TargetRing3D.cs` | create | Step 69 — SphereCollider trigger, random orientation, per-target PointLight |
| `Assets/Scripts/Gameplay/PlanetCore3D.cs` | create | Step 70 — URP Lit planet mesh, decorative torus ring, Y-axis rotation |
| `Assets/Prefabs/OrbiterBall3D.prefab` | create | Step 77 — Sphere mesh + 3D trail + SphereCollider + Rigidbody hierarchy |
| `Assets/Prefabs/TargetRing3D.prefab` | create | Step 77 — Torus mesh + SphereCollider + PointLight hierarchy |
| `Assets/Prefabs/HitBurst3D.prefab` | create | Step 72 — sphere-shape 24-particle 3D burst |
| `Assets/Materials/PlanetMat.mat` | create | Step 70 — URP Lit, Albedo #1a4aaa, Emission #091d55 |
| `Assets/Materials/OrbitPathMat.mat` | create | Step 71 — URP Unlit torus ring, alpha 0.20 |
| `Assets/Materials/StarfieldMat.mat` | create | Step 75 — unlit billboard points for 1500-star ParticleSystem |
| `Builds/Android/OrbitDrop3D.apk` | build | Step 52 — signed Android APK (3D build) |
| `Builds/iOS/` | build | Step 56 — Xcode project (3D build) |

---

### Implementation Notes

**Physics approach**
- Orbiters use `Transform` translation (kinematic) while orbiting — no Rigidbody forces applied. On `Release()`, switch the Rigidbody2D to Dynamic, set `rb.velocity = GetTangentVelocity()`. This exactly matches the HTML prototype's linear tangent trajectory.
- No physics gravity (`Physics2D.gravity = Vector2.zero`). Collision only between Ball and Target layers.

**Tangent speed scaling**
- HTML prototype uses: `speed = radius × |angularSpeed| × 60 × 2.8`. In Unity, with delta-time physics: `speed = radius × |angularSpeed| × 2.8f / Time.fixedDeltaTime` — verified to cross the ~5 world-unit play field in the same subjective time.

**Object pooling**
- HitBurst and ScorePopup use a `Queue<GameObject>` pool (pre-warmed 6 each). Prevents GC spikes during combo hits. Never call `Instantiate/Destroy` for these during gameplay.

**DOTween gotcha**
- Call `DOTween.Kill(transform)` before any new tween on the same transform (e.g., in `TargetRing.SetHit()`), otherwise orphaned tweens fight each other.

**Safe area**
- `Screen.safeArea` returns correct values on iOS 12+ and Android API 28+. Below API 28, the full screen rect is returned — no side effects. Test on a physical iPhone with Dynamic Island or notch.

**Scene loading**
- Use `SceneManager.LoadSceneAsync` in Bootstrap to avoid a single-frame stutter. MainMenu loads in background while the Boot scene's one frame shows black.

**PlayerPrefs persistence**
- Call `PlayerPrefs.Save()` explicitly in `OnApplicationPause(true)` and `OnApplicationQuit()` — on iOS, the app can be killed before `OnApplicationQuit` fires.

**Android back button**
- Handle `Input.GetKeyDown(KeyCode.Escape)` in `GameManager`: during play → pause/confirm-quit dialog; on MainMenu → minimize app (`Application.Quit()`).

**3D orbital plane math**
- Each orbiter is assigned one of five plane normals. Gram-Schmidt orthogonalization builds two perpendicular basis vectors `u` and `v` in that plane: `u = normalize(ref - axis * dot(ref, axis))`, `v = cross(axis, u)`. Position = `u*cos(angle)*radius + v*sin(angle)*radius`. Tangent velocity = `(-sin(angle)*sign(speed)*u + cos(angle)*sign(speed)*v).normalized * |speed| * radius * 8.5f`.

**3D camera — tap vs drag**
- `CameraOrbitController` tracks the total screen-space distance moved between `PointerDown` and `PointerUp`. If total movement > 10 pixels → camera drag event, `BallLauncher` ignores it. If ≤ 10 pixels → tap event, `BallLauncher` fires the closest orbiter. This prevents accidental ball releases while rotating the view.

**Screen-space closest-orbiter selection**
- In 3D, "closest to tap" is measured in screen space, not world space. Each orbiter's world position is projected via `Camera.main.WorldToScreenPoint()` to 2D pixel coordinates, then 2D distance to the tap point is measured. This exactly matches the HTML5 3D prototype's behaviour.

**SphereCollider vs CircleCollider2D**
- In the 3D build, `TargetRing3D` uses a 3D `SphereCollider` (radius 0.42) as trigger and `OrbiterBall3D` uses a `SphereCollider` (radius 0.16). Unity's 3D physics `OnTriggerEnter(Collider other)` callback is used — not the 2D variant.

**Orbit ring alignment**
- Each torus orbit-path ring is aligned via `ring.transform.rotation = Quaternion.FromToRotation(Vector3.up, planeNormal)`. Unity has no built-in Torus primitive; create one via ProBuilder, a procedural mesh script, or import from the Unity Asset Store.

**Life-loss guard (`lifeLock`)**
- When the last orbiter is shot and the ball exits the camera without hitting a target, multiple `CheckAllShotsExited()` callbacks can fire in the same frame. A `lifeLock` boolean is set to `true` on the first `LoseLife()` call and reset to `false` after `SpawnOrbiters()` completes (~0.8 s delay). This prevents multiple lives being lost for a single miss event.

**Speed scale 8.5**
- The Three.js prototype uses `|speed| * radius * 8.5` as the world-space release velocity magnitude. In Unity, pass this directly as `rb.velocity = tangent * Mathf.Abs(Speed) * Radius * 8.5f` (no `fixedDeltaTime` division — `Rigidbody.velocity` is already in world-units/second).

---

## Test Cases

### Unit Tests

| # | Test Name | Input / Condition | Expected Result | Status |
|---|-----------|-------------------|-----------------|--------|
| 1 | `test_orbit_position_angle_zero` | `angle=0`, `radius=3` | `position = planet + (3, 0)` | pending |
| 2 | `test_orbit_position_angle_90` | `angle=π/2`, `radius=3` | `position = planet + (0, 3)` | pending |
| 3 | `test_tangent_velocity_clockwise_angle0` | `angle=0`, `speed=+0.018`, `radius=3` | `vx≈0`, `vy > 0` | pending |
| 4 | `test_tangent_velocity_counter_clockwise_angle0` | `angle=0`, `speed=-0.018`, `radius=3` | `vx≈0`, `vy < 0` | pending |
| 5 | `test_tangent_velocity_magnitude` | `radius=3`, `speed=0.018` | magnitude = `3 × 0.018 × 2.8 / fixedDt` | pending |
| 6 | `test_score_first_hit_100` | `comboCount=0`, AddHitScore() | `score=100`, `comboCount=1` | pending |
| 7 | `test_score_second_hit_200` | `comboCount=1`, AddHitScore() | `score=200`, `comboCount=2` | pending |
| 8 | `test_level_bonus_500` | `AddLevelBonus(1)` | `score += 500` | pending |
| 9 | `test_level_bonus_scales` | `AddLevelBonus(3)` | `score += 1500` | pending |
| 10 | `test_combo_reset_on_new_level` | `ResetCombo()` called | `comboCount = 0` | pending |
| 11 | `test_best_score_saved` | `Score=800 > BestScore=600` → `SaveBestScore()` | `PlayerPrefs.GetInt("od_best") = 800` | pending |
| 12 | `test_best_score_not_overwritten` | `Score=200`, `BestScore=800` → `SaveBestScore()` | `PlayerPrefs.GetInt("od_best")` still `800` | pending |
| 13 | `test_level_config_target_count_1` | `GetConfig(1).targetCount` | `3` | pending |
| 14 | `test_level_config_target_count_5` | `GetConfig(5).targetCount` | `7` | pending |
| 15 | `test_level_config_target_count_10` | `GetConfig(10).targetCount` | `7` (capped) | pending |
| 16 | `test_level_config_orbiter_count_1` | `GetConfig(1).orbiterCount` | `1` | pending |
| 17 | `test_level_config_orbiter_count_4` | `GetConfig(4).orbiterCount` | `3` (capped) | pending |
| 18 | `test_lose_life_decrements` | `Lives=2` → `LoseLife()` | `Lives=1` | pending |
| 19 | `test_lose_life_triggers_game_over` | `Lives=1` → `LoseLife()` | `GameManager.State = GameOver` | pending |
| 20 | `test_game_state_transitions` | `StartGame()` → `OnLevelComplete()` → `EndGame()` | States: Playing → LevelComplete → GameOver | pending |
| 21 | `test_3d_basis_vectors_orthogonal` | `ComputeBasis(new Vector3(0,1,0))` | `dot(u,v) ≈ 0`, `dot(u,axis) ≈ 0`, `dot(v,axis) ≈ 0` | pending |
| 22 | `test_3d_orbit_position_equatorial_angle0` | Equatorial plane, angle=0, radius=3 | `position ≈ (3, 0, 0)` | pending |
| 23 | `test_3d_orbit_position_equatorial_angle90` | Equatorial plane, angle=π/2, radius=3 | `position ≈ (0, 0, 3)` | pending |
| 24 | `test_3d_orbit_position_side_plane_angle0` | Side plane `(1,0,0)`, angle=0, radius=3 | `position.x ≈ 0` (ball in YZ plane only) | pending |
| 25 | `test_3d_tangent_perpendicular_to_radius` | Any plane, any angle | `Mathf.Abs(Vector3.Dot(tangent.normalized, position.normalized)) < 0.001f` | pending |
| 26 | `test_3d_tangent_magnitude` | `radius=3`, `speed=0.018`, scale=8.5 | `tangent.magnitude ≈ 3 * 0.018 * 8.5 = 0.459` | pending |
| 27 | `test_camera_phi_clamp_min` | `Phi` dragged below `0.10` | `Phi` clamped to `0.10f` | pending |
| 28 | `test_camera_phi_clamp_max` | `Phi` dragged above `π - 0.10` | `Phi` clamped to `Mathf.PI - 0.10f` | pending |
| 29 | `test_3d_target_on_sphere_surface` | `SpawnTargets` with sphere placement radius 2.5 | All target positions: `Vector3.Distance(Vector3.zero, pos) ≈ 2.5` | pending |
| 30 | `test_diagonal_plane_basis_orthogonal` | `axis = normalize(0.577, 0.577, 0.577)` | `dot(u,v) ≈ 0`, `dot(u,axis) ≈ 0`, `dot(v,axis) ≈ 0` | pending |

---

### Widget Tests

| # | Test Name | Screen / Component | Expected Behaviour | Status |
|---|-----------|-------------------|-------------------|--------|
| 1 | `test_main_menu_title_visible` | MainMenu scene | TMP text "Orbit Drop" rendered at correct position | pending |
| 2 | `test_main_menu_best_hidden_zero` | MainMenu — BestScore=0 | Best score TMP text `gameObject.activeSelf = false` | pending |
| 3 | `test_main_menu_best_shown_nonzero` | MainMenu — BestScore=500 | Best score TMP text shows "🏆 Best: 500" | pending |
| 4 | `test_launch_button_loads_game_scene` | LAUNCH button tap | `SceneManager.GetActiveScene().name = "Game"` | pending |
| 5 | `test_hud_score_updates` | `GameHUD.UpdateScore(1250)` | `scoreText.text = "1250"` | pending |
| 6 | `test_hud_three_hearts_on_start` | Game starts, lives=3 | All 3 life `Image` components have full alpha | pending |
| 7 | `test_hud_one_heart_after_two_losses` | `LoseLife()` × 2 | 1 full heart, 2 empty hearts | pending |
| 8 | `test_hud_target_count_display` | 4 targets remaining | `targetCountText.text = "TARGETS: 4"` | pending |
| 9 | `test_hud_combo_hidden_at_one` | `comboCount=1` | `comboText.gameObject.activeSelf = false` | pending |
| 10 | `test_hud_combo_visible_at_three` | `comboCount=3` | `comboText.text = "×3 COMBO!"`, active = true | pending |
| 11 | `test_level_complete_panel_shows` | `LevelCompleteUI.Show(2, 1000)` | Panel alpha = 1, scale = 1, levelText shows "LEVEL 2 CLEAR!" | pending |
| 12 | `test_level_complete_panel_auto_hides` | Wait 2 s after `Show()` | Panel alpha fades to 0 | pending |
| 13 | `test_game_over_panel_shows` | `GameOverUI.Show(800, 600)` | Panel fades in, scoreText="800" | pending |
| 14 | `test_game_over_new_best_label` | `Show(900, 800)` (score > best) | `bestText.text = "🏆 NEW BEST!"` | pending |
| 15 | `test_game_over_retry_resets_game` | Tap retry button | `GameManager.State = Playing`, `Score = 0`, `Lives = 3` | pending |
| 16 | `test_game_over_menu_returns_to_mainmenu` | Tap Main Menu button | `SceneManager.GetActiveScene().name = "MainMenu"` | pending |
| 17 | `test_orbiter_ball_colour_matches_config` | LevelManager spawns orbiter with `#00D2FF` | `SpriteRenderer.color == new Color(0, 0.82f, 1)` | pending |
| 18 | `test_target_ring_invisible_when_hit` | `TargetRing.SetHit()` | All child SpriteRenderers disabled | pending |
| 19 | `test_safe_area_applies_on_iphone_notch` | SafeAreaHandler on iPhone with notch | `anchorMin.y > 0` (padded from bottom/top) | pending |
| 20 | `test_planet_pulse_animation_runs` | PlanetCore.Start() | DOTween tween active on planet transform | pending |
| 21 | `test_camera_drag_rotates_view` | Simulate pointer drag right 100px | `camera.transform.position.x` changes; planet remains in view centre | pending |
| 22 | `test_tap_vs_drag_distinction` | Pointer moves 5px then releases | `BallLauncher` fires nearest ball; camera does not rotate | pending |
| 23 | `test_orbit_ring_aligned_to_equatorial_plane` | Equatorial plane torus spawned | `ring.transform.up ≈ Vector3.up` (within 0.001) | pending |
| 24 | `test_3d_target_random_orientation` | `TargetRing3D` spawned | `target.transform.rotation != Quaternion.identity` | pending |
| 25 | `test_decorative_ring_visible` | `PlanetCore3D` in game scene | Decorative ring child MeshRenderer is enabled and within camera frustum | pending |
| 26 | `test_3d_trail_emits_on_release` | `OrbiterBall3D.Release()` called | `TrailRenderer.emitting == true` immediately after Release | pending |

---

### Integration Tests

| # | Test Name | Steps | Expected Result | Status |
|---|-----------|-------|-----------------|--------|
| 1 | `test_full_level_1_clear` | 1. StartGame() 2. Release orbiter 3. Shot hits all 3 targets | LevelComplete state fires, level bonus added, scene transitions to level 2 | pending |
| 2 | `test_miss_all_targets_lose_life` | 1. StartGame() 2. Release orbiter 3. Ball exits camera without hitting target | `lives` decrements, new orbiter spawns after 0.7 s | pending |
| 3 | `test_three_misses_game_over` | Miss all targets 3 times | `GameManager.State = GameOver`, Game Over UI shown | pending |
| 4 | `test_combo_three_targets` | Hit 3 targets consecutively | Score = 100+200+300 = 600; comboCount = 3 | pending |
| 5 | `test_multi_orbiter_level_2` | Level 2: 2 orbiters. Release both. | Each ball travels independently, can each hit different targets | pending |
| 6 | `test_level_3_faster_orbiters` | Reach level 3 | `OrbitController.Speed` > level 1 speed value | pending |
| 7 | `test_best_score_survives_app_kill` | Score 950, EndGame(), OnApplicationQuit() → relaunch | `ScoreManager.BestScore = 950` on MainMenu | pending |
| 8 | `test_app_pause_resume_ball_position` | Release ball, press Home, resume | Ball resumes from same position, `Time.timeScale` restored | pending |
| 9 | `test_android_back_button_on_mainmenu` | On MainMenu, press Android Back | `Application.Quit()` called | pending |
| 10 | `test_android_back_button_during_play` | During play, press Android Back | Pause dialog shown (or confirm-quit) | pending |
| 11 | `test_level_target_count_increases` | Play level 1 (3 targets), clear, enter level 2 | Level 2 has 4 targets spawned | pending |
| 12 | `test_score_popup_appears_on_hit` | Ball hits target | Score popup TMP text spawned above hit position, floats up | pending |
| 13 | `test_hit_burst_particles_on_hit` | Ball hits target | HitBurst ParticleSystem plays at target position | pending |
| 14 | `test_target_collider_disabled_when_hit` | Target hit → `SetHit()` called | `CircleCollider2D.enabled = false` prevents double-registration | pending |
| 15 | `test_canvas_safe_area_android_cutout` | Run on Android phone with punch-hole camera | HUD TMP texts not occluded by camera cutout | pending |
| 16 | `test_full_3d_level_clear` | StartGame(), drag camera, release orbiter toward 3D target, hit | LevelComplete fires, 3D burst plays, level 2 spawns with new orbital planes | pending |
| 17 | `test_camera_drag_no_fire` | Drag pointer 50px (> 10px threshold) | No ball released; camera rotates; all orbiters continue orbiting | pending |
| 18 | `test_multi_plane_orbiters` | Level with 3 orbiters on 3 different planes | All 3 orbit on separate planes with no position intersection at any angle | pending |
| 19 | `test_3d_ball_culled_at_distance` | Ball released, no target hit | Ball `Destroy()`-d when `Vector3.Distance(pos, Vector3.zero) > 25f` | pending |
| 20 | `test_perspective_all_targets_visible` | Spawn 7 targets on sphere shell | Camera rotation exposes targets behind planet; all 7 reachable from some camera angle | pending |
| 21 | `test_3d_spark_burst_directionality` | Ball hits target at `(0, 2.5, 0)` | ParticleSystem sphere-shape emits sparks in all 3D directions, not a flat 2D cone | pending |

---

### Edge Cases

| # | Scenario | Expected Behaviour | Status |
|---|----------|--------------------|--------|
| 1 | Two balls released in rapid succession hit the same target | First ball registers hit; `target.IsHit = true` blocks second registration | pending |
| 2 | All orbiters shot, balls still in flight — level complete from last in-flight ball | `OnTriggerEnter2D` on last target fires level complete; no race condition with life-loss check | pending |
| 3 | Target placement fails 200 attempts (very crowded level) | Slot skipped; level spawns with fewer targets gracefully — no infinite loop | pending |
| 4 | DOTween tween called twice on same target (rapid hits) | `DOTween.Kill(transform)` before new tween prevents conflict | pending |
| 5 | App goes to background (phone call) mid-flight | `OnApplicationPause(true)` → `Time.timeScale = 0`; ball frozen, resumes on return | pending |
| 6 | Device rotated to landscape mid-play | Portrait lock prevents rotation; Screen.orientation stays Portrait | pending |
| 7 | `PlayerPrefs` unavailable / corrupted | `PlayerPrefs.GetInt("od_best", 0)` returns default 0; no crash | pending |
| 8 | Ultra-wide device (21:9, Galaxy S23 Ultra) | `CameraSetup.cs` expands orthographic size — full play field visible, no clipping | pending |
| 9 | Very small screen (iPhone SE 4.7") | Canvas Scaler shrinks UI correctly; all buttons still tappable (44pt minimum) | pending |
| 10 | 120 Hz display (ProMotion iPhone/Android) | `targetFrameRate = 60` caps correctly; physics deterministic regardless of refresh | pending |
| 11 | Low memory device — particle pool exhausted | Pool grows dynamically (`Instantiate` new instance if queue empty); no null crash | pending |
| 12 | `SceneManager.LoadSceneAsync` called while already loading | Guard: `if (isLoading) return;` flag in Bootstrap prevents double-load | pending |
| 13 | Ball trajectory passes through the torus ring hole (misses the SphereCollider) | Ball continues flying; no hit registered; target remains active for another attempt | pending |
| 14 | Two orbiters on opposite planes released simultaneously; both exit camera without hitting | `lifeLock` fires only once; exactly 1 life lost, not 2; new orbiters spawn after 0.8 s | pending |
| 15 | Camera `phi` dragged to near 0 or near π (pole regions) | `Phi` clamped at 0.10 / π−0.10; camera does not flip through either pole | pending |
| 16 | 7 targets requested but sphere surface is crowded (overlap detected after 200 placement attempts) | Slot skipped gracefully; level spawns with fewer targets — no infinite loop or crash | pending |

---

### Device / Platform Coverage

| # | Device / OS | Scenario | Expected Result | Status |
|---|------------|----------|-----------------|--------|
| 1 | Samsung Galaxy A54 — Android 13 (API 33) | Full game flow, 3 levels | 60 fps, touch input correct, safe area correct | pending |
| 2 | Google Pixel 6 — Android 12 (API 31) | Full game flow | 60 fps, no ANR, PlayerPrefs persists | pending |
| 3 | Budget Android — API 21 (min target) | Boot → MainMenu → Play | Launches without crash; may run at 45+ fps | pending |
| 4 | Samsung Galaxy Tab S8 — Android 12 (Tablet 4:3) | Portrait play | Canvas fills portrait view; gameplay centred | pending |
| 5 | iPhone 15 Pro Max — iOS 17 | Full game flow | 60 fps, Dynamic Island safe area correct | pending |
| 6 | iPhone SE (3rd gen) — iOS 16 (small 4.7" screen) | Full game flow | All UI elements visible, buttons tappable | pending |
| 7 | iPhone 12 — iOS 15 (notch) | Full game flow | Notch safe area padded; score not behind notch | pending |
| 8 | iPad Air (5th gen) — iPadOS 16 | Portrait play | Canvas fills portrait, gameplay playable | pending |
| 9 | iOS Simulator — iPhone 15 | Basic smoke test (no touch) | Scene loads, planet visible, target spawns | pending |
| 10 | Android Emulator — Pixel 6 API 33 | Basic smoke test | Game launches, Start screen renders | pending |
| 11 | Samsung Galaxy S23 Ultra — Android 13 (120 Hz) | Camera drag + tap-to-fire during 3D gameplay | Drag is smooth; `targetFrameRate=60` cap enforced; no jitter at 120 Hz | pending |
| 12 | iPhone 14 Pro — iOS 16 (ProMotion 120 Hz) | Full 3D gameplay + camera rotation | 60 fps cap holds; Bloom post-processing renders correctly on ProMotion display | pending |

---

### Test Plan for QA Team
> Copy this section verbatim into the PR description.

**Scope**: Full Unity mobile game on Android and iOS — all game screens, gameplay flow, touch input, level progression, scoring, best-score persistence, safe area, and 60 fps performance.

**Pre-conditions**:
- Android APK installed on a physical Android device (API 21+): `adb install -r OrbitDrop.apk`
- iOS IPA installed via Xcode on a physical iPhone (iOS 12+)
- Device storage has at least 50 MB free
- Device not in Low Power Mode (may cap frame rate on iOS)

**QA Steps**:
1. Launch the app. Verify the Boot scene loads instantly (< 1 s) and transitions to the Main Menu.
2. On the Main Menu, verify: "Orbit Drop" title visible, LAUNCH button visible, best score hidden if no prior session.
3. Tap LAUNCH. Verify the Game scene loads: planet at centre, one glowing ball orbiting it with a direction arrow.
4. Verify the starfield is visible and stars twinkle slowly.
5. Tap the screen. Verify the ball detaches and flies in the direction the arrow was pointing.
6. Allow the ball to exit the screen. Verify one heart is removed from the HUD.
7. Repeat until 0 lives. Verify the Game Over screen fades in with the correct score.
8. On the Game Over screen, tap **Try Again**. Verify: score resets to 0, lives restore to 3, level resets to 1.
9. Hit a target. Verify: burst particle effect plays, floating `+100` popup appears, target ring disappears.
10. Hit a second target immediately. Verify `×2 COMBO!` appears in the HUD and `+200` popup shows.
11. Clear all targets in a level. Verify "LEVEL N CLEAR!" banner appears with bonus score, then level advances with new targets.
12. On the Game Over screen, tap **Main Menu**. Verify the Main Menu scene loads.
13. Check that the best score from the previous play session now appears on the Main Menu.
14. Kill the app completely, relaunch. Verify best score is still displayed.
15. On Android: press the system Back button during play. Verify the game pauses or shows a quit confirmation.
16. Press the Home button mid-game. Resume. Verify the ball was frozen and resumes correctly.
17. Hold the device in landscape. Verify the screen stays in portrait (no rotation).
18. On an iPhone with notch or Dynamic Island: verify the top score bar is not hidden behind the notch.
19. On Android with punch-hole camera: verify the lives counter / score is not occluded.
20. Open Unity Profiler (USB). Play 3 levels. Verify no frame time spike above 20 ms.

**Expected Outcomes**:
- App launches cleanly on both Android and iOS
- Touch tap correctly releases the orbiting ball in the tangent direction
- Combo counter increments on consecutive hits
- Lives decrement on misses; Game Over triggers at 0 lives
- Level advances after all targets cleared; target count increases each level
- Best score persists across app kills and relaunches
- Safe area correctly pads HUD on notch/Dynamic Island/punch-hole devices
- 60 fps maintained on mid-range hardware

**Out of Scope**:
- Google Play Store / App Store submission (build signing only)
- Push notifications
- In-app purchases
- Cloud save / leaderboard
- Accessibility (VoiceOver / TalkBack)
- Tablet landscape mode

---

## Done

### Test Results
*(To be filled after all test cases pass)*
- Unit tests: — / 30 passed
- Widget tests: — / 26 passed
- Integration tests: — / 21 passed
- Edge cases: — / 16 passed
- Device / Platform: — / 12 passed
- **Total: pending**

### Summary
*(To be filled on completion)*

### Commit / PR
*(To be filled on completion)*
