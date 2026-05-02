using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// In-game credits: team names grouped by function. Each line under a section is one person’s name
    /// (no per-line role subtitle). The same name appears again in another section when their work spans that team.
    /// Third-party assets use structured fields (no raw URLs in-game). AI tools are name-only.
    /// </summary>
    public static class CreditsCatalog
    {
        /// <summary>Display heading for the AI tool disclosure block.</summary>
        public const string AiAssistanceSectionHeading = "AI ASSISTANCE";

        /// <summary>Display heading for the optional third-party block (only if <see cref="ThirdPartyCredits"/> is non-empty).</summary>
        public const string ThirdPartySectionHeading = "THIRD-PARTY ASSETS";

        /// <summary>Team by function: section heading and person names (duplicate names across sections when roles span teams).</summary>
        public static readonly (string Heading, IReadOnlyList<string> Lines)[] TeamByFunctionSections = new (string, IReadOnlyList<string>)[]
        {
            (
                "PRODUCTION",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Kevin McGhee",
                    "Nouf Alhamoudi",
                    "River Vondi",
                    "Chris Del Gesso",
                    "Ashley Haberberger",
                    "Matthew Glass"
                })
            ),
            (
                "DESIGN",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Prince Ukoha",
                    "Alex Ford",
                    "Matthew Nichols",
                    "River Vondi"
                })
            ),
            (
                "PROGRAMMING",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Jeff Fattic",
                    "Rush Doebelin",
                    "Markus Ross"
                })
            ),
            (
                "ART",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Bella Howe",
                    "Rachel Bratton",
                    "Bianca Darlington",
                    "Jade Dorman",
                    "Logan Hentschel",
                    "Jamontae Martin",
                    "Corey Miller",
                    "Jack Murray",
                    "Sam Nienaber",
                    "Bri Ochab",
                    "Tyler Rolwes",
                    "Evan White",
                    "Kaya Wilson"
                })
            ),
            (
                "AUDIO",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Ashley Haberberger",
                    "Jade Dorman"
                })
            ),
            (
                "WRITING & NARRATIVE",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Nicholas Best",
                    "Matthew Glass",
                    "Bella Howe",
                    "Rachel Bratton",
                    "Bianca Darlington",
                    "Jack Murray",
                    "Ashley Haberberger"
                })
            ),
            (
                "OPERATIONS, QA & RELEASE",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Chris Del Gesso",
                    "Matthew Nichols"
                })
            )
        };

        /// <summary>AI tools (name-only lines, same as team rows).</summary>
        public static readonly IReadOnlyList<string> AiAssistanceNames = new ReadOnlyCollection<string>(new string[]
        {
            "Cursor",
            "Composer 2",
            "Claude"
        });

        /// <summary>
        /// One third-party asset row for the credits scroll: display name, provenance, license, and in-game use.
        /// Full URLs remain in repository docs (<c>Docs/Assets.md</c>), not in these strings.
        /// </summary>
        public readonly struct ThirdPartyCreditEntry
        {
            /// <summary>
            /// Initializes a third-party credit row.
            /// </summary>
            /// <param name="assetName">Short title shown to the player.</param>
            /// <param name="source">Where the asset comes from (site or catalog name, plain text).</param>
            /// <param name="license">License summary plain text.</param>
            /// <param name="usage">How the game uses it.</param>
            public ThirdPartyCreditEntry(string assetName, string source, string license, string usage)
            {
                AssetName = assetName;
                Source = source;
                License = license;
                Usage = usage;
            }

            /// <summary>Short title for the asset.</summary>
            public string AssetName { get; }

            /// <summary>Provenance (no URLs).</summary>
            public string Source { get; }

            /// <summary>License summary.</summary>
            public string License { get; }

            /// <summary>In-game purpose.</summary>
            public string Usage { get; }
        }

        /// <summary>
        /// Shipped third-party rows. Empty collection hides the in-game third-party section.
        /// </summary>
        public static readonly IReadOnlyList<ThirdPartyCreditEntry> ThirdPartyCredits = new ReadOnlyCollection<ThirdPartyCreditEntry>(new ThirdPartyCreditEntry[]
        {
            new ThirdPartyCreditEntry(
                "Timer Start",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Chime at start of wardrobe timer."),
            new ThirdPartyCreditEntry(
                "Timer Warning",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Warning when wardrobe time is running low."),
            new ThirdPartyCreditEntry(
                "Equip Item SFX",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Clothing equip sound."),
            new ThirdPartyCreditEntry(
                "Unequip Item SFX",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Clothing unequip sound."),
            new ThirdPartyCreditEntry(
                "Swap Item SFX",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Clothing swap / change sound."),
            new ThirdPartyCreditEntry(
                "Timer End",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Sound when wardrobe timer ends."),
            new ThirdPartyCreditEntry(
                "Open — clothing chest",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Opening the clothing chest."),
            new ThirdPartyCreditEntry(
                "Open — shoe rack",
                "Pixabay (sound effects)",
                "Pixabay Content License — free for commercial use; attribution not required.",
                "Opening the shoe rack."),
            new ThirdPartyCreditEntry(
                "Speech bubble illustration",
                "Pixabay (illustrations)",
                "Pixabay Content License.",
                "Results screen speech bubble art."),
            new ThirdPartyCreditEntry(
                "Caldstone font",
                "DaFont — Caldstone typeface",
                "DaFont lists as free; license text on site is unspecified.",
                "General UI text."),
            new ThirdPartyCreditEntry(
                "Bonello font",
                "DaFont — Bonello typeface",
                "DaFont — free for personal use (see type page).",
                "Game title styling.")
        });
    }
}
