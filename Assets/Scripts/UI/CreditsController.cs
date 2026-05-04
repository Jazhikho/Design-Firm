using Assets.Scripts.Core;
using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CreditsController : MonoBehaviour
{
    private Button _btnBack;

    private ScrollView _scrollCredits;

    private void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        _btnBack = root.Q<Button>("btnBack");
        _btnBack?.RegisterCallback<ClickEvent>(OnCreditsCloseButtonClick);

        _scrollCredits = root.Q<ScrollView>("CreditsScroll");

        BuildCreditsScrollContent();
    }

    private void OnDisable()
    {
        _btnBack?.UnregisterCallback<ClickEvent>(OnCreditsCloseButtonClick);
    }

    /// <summary>
    /// Dismisses the credits overlay when Close is used.
    /// </summary>
    private void OnCreditsCloseButtonClick(ClickEvent _)
    {
        AudioManager.Instance.PlayButtonSfx();
        NavigationManager.Instance.NavigateToScene(GameConstants.MainMenuScene);
    }

    /// <summary>
    /// Fills the credits scroll with section headings and one label per name (no role sub-lines); third-party
    /// lines appear only when <see cref="CreditsCatalog.ThirdPartyAttributions"/> has entries.
    /// </summary>
    private void BuildCreditsScrollContent()
    {
        if (_scrollCredits == null) return;

        _scrollCredits.contentContainer.Clear();
        bool firstTeamSection = true;
        foreach ((string teamHeading, IReadOnlyList<string> teamLines) in CreditsCatalog.TeamByFunctionSections)
        {
            AddCreditsSectionHeading(_scrollCredits.contentContainer, teamHeading, firstTeamSection);
            firstTeamSection = false;
            foreach (string personName in teamLines)
            {
                AddCreditsNameLine(_scrollCredits.contentContainer, personName);
            }
        }

        AddCreditsSectionHeading(_scrollCredits.contentContainer, CreditsCatalog.AiAssistanceSectionHeading, false);
        foreach (string toolName in CreditsCatalog.AiAssistanceNames)
        {
            AddCreditsNameLine(_scrollCredits.contentContainer, toolName);
        }

        IReadOnlyList<string> thirdParty = CreditsCatalog.ThirdPartyAttributions;
        if (thirdParty.Count > 0)
        {
            AddCreditsSectionHeading(_scrollCredits.contentContainer, CreditsCatalog.ThirdPartySectionHeading, false);
            foreach (string line in thirdParty)
            {
                if (line == null) continue;

                string t = line.Trim();
                if (t.Length == 0) continue;

                Label row = new(t);
                row.AddToClassList("credits-name-line");
                _scrollCredits.contentContainer.Add(row);
            }
        }
    }

    /// <summary>
    /// Adds a centered, all-caps section label for the credits panel.
    /// </summary>
    private void AddCreditsSectionHeading(VisualElement parent, string title, bool isFirst)
    {
        Label heading = new(title);
        heading.AddToClassList("credits-section-heading");
        if (isFirst)
            heading.AddToClassList("credits-section-heading-first");

        parent.Add(heading);
    }

    /// <summary>
    /// Adds a single centered name line for credits (team, AI tools, or third-party text).
    /// </summary>
    private void AddCreditsNameLine(VisualElement parent, string name)
    {
        if (name == null) return;

        string trimmed = name.Trim();
        if (trimmed.Length == 0) return;

        Label row = new(trimmed);
        row.AddToClassList("credits-name-line");
        parent.Add(row);
    }
}