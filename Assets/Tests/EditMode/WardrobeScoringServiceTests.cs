using System.Collections.Generic;
using NUnit.Framework;
using Assets.Scripts.Core.Scoring;
using Assets.Scripts.Data;

public class WardrobeScoringServiceTests
{
    [Test]
    public void ScoreItem_ReturnsFullPoint_When_SelectedMatchesIdeal()
    {
        var result = WardrobeScoringService.ScoreItem(
            selectedId: "jacket_01",
            idealId: "jacket_01",
            idealCommentary: "Perfect choice.",
            scoredItems: null,
            slotFeedback: "Wrong jacket.");

        Assert.That(result.Score, Is.EqualTo(1f));
        Assert.That(result.Commentary, Is.EqualTo("Perfect choice."));
    }

    [Test]
    public void ScoreItem_ReturnsFullPoint_When_BothSelectedAndIdealAreEmpty()
    {
        var result = WardrobeScoringService.ScoreItem(
            selectedId: null,
            idealId: null,
            idealCommentary: "Correct to wear nothing here.",
            scoredItems: null,
            slotFeedback: "Wrong.");

        Assert.That(result.Score, Is.EqualTo(1f));
        Assert.That(result.Commentary, Is.EqualTo("Correct to wear nothing here."));
    }

    [Test]
    public void ScoreItem_TreatsNothingPrefixAsEmpty()
    {
        var result = WardrobeScoringService.ScoreItem(
            selectedId: "nothing_jacket",
            idealId: null,
            idealCommentary: "Correct empty slot.",
            scoredItems: null,
            slotFeedback: "Wrong.");

        Assert.That(result.Score, Is.EqualTo(1f));
        Assert.That(result.Commentary, Is.EqualTo("Correct empty slot."));
    }

    [Test]
    public void ScoreItem_ReturnsPartialCredit_When_SelectedItemExistsInScoredItems()
    {
        var scoredItems = new List<ScoredItem>
        {
            new() {
                itemId = "top_02",
                score = 0.5f,
                commentary = "Close, but not ideal."
            }
        };

        var result = WardrobeScoringService.ScoreItem(
            selectedId: "top_02",
            idealId: "top_01",
            idealCommentary: "Perfect.",
            scoredItems: scoredItems,
            slotFeedback: "Wrong top.");

        Assert.That(result.Score, Is.EqualTo(0.5f));
        Assert.That(result.Commentary, Is.EqualTo("Close, but not ideal."));
    }

    [Test]
    public void ScoreItem_ReturnsZero_When_NoMatchAndNoPartialCredit()
    {
        var result = WardrobeScoringService.ScoreItem(
            selectedId: "shoes_99",
            idealId: "shoes_01",
            idealCommentary: "Perfect.",
            scoredItems: new List<ScoredItem>(),
            slotFeedback: "Those shoes do not fit the scenario.");

        Assert.That(result.Score, Is.EqualTo(0f));
        Assert.That(result.Commentary, Is.EqualTo("Those shoes do not fit the scenario."));
    }

    [Test]
    public void ScoreItem_ReturnsZero_When_SelectedIsNullAndIdealHasValue()
    {
        var result = WardrobeScoringService.ScoreItem(
            selectedId: null,
            idealId: "bottoms_01",
            idealCommentary: "Perfect.",
            scoredItems: null,
            slotFeedback: "Missing required bottoms.");

        Assert.That(result.Score, Is.EqualTo(0f));
        Assert.That(result.Commentary, Is.EqualTo("Missing required bottoms."));
    }
}