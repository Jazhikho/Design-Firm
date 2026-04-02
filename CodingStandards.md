# Coding standards — Fashion Styling Game

This document defines conventions for code in this repository. It applies to Unity C# under `Assets/` unless a file or folder explicitly documents an exception.

## Project layout

Follow the structure described in [README.md](README.md):

- **Runtime gameplay and UI:** `Assets/Scripts/`
- **Editor-only tools:** `Assets/Editor/`
- **Tests:** `Assets/Tests/` (Edit Mode / Play Mode as appropriate)
- **Scenes:** `Assets/Scenes/`
- **Runtime-loaded assets:** `Assets/Resources/` — use only when `Resources.Load` (or equivalent) is required; prefer references, ScriptableObjects, and Addressables patterns when the project adopts them.

Design and course documentation belong in **`Docs/`** at the repo root, not under `Assets/`.

## C# naming and style

| Element | Convention | Examples |
|--------|------------|----------|
| Types, methods, properties, events | PascalCase | `OutfitValidator`, `LoadScenario` |
| Public fields (avoid when possible) | PascalCase | Prefer properties or `[SerializeField]` private fields |
| Private instance fields | `_camelCase` | `_playButton`, `_currentEra` |
| Local variables and parameters | `camelCase` | `rootElement`, `eraId` |
| Constants | PascalCase static readonly or `const` | `MaxAccessoryCount` |

Unity engine message methods (`Start`, `Update`, `Awake`, `OnEnable`, etc.) must use **exact** names and signatures required by Unity.

## Typing and clarity

- Prefer **explicit types** in fields, properties, parameters, and return types.
- `var` is acceptable when the right-hand side makes the type obvious (for example `new List<EraConfig>()` or `GetComponent<T>()`).
- **Ternary expressions** (`condition ? a : b`) are allowed when they stay short and readable; prefer a clear `if`/`else` for non-trivial logic.

## Unity practices

- Use **`[SerializeField] private`** for values that designers should tune in the Inspector; avoid magic numbers in code when the value belongs in the editor.
- **Cache** component and `UIDocument` references in `Awake` / `OnEnable` where appropriate; avoid expensive lookups every frame in `Update`.
- Prefer **ScriptableObjects** (under `Assets/` as project data assets) for shared configuration, era definitions, and registries when data is reused across scenes.
- Use **events**, `UnityEvent`, or C# events for loose coupling instead of hard-wired singletons where practical.
- Keep **physics** in `FixedUpdate` and **input / frame logic** in `Update` according to Unity’s execution model.

## Error handling and logging

- Fail with **clear messages** (`Debug.LogError`, exceptions with context) rather than silent defaults that hide bugs.
- Remove temporary `Debug.Log` noise before merging; keep logs that aid diagnosis of genuine failure modes where useful.

## Documentation in code

- Add XML documentation comments (`///`) on **public** APIs and on non-obvious types or methods the team will call across assemblies.
- For `///` summaries, state purpose; document parameters and returns where it helps IntelliSense consumers.
- Comment **why** something non-obvious is done, not what the next line literally does.

## Testing

- Place automated tests under `Assets/Tests/` and keep them aligned with assemblies/project references Unity expects.
- Prefer testing **state, data, and pure logic** over brittle frame-by-frame visual assertions unless a test harness specifically requires it.

## Version control and assets

- Follow [.gitignore](.gitignore) for generated Unity folders (`Library/`, `Temp/`, etc.) and local tooling.
- Large binaries (textures, audio, models) should be tracked thoughtfully; use [.gitattributes](.gitattributes) and team policy (Git LFS or external hosting) for files that would bloat the repo.

## Versioning and change records

- **Version:** [VERSION.md](VERSION.md) is the source of truth for the product version; keep user-facing version labels in sync when you bump it.
- **AI-assisted work:** Significant AI-assisted changes should be reflected per [Docs/AI-Provenance-Log.md](Docs/AI-Provenance-Log.md) and team policy.

## Commits

Use clear messages, for example:

```text
<type>: <short subject>

Optional body with context.
```

Common types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `release`.

---

*This document can be revised by team agreement. When it conflicts with a client or course requirement, the requirement wins.*
