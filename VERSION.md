# Version

**Player-facing (Unity `ProjectSettings` bundle version, `Application.version`):** 0.2 — intentionally separate from the internal label below.

**Internal / repo build label:** 0.0.5.5 — Merged PRs #64 (settings scene and slider), #65 (art/data/schema updates), #66 (new trunk visuals), and #67 (UI styling). Applied post-merge fixes on `main`: corrected broken scenario `itemId` references (`2000sBusinessTop`, `1980sFormalShoes`), fixed `1990s Casual` avatar key (`Avatars/2000sFemModel.png`), and hardened settings volume logic (`AudioManager` master-volume accessors plus callback unregistration). Player-facing bundle version unchanged at 0.2.
