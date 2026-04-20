using Assets.Scripts.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Scoring
{
    public sealed class ScoreItemResult
    {
        public float Score { get; }
        public string Commentary { get; }

        public ScoreItemResult(float score, string commentary)
        {
            Score = score;
            Commentary = commentary ?? string.Empty;
        }
    }

    public static class WardrobeScoringService
    {
        public static ScoreItemResult ScoreItem(
            string selectedId,
            string idealId,
            string idealCommentary,
            List<ScoredItem> scoredItems,
            string slotFeedback)
        {
            if (!string.IsNullOrEmpty(selectedId) && selectedId.StartsWith("nothing_"))
                selectedId = null;

            if ((string.IsNullOrEmpty(idealId) && string.IsNullOrEmpty(selectedId)) ||
                (!string.IsNullOrEmpty(idealId) && selectedId == idealId))
            {
                return new ScoreItemResult(1f, idealCommentary);
            }

            if (!string.IsNullOrEmpty(selectedId) && scoredItems != null)
            {
                foreach (var scoredRow in scoredItems)
                {
                    if (scoredRow.itemId == selectedId)
                    {
                        return new ScoreItemResult(scoredRow.score, scoredRow.commentary);
                    }
                }
            }

            return new ScoreItemResult(0f, slotFeedback);
        }
    }
}
