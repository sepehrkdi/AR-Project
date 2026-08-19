# Ground-Anchored AR Presenter for Wall-Mounted Paintings

Unity 2022 LTS · Vuforia 10.x · Android (ARCore)

Recognize paintings on a wall (Vuforia **Image Targets**) and spawn a 3-D “presenter” on the **real** floor directly beneath each canvas. The presenter faces the user, shows a speech-bubble description, and despawns the moment tracking is lost. Zero taps.

---

## TL;DR

* **What**: AR guide for galleries/museums; each painting gets a grounded, talking character.
* **How**: X/Z from the painting pose; **Y from Vuforia GroundPlaneStage**. No physics raycasts.
* **Why**: Deterministic floor alignment, no floating/sinking, no per-frame updates.

---

## Demo

* **Video**: `https://github.com/sepehrkdi/AR-Project/blob/master/DEMO.mp4`
* **Repo**: `https://github.com/sepehrkdi/AR-Project`
* **Demo**: `https://github.com/sepehrkdi/AR-Project/blob/master/demo-thumb.png`

---

## Features

* Ground-locked character placement beneath wall-mounted paintings (no raycasts).
* Presenter faces the visitor; auto-spawn/despawn on tracking events.
* Speech-bubble text (TextMeshPro) with auto-sizing background.
* Event-driven; no `Update()` polling; mobile-ready performance.

---

## Requirements

* **Unity**: 2022 LTS
* **Vuforia Engine**: 10.x (`.unitypackage`)
* **Device**: Android with **ARCore** (iOS with **ARKit** also works; minor config changes)
* **Packages**: TextMeshPro (built-in)

---

## Architecture (1-minute view)

| Layer    | Component                                   | Responsibility                         |
| -------- | ------------------------------------------- | -------------------------------------- |
| Tracking | Vuforia **Image Target** (per painting)     | 6-DOF pose of painting                 |
| Tracking | **PlaneFinder + GroundPlaneStage**          | Real-world floor anchor (stable Y)     |
| Logic    | `AutoCharacterPlacer.cs`                    | Listen to target status; spawn/destroy |
| Content  | **Character prefab** + `CharacterSpeech.cs` | Model + bubble UI + optional animation |
| Debug    | Android **Logcat**, optional TMP overlay    | On-device diagnostics                  |

**Core idea**: `spawnPos = (x_painting, y_stage, z_painting) + offset`.
Parent the presenter under **GroundPlaneStage** for anchor stability.

---

## Setup

### 1) Import & License

1. Download Vuforia `.unitypackage`.
2. Unity → **Assets > Import Package > Custom Package…** → import.
3. **Edit > Project Settings > Vuforia** → paste your license key.

### 2) Scene Bootstrap

* Delete **Main Camera**.
* Add **Vuforia > ARCamera**; assign license key.
* Add **Vuforia > Ground Plane > Plane Finder**.
* Add **Vuforia > Ground Plane > Ground Plane Stage**.
* For each painting, add **Vuforia > Image Target**, pick image from your Vuforia database, set real width (e.g., `0.30` for 30 cm).

### 3) Character Prefab with Speech Bubble

* In your character prefab:

  * Add **Canvas (World Space)** at head height; scale e.g. `0.001`.
  * Add **Image** `BubbleBG` (rounded rect).
  * Add **TextMeshPro Text** `BubbleText` (child of BG).
  * On `BubbleBG`, add **Vertical Layout Group** and **Content Size Fitter (Preferred/Preferred)** so the background auto-fits text.
* `CharacterSpeech.cs`:

  ```csharp
  using TMPro;
  using UnityEngine;

  public class CharacterSpeech : MonoBehaviour
  {
      [SerializeField] private TextMeshProUGUI textUI;
      public void SetDescription(string msg) => textUI.text = msg;
  }
  ```

---

## Core Script

Attach `AutoCharacterPlacer.cs` to each **Image Target**.

```csharp
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class AutoCharacterPlacer : MonoBehaviour
{
    [Header("Presenter")]
    public GameObject characterPrefab;  // assign in Inspector

    private ObserverBehaviour observer;
    private GameObject spawnedCharacter;

    void Awake() => observer = GetComponent<ObserverBehaviour>();

    void OnEnable()  => observer.OnTargetStatusChanged += OnStatusChanged;
    void OnDisable() => observer.OnTargetStatusChanged -= OnStatusChanged;

    private void OnStatusChanged(ObserverBehaviour b, TargetStatus s)
    {
        bool tracked = s.Status == Status.TRACKED || s.Status == Status.EXTENDED_TRACKED;

        if (tracked && spawnedCharacter == null) Spawn();
        else if (!tracked && spawnedCharacter != null) Despawn();
    }

    private void Spawn()
    {
        var stage = GameObject.Find("Ground Plane Stage");
        if (!stage) { Debug.LogWarning("Ground Plane Stage not found"); return; }

        Vector3 img = transform.position;                 // painting pose
        float  y    = stage.transform.position.y;         // authoritative floor Y
        Vector3 pos = new Vector3(img.x + 0.25f, y, img.z); // small X offset from wall

        // Face user on horizontal plane
        Vector3 toCam = Camera.main.transform.position - pos; toCam.y = 0f;
        Quaternion rot = toCam.sqrMagnitude > 1e-4 ? Quaternion.LookRotation(toCam) : Quaternion.identity;

        spawnedCharacter = Instantiate(characterPrefab, pos, rot, stage.transform);

        if (spawnedCharacter.TryGetComponent(out CharacterSpeech speech))
            speech.SetDescription(GetPaintingDescription(gameObject.name));
    }

    private void Despawn()
    {
        Destroy(spawnedCharacter);
        spawnedCharacter = null;
    }

    private string GetPaintingDescription(string imageTargetName)
    {
        switch (imageTargetName)
        {
            case "bhj":       return "Mona Lisa - Leonardo da Vinci.";
            case "vessel":    return "Starry Night - Vincent van Gogh.";
            case "TheScream": return "The Scream - Edvard Munch.";
            default:          return "Artwork description unavailable.";
        }
    }
}
```

Why this works:

* No physics; no raycasts; no layer juggling.
* Presenter is a child of **GroundPlaneStage** → stays glued to real floor poses.
* Event-driven via `OnTargetStatusChanged` → zero `Update()` cost.

---

## Building

### Android

1. **File > Build Settings** → Android → *Switch Platform*.
2. **Player Settings**:

   * Identification → Package Name.
   * Other Settings → Scripting Backend: IL2CPP, Target Architectures: ARM64.
3. Ensure device supports **ARCore**; deploy via **Build & Run**.

### iOS (optional)

* Switch to iOS, ensure **ARKit** support in Vuforia, build Xcode project.

---

## Project Structure

```
Assets/
  Prefabs/
    Character.prefab
  Scripts/
    AutoCharacterPlacer.cs
    CharacterSpeech.cs
  Resources/
    (optional assets)
  Vuforia/
    (ImageTarget DB metadata)
Docs/
  demo-thumb.png
README.md
```

> The heavy Vuforia binary folders are intentionally **not** in source control; see re-import below.

---

## Re-import Instructions (for collaborators)

1. Download Vuforia `.unitypackage` (same version listed below).
2. Import via **Assets > Import Package > Custom Package…**.
3. Paste team license key into **Project Settings > Vuforia**.
4. If Unity can’t find images for Image Targets, re-import your Vuforia **Database** package.

---

## Troubleshooting

**Character spawns on the painting, not the floor**
You’re probably using an older raycast variant or missing GroundPlaneStage lookup. Use the script above; ensure `Ground Plane Stage` exists in the scene.

**Nothing spawns**

* Check `OnTargetStatusChanged` events (ObserverBehaviour attached?).
* Confirm `Image Target` names match the switch-case keys or adjust the mapping.
* Ensure **ARCamera** has the license key and device supports ARCore.

**Wrong facing direction**
Prefab’s local forward should be +Z. If not, add a parent transform and rotate child to face +Z.

**Jitter on floor**
Keep the presenter parented to GroundPlaneStage; avoid mixing physics or additional constraints.

**On-device debugging**
Use **Unity Android Logcat** (Package Manager → Install → Window > Analysis > Android Logcat). Add `Debug.Log` lines in `Spawn()`/`Despawn()`.

---

## Performance Notes

* No per-frame logic; only event callbacks → low CPU.
* Parenting to Stage avoids extra transforms and re-position math.
* For many targets: pool presenters to avoid GC spikes (`Instantiate`/`Destroy` bursts).

---

## Roadmap

* Presenter pooling and lightweight animation state machine.
* External description store (JSON/ScriptableObjects) with localization.
* Auto-scale presenter by camera distance.
* AR Foundation fallback for devices without Vuforia.

---

## Versions

* **Unity**: 2022 LTS
* **Vuforia**: 10.x
* **Target**: Android (ARCore) / iOS (ARKit)

---

## License

MIT

---

## Citation

If you reference this in reports/papers:

```
sepehrkdi, “Ground-Anchored AR Presenter for Wall-Mounted Paintings,”
GitHub, 2025. https://github.com/sepehrkdi/AR-Project
```

---

### Appendix: Optional (Raycast filter fallback)

If you must use physics (not recommended), filter out the painting’s collider:

```csharp
Vector3 origin = transform.position + Vector3.up * 2f;
var hits = Physics.RaycastAll(origin, Vector3.down, 10f);
foreach (var h in hits)
{
    if (h.collider.transform.IsChildOf(transform)) continue;
    // use first non-self hit
    var floor = h.point; break;
}
```

Prefer the **GroundPlaneStage Y** approach used in this repo-it’s simpler and more robust.
