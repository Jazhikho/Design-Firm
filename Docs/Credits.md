# Credits

## Project

**Fashion Styling Game**  
Interactive styling game developed to support Dr. Trawick's History of Fashion course.

## Studio and Ownership

- **Studio:** Sprint 53 Studio
- **Primary studio page:** https://www.instagram.com/sprint53studio
- **Repository license:** MIT (see `LICENSE`)

## Academic Context

- **Client / course context:** Dr. Trawick's History of Fashion course
- **Core eras covered:** 1980s, 1990s, 2000s, 2010s
- **Project brief and requirements:** `Docs/ClientSpec.md`

## Research and Reference Sources

Fashion-history references used for course-aligned era coverage are tracked in:

- `Docs/Sources.md`

## Third-Party Assets and Attribution

Third-party asset attribution and license tracking are maintained in:

- `Docs/Assets.md`

## AI Assistance Disclosure

AI-assisted work disclosures and human-audit notes are maintained in:

- `Docs/AI-Provenance-Log.md`
- `AI-Use-Statement.md`

When listing assistant tools in in-game, marketing, or ship credits, use: **Cursor** — repository management and merge-patching assistance; **Composer 2** — credits screen prompt/support and implementation assistance.

## Team and Role Credits

The **in-game** main menu “Credits” screen reads team data from `Assets/Scripts/Data/CreditsCatalog.cs`, grouped by **function** (production, design, programming, art, and so on) via `TeamByFunctionSections`, plus the AI assistance block. A “Third-Party Assets” block appears in the build **only** when `ThirdPartyAttributions` in that file is non-empty; keep those entries in sync with confirmed rows in `Docs/Assets.md` (the game does not read `Docs/Assets.md` at runtime).

Current team names and role assignments in external team docs are also linked from `README.md` under **External Docs and Tools**.
