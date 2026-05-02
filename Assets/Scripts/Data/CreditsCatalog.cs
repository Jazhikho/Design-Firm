using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// In-game credits: team names grouped by function. Each line under a section is one person’s name
    /// (no per-line role subtitle). The same name appears again in another section when their work spans that team.
    /// Third-party lines are full attribution text (may wrap). AI tools are name-only.
    /// </summary>
    public static class CreditsCatalog
    {
        /// <summary>Display heading for the AI tool disclosure block.</summary>
        public const string AiAssistanceSectionHeading = "AI ASSISTANCE";

        /// <summary>Display heading for the optional third-party block (only if <see cref="ThirdPartyAttributions"/> is non-empty).</summary>
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
        /// Shipped, player-visible third-party credit lines. Leave empty to hide the in-game section.
        /// </summary>
        public static readonly IReadOnlyList<string> ThirdPartyAttributions = new ReadOnlyCollection<string>(new string[]
        {
            "Timer Start — Pixabay sound effect. https://pixabay.com/sound-effects/film-special-effects-game-start-317318/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Chime at start of timer.",

            "Timer Warning — Pixabay sound effect. https://pixabay.com/sound-effects/film-special-effects-warning-sound-6686/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Warning when time is low.",

            "Equip Item SFX — Pixabay sound effect. https://pixabay.com/sound-effects/household-clothes-drop-2-40202/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Clothing equip.",

            "Unequip Item SFX — Pixabay sound effect. https://pixabay.com/sound-effects/household-clothes-drop-2-40202/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Clothing unequip.",

            "Swap Item SFX — Pixabay sound effect. https://pixabay.com/sound-effects/household-fabric-rustling-and-sliding-25971/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Clothing change.",

            "Timer End — Pixabay sound effect. https://pixabay.com/sound-effects/film-special-effects-tv-show-hiter-351567/"
            + " — Pixabay Content License (free for commercial use; attribution not required). End of timer.",

            "Open / click clothing chest — Pixabay sound effect. https://pixabay.com/sound-effects/film-special-effects-wooden-trunk-latch-1-183944/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Chest open.",

            "Open / click shoe rack — Pixabay sound effect. https://pixabay.com/sound-effects/household-clothes-drop-2-40202/"
            + " — Pixabay Content License (free for commercial use; attribution not required). Shoe rack open.",

            "Speech bubble illustration — Pixabay. https://pixabay.com/illustrations/bubble-labels-shape-drawing-speech-4831953/"
            + " — Free for use under the Pixabay Content License.",

            "Caldstone font — https://www.dafont.com/caldstone.font — License on DaFont listed as free (license unspecified on page); UI body text.",

            "Bonello font — https://www.dafont.com/bonello.font — Free for personal use (per DaFont); game title styling."
        });
    }
}
