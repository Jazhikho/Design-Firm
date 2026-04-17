# AI Provenance Log

Disclosure and audit trail for AI-assisted work. Update when committing significant AI-assisted contributions; include relevant disclosure, source verification, or human-audit notes in touched docs.

## Format (for future entries)

**Date:**  
**Tool or model used:**  
**Task purpose:**  
**Input materials used:**  
**Summary of AI contribution:**  
**What the human accepted / rejected / changed:**  
**Validation method used:**  
**Final approver:** (if accepted / applicable)

---

**Date:** 2026-04-16  
**Tool or model used:** Claude Code (claude-sonnet-4-6, Claude Code CLI)  
**Task purpose:** Introduce a cross-scene `AudioManager` singleton and wire `Universal_Button_SFX_2.wav` to the Play and Quit button click handlers in `MainMenuController`; SFX must fire before any other handler logic.  
**Input materials used:** `Assets/Scripts/UI/MainMenuController.cs`; `Assets/Audio/SFX/Universal_Button_SFX_2.wav`; existing Addressables and controller patterns in `WardrobeController.cs`, `ResultsController.cs`, `StartupServices.cs`; user requirements and approval via plan-mode review.  
**Summary of AI contribution:** Created `Assets/Scripts/Core/AudioManager.cs` — `[RequireComponent(typeof(AudioSource))]` singleton with `DontDestroyOnLoad`, serialized `_buttonSfx` AudioClip, `PlayButtonSfx()` via `PlayOneShot`, and `ButtonSfxLength` property; updated `MainMenuController.cs` to call `AudioManager.Instance.PlayButtonSfx()` as the first line of both `OnPlayClicked` and `OnQuitClicked`; replaced the immediate `Application.Quit()` call with a coroutine (`QuitAfterSfx`) that waits `ButtonSfxLength` seconds before quitting so the SFX is audible; added `using System.Collections` and `using Assets.Scripts.Core` to the using block; advised against making the SFX an Addressable (entry-scene async race condition, single tiny asset); documented cross-scene music extension pattern for future use.  
**What the human accepted / rejected / changed:** Accepted plan as written after plan-mode review; no changes requested.  
**Validation method used:** File review of both scripts; manual Unity Editor play-mode test required (see plan verification steps).  
**Final approver:** Jeff Fattic  

---

**Date:** 2026-04-15  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Merge GitHub PR #41 into `main` with a documented merge commit, then apply code-only follow-ups on `WardrobeController` (no layout polish); bump internal `VERSION.md` label; push `origin/main`.  
**Input materials used:** `origin/rushBranchNew`; PR #41 diff and prior review notes; `WardrobeController.cs`, `VERSION.md`, this log.  
**Summary of AI contribution:** Recreated merge as `--no-ff` with merge message referencing PR #41; removed unused hover state field; added `ApplyScenarioDescriptionLabel` with null check for `scenarioDesc`, scenario text when `ActiveScenario` is null tied to `_sandboxMode` plus warning and user-facing fallback when not sandbox; replaced `MouseOverEvent` with `PointerEnterEvent`; renamed hover handler to `WardrobeTilePointerEntered` with early returns, null-safe labels, `currentTarget` as `Button`, and `description ?? string.Empty`; updated `VERSION.md` to 0.0.4.1; this entry.  
**What the human accepted / rejected / changed:** Accepted via request to merge PR #41, apply corrections, and push (layout polish explicitly out of scope).  
**Validation method used:** `read_lints` on `WardrobeController.cs`; `git log` / `git push` verification.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-11  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Set Unity player-facing `bundleVersion` to 0.2 while keeping the internal four-part repo label in `VERSION.md`; document the split; commit and push.  
**Input materials used:** `ProjectSettings/ProjectSettings.asset`; `VERSION.md`; user request to decouple user-facing and internal versioning.  
**Summary of AI contribution:** Updated `bundleVersion` from 0.1 to 0.2; revised `VERSION.md` to state player-facing 0.2 vs internal 0.0.4.0; this entry.  
**What the human accepted / rejected / changed:** Accepted via direct request (“humor me” on intentional mismatch).  
**Validation method used:** Grep for `bundleVersion`; file review.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-11  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** After merge of PR #31, bump repository version label and replace machine-specific or broken Claude Code local Bash allow-list entries with paths for the `Design Firm` workspace; commit and push to `origin`.  
**Input materials used:** `VERSION.md` (was 0.0.3.3); `.claude/settings.local.json` from merged PR #31; user direction to handle version and settings locally.  
**Summary of AI contribution:** Set `VERSION.md` to 0.0.4.0 with summary line for PR #31 and this housekeeping; rewrote `.claude/settings.local.json` `permissions.allow` to use `\"/d/School/Design Firm\"` and `git -C` for the same tree, removed invalid `python3 -c` stubs; this entry.  
**What the human accepted / rejected / changed:** Accepted via request to perform bump and settings fix after merge.  
**Validation method used:** JSON parse check on `settings.local.json`; `git status` / push.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-10  
**Tool or model used:** Claude Code (claude-code CLI, Claude Opus 4)  
**Task purpose:** Introduce Unity Addressables for async data/sprite loading; reorganize art, scene, and script assets into a cleaner structure; replace legacy wardrobe scripts with event-driven Core/Data/UI architecture.  
**Input materials used:** Existing `Assets/Resources/`, `Assets/UI/Placeholders/Sprites/`, `Assets/Scripts/WardrobeScripts/`, `Assets/Scripts/UI/MainMenuScript.cs`, `Assets/Data/scenarios.json`; Unity Addressables documentation; `CodingStandards.md`.  
**Summary of AI contribution:** Added `com.unity.addressables` package and generated `Assets/AddressableAssetsData/` (settings, groups, schemas, templates, data builders); moved art from `Resources/` and `UI/Placeholders/Sprites/` into `Assets/Art/Avatars/` and `Assets/Art/Clothing/{Bottoms,Shoes,Tops}`; added `Assets/Data/clothing_items.json` with 36 items across four slots; updated `scenarios.json` with `avatarImage`, `era`, `category` fields and fixed trailing-comma parse error; removed legacy `Assets/Resources/wardrobeTempResources/`, `Assets/Data/EXAMPLEitemsJson.json`, `itemsJson.json`, `JSONinstructions.txt`; deleted `Assets/Editor/wardrobeManagerRelated/` (LoadManagerOnPlay, editor prefab); deleted `Assets/Scripts/WardrobeScripts/` (WardrobeButtonScripts, WardrobeItem, WardrobeItemList, WardrobeManagerScript) and `TaskResultScripts/TempScenarioPlaceholder.cs`; created `Assets/Scripts/Core/` with `GameConstants` (moved from root), `ScenarioState` (singleton with events), `StartupServices` (`RuntimeInitializeOnLoadMethod` loading scenarios and clothing JSON from Addressables), `WardrobeState` (singleton with per-slot change events and LINQ-filtered available-item getters); created `Assets/Scripts/Data/` with `DataLoader`, `Scenario`/`ScenariosWrapper`, `WardrobeItem`/`WardrobeItemsWrapper` (added `ClothingSlot` enum with computed `SlotType` property for type-safe slot dispatch while keeping human-readable JSON strings); replaced `MainMenuScript` with `MainMenuController` (runtime `Application.platform` check instead of `#if UNITY_WEBGL`); created `WardrobeController` (UI Toolkit grids, `RegisterCallback<ClickEvent, TileButtonData>`, Addressables sprite loading with location pre-check and fallback labels, cached `Image` fields updated via state change events, disabled tiles for missing art); refactored `ScenariosController` (added `ToggleButtonGroup` decade/category filters, scenario cards in horizontal `ScrollView`, filter logic via `ToggleButtonGroupState` bitmask); refactored `ResultsController`; added `Assets/UI/USS/ScenariosStylesheet.uss`; updated `WardrobeStylesheet.uss` (`.missing-art` gray disabled style, `.selected` highlight); updated `WardrobeUI.uxml` and `ScenariosUI.uxml`; updated `ProjectSettings/EditorBuildSettings.asset` scene list; this entry.  
**What the human accepted / rejected / changed:** Accepted via iterative request-review cycle across multiple sessions.  
**Validation method used:** Runtime testing in Unity editor across scenario selection and wardrobe scenes; console error diagnosis and fixes (null refs, Addressable key mismatches, event wiring, `evt.currentTarget` vs `evt.target`).  
**Final approver:** Jeff Fattic  

---

**Date:** 2026-04-10  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** After merge of PR #28 (`rushBranch`), apply coding-standards and robustness fixes to results/temp scenario code and editor wardrobe bootstrap; post PR follow-up notes; bump `VERSION.md`; push `main`.  
**Input materials used:** Merged PR #28 tree; `CodingStandards.md`; `ResultsController`, `TempScenarioPlaceholder`, `LoadManagerOnPlay`; GitHub PR #28.  
**Summary of AI contribution:** Merged PR #28 via `gh`; renamed `loadManagerOnPlay` → `LoadManagerOnPlay` (static class, prefab path const, early return); replaced `tempScenarioItemTesting` with `TempScenarioPlaceholder` (PascalCase types: `ScoredItem`, `IdealOutfit`, …); `ResultsController`: `_root`, `_totalScore` reset, null guards for scenario/scored list/items, `ScoreItem` safe when `WardrobeItemClothing` null, typo `totalScoreDispaly` fixed, `IdealItemHint` overload for `IdealOutfitItem`; folder `taskResultScripts` → `TaskResultScripts`; PR comment with done vs backlog; `VERSION.md` 0.0.3.3; this entry.  
**What the human accepted / rejected / changed:** Accepted via request to merge, fix, document, and push.  
**Validation method used:** `read_lints` on edited C#; grep for old symbol names.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-08  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Trim README doc links (remove Credits and GDD); delete `Docs/GDD.md`; document that `CodingStandards.md` is at repository root; bump `VERSION.md`; commit and push.  
**Input materials used:** User request; `README.md`, `Docs/GDD.md`, `VERSION.md`, this log.  
**Summary of AI contribution:** README restructured (`Docs/` vs coding standards section); removed broken Credits and GDD entries; deleted `GDD.md`; `VERSION.md` 0.0.3.2; this entry.  
**What the human accepted / rejected / changed:** Accepted via direct request.  
**Validation method used:** Grep for stale `GDD` / `Credits` links in markdown.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-08  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Final coding-standards review on `main`; align scripts with `CodingStandards.md`; commit and push.  
**Input materials used:** Staged changes on `main` (`GameConstants`, `UI/*Controller`, `WardrobeScripts/*`, `wardrobeScene.unity`); `CodingStandards.md`; `VERSION.md`; this log.  
**Summary of AI contribution:** Third-pass review; moved `[SerializeField]` below XML summaries for serialized fields; corrected `SetUpClothing` summary (grid vs ListView); added null checks after `Q<Button>` in main menu, results, and scenarios UI; clarified manager inspector error and XML on JSON DTO types; bumped `VERSION.md` to 0.0.3.1; this entry.  
**What the human accepted / rejected / changed:** Accepted via request to commit and push after review.  
**Validation method used:** Read-through against `CodingStandards.md`; `read_lints` on edited C# paths.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-03  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Merge PR #18 (`rushBranch`) into `main` and apply immediate hardening; align with repo `CodingStandards.md` and contribution records (`VERSION.md`, this log).  
**Input materials used:** Prior PR #18 review; `git fetch` / `git merge origin/rushBranch`; `wardrobeButtonScripts.cs`, `wardrobeItemList.cs`, `wardrobeManagerScript.cs`, `WardrobeUITEMP.uxml`, `VERSION.md`.  
**Summary of AI contribution:** Fast-forward merge from `origin/rushBranch`; added `DefaultExecutionOrder` so JSON loads before UI wiring; `GetFirstDisplayableItem` for defaults; null-safe `Q` usage, guarded `OnDisable`, `string.IsNullOrEmpty` before `LoadScene`, safer `itemsChosen`; timer label wiring when assigned; renamed static/item types to PascalCase (`NewItemAdd`, `SetItemSlot`, `GetItemSlot`, `NewSerialItem`, `SerialItems`); `[SerializeField]` for inspector fields; XML comment in UXML for Jeff restoring River’s layout; updated `wardrobeItem.cs` summary; bumped `VERSION.md`; this log entry.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** Repo-wide grep for removed symbol names; IDE diagnostics on edited C# files.  
**Final approver:** Chris Del Gesso  

---

**Date:** 2026-04-02  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Fix Unity Console Semantic warning “The specified URL is empty or invalid” on wardrobe UXML.  
**Input materials used:** Console screenshot (line 42); `WardrobeUITEMP.uxml`.  
**Summary of AI contribution:** Removed unused `data-source=""` and editor-only `double-click-selects-word` from Proceed `Button`; bumped `VERSION.md`; this log entry.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** Attribute at reported line matched empty binding URL pattern.  
**Final approver:** Chris Del Gesso

---

**Date:** 2026-04-02  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Integrate merged `main` (wardrobe UI PR #12) into `rushBranch` (PR #16) preferring #16 asset naming (`WardrobeUITEMP`, `WardrobeStylesheetTEMP`); resolve modify/delete conflict; wire scene references; tighten wardrobe scripts.  
**Input materials used:** Local `git merge origin/main` on `rushBranch`; prior PR review notes; Unity scene and UXML/USS paths.  
**Summary of AI contribution:** Combined #12 full wardrobe layout with #16 `rushTempLeft` / Proceed controls inside `WardrobeUITEMP.uxml`; removed `WardrobeUI.uxml`; pointed Style at `WardrobeStylesheetTEMP.uss`; replaced `GameObject.Find` with `[SerializeField]` and YAML references in `wardrobeScene.unity`; validation and null checks in manager, button test, picker, and slot scripts; removed empty `wardrobeItem` MonoBehaviour and unused `wardrobeAvatarScript`; bumped `VERSION.md`; this log entry.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** Merge conflict simulation resolved; grep for stale `WardrobeUI` / `WardrobeStylesheet.uss` paths; file review of scene fileIDs for slot components.  
**Final approver:** Chris Del Gesso

---

**Date:** 2026-04-02  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Add repository-root coding standards for Unity C#; prepare commit and push per team workflow (`VERSION.md`, README discoverability).  
**Input materials used:** User request; existing `README.md`, `VERSION.md`, and project layout.  
**Summary of AI contribution:** Authored `CodingStandards.md` (layout, naming, Unity practices, versioning pointers; ternary expressions allowed when readable); bumped `VERSION.md`; added README Docs link; this log entry.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** File review against README structure and `VERSION.md` pattern.  
**Final approver:** Chris Del Gesso

---

**Date:** 2026-03-29  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Align repository folder layout with Unity (`Assets/` hierarchy, `Editor`/`Plugins` placeholders); Unity-oriented `.gitignore`; update `README.md` and `VERSION.md`.  
**Input materials used:** User request; existing folder layout, `README.md`, and `.gitignore`.  
**Summary of AI contribution:** Moved `Scenes`, `Scripts`, `Art`, `Audio`, `Resources`, `Shaders`, `Tests` under `Assets/` via `git mv`; added empty `Assets/Editor` and `Assets/Plugins`; merged GitHub Unity `.gitignore` patterns and scoped generic binary ignores; revised README project structure table and Unity notes.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** Path review; `git status` for moves.  
**Final approver:** Chris Del Gesso

---

**Date:** 2026-03-22  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Add studio home URL (Instagram), MIT license file, and Sprint 53 Studio ownership to README; bump `VERSION.md`.  
**Input materials used:** User request; existing `README.md`, `VERSION.md`.  
**Summary of AI contribution:** Drafted `LICENSE` (MIT, copyright Sprint 53 Studio), README ownership/licensing lines, version note.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** File review against user requirements.  
**Final approver:** Chris Del Gesso

---

**Date:** 2026-03-22  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Add `Docs/Credits.md` with organized team credits; link from README; bump `VERSION.md`.  
**Input materials used:** User-supplied name and role list; existing `README.md`, `VERSION.md`.  
**Summary of AI contribution:** Structured lead-role table plus alphabetical roster; preserved user wording for titles.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** Cross-check against provided roster.  
**Final approver:** Chris Del Gesso

---

**Date:** 2026-03-22  
**Tool or model used:** Cursor agent (AI-assisted editing)  
**Task purpose:** Reformat `Docs/Credits.md` to read like an in-game credit sequence.  
**Input materials used:** Existing `Credits.md` roster and roles.  
**Summary of AI contribution:** Section ordering, game-style headers, centered splash/closing HTML blocks, role grouping.  
**What the human accepted / rejected / changed:** (fill at commit / review)  
**Validation method used:** Cross-check names and roles against prior credits list.  
**Final approver:** Chris Del Gesso
