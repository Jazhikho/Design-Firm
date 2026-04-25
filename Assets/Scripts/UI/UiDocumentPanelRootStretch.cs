using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Shared layout for UI Toolkit screens: the UIDocument panel root and optional <see cref="AppDocumentRootName"/>
    /// shell must fill the Game view so flex and percentage heights resolve correctly across aspect ratios.
    /// </summary>
    public static class UiDocumentPanelRootStretch
    {
        /// <summary>
        /// UXML name of the full-viewport wrapper used on multi-root screens (scenario, wardrobe, settings, results).
        /// </summary>
        public const string AppDocumentRootName = "AppDocumentRoot";

        /// <summary>
        /// Applies column flex and 100% size to the UIDocument <paramref name="panelRoot"/> so children participate in full-panel layout.
        /// </summary>
        /// <param name="panelRoot">The value of <see cref="UIDocument.rootVisualElement"/>.</param>
        public static void ApplyToPanelRoot(VisualElement panelRoot)
        {
            if (panelRoot == null)
            {
                Debug.LogError("UiDocumentPanelRootStretch.ApplyToPanelRoot: panelRoot is null.");
                return;
            }

            panelRoot.style.flexDirection = FlexDirection.Column;
            panelRoot.style.flexGrow = 1f;
            panelRoot.style.flexShrink = 0f;
            panelRoot.style.minHeight = 0f;
            panelRoot.style.minWidth = 0f;
            panelRoot.style.width = new Length(100f, LengthUnit.Percent);
            panelRoot.style.height = new Length(100f, LengthUnit.Percent);
        }

        /// <summary>
        /// If a visual element named <see cref="AppDocumentRootName"/> exists under <paramref name="panelRoot"/>, stretches it to fill the panel.
        /// </summary>
        /// <param name="panelRoot">The UIDocument root visual element.</param>
        public static void TryApplyToAppDocumentShell(VisualElement panelRoot)
        {
            if (panelRoot == null)
            {
                return;
            }

            VisualElement shell = panelRoot.Q<VisualElement>(AppDocumentRootName);
            if (shell == null)
            {
                return;
            }

            shell.style.flexGrow = 1f;
            shell.style.flexShrink = 0f;
            shell.style.minHeight = 0f;
            shell.style.minWidth = 0f;
            shell.style.width = new Length(100f, LengthUnit.Percent);
            shell.style.height = new Length(100f, LengthUnit.Percent);
        }

        /// <summary>
        /// Applies <see cref="ApplyToPanelRoot"/> and <see cref="TryApplyToAppDocumentShell"/> in one call for standard screens.
        /// </summary>
        /// <param name="panelRoot">The UIDocument root visual element.</param>
        public static void ApplyToPanelRootAndAppShell(VisualElement panelRoot)
        {
            ApplyToPanelRoot(panelRoot);
            TryApplyToAppDocumentShell(panelRoot);
        }
    }
}
