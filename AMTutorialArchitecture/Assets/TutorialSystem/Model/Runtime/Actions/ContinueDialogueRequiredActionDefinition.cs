using System;

namespace AMTutorialArchitecture.Tutorial.Domain.Actions
{
    public sealed class ContinueDialogueRequiredActionDefinition : IRequiredActionDefinition
    {
        public ContinueDialogueRequiredActionDefinition(string expectedActionId)
        {
            ExpectedActionId = expectedActionId ?? string.Empty;
        }

        public string Description => "Continue the tutorial dialogue.";

        public string ExpectedActionId { get; }

        public bool IsSatisfiedBy(TutorialActionOccurrence occurrence)
        {
            return occurrence != null
                && occurrence.Succeeded
                && string.Equals(occurrence.ActionId, ExpectedActionId, StringComparison.Ordinal);
        }
    }
}
