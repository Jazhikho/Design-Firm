using Assets.Scripts.Data;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public sealed class ScenarioState
    {
        public static ScenarioState Instance { get; } = new();

        /// <summary>
        /// Raised once after scenarios have been loaded from Addressables.
        /// Subscribers that register after loading has already completed will be invoked immediately.
        /// </summary>
        public event Action ScenariosLoaded;

        public bool IsScenariosLoaded { get; private set; }

        public List<Scenario> Scenarios { get; set; } = new();

        public Scenario ActiveScenario { get; set; }

        /// <summary>
        /// Marks scenarios as loaded and raises <see cref="ScenariosLoaded"/>.
        /// Called by StartupServices after Addressables loading completes.
        /// </summary>
        internal void NotifyScenariosLoaded()
        {
            IsScenariosLoaded = true;
            ScenariosLoaded?.Invoke();
        }

        /// <summary>
        /// Private constructor to enforce singleton pattern. Use <see cref="Instance"/> to access the single GameState instance.
        /// </summary>
        private ScenarioState() { }
    }
}
