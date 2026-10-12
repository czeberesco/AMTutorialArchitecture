using System;

namespace AMTutorialArchitecture.Tutorial.Domain.Actions
{
    public sealed class SemanticActionRequiredActionDefinition : IRequiredActionDefinition
    {
        public SemanticActionRequiredActionDefinition(string expectedActionId, string description = null)
        {
            ExpectedActionId = expectedActionId ?? string.Empty;
            Description = string.IsNullOrWhiteSpace(description)
                ? $"Complete action '{ExpectedActionId}'."
                : description;
        }

        public string Description { get; }

        public string ExpectedActionId { get; }

        public bool IsSatisfiedBy(TutorialActionOccurrence occurrence)
        {
            return occurrence != null
                && occurrence.Succeeded
                && string.Equals(occurrence.ActionId, ExpectedActionId, StringComparison.Ordinal);
        }
    }
}
