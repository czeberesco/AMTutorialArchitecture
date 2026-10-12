namespace AMTutorialArchitecture.Tutorial.Domain.Actions
{
    public sealed class ContinueDialogueRequiredActionDefinition : IRequiredActionDefinition
    {
        public string Description => "Continue the tutorial dialogue.";

        public string ExpectedActionId => TutorialActionIds.ContinueDialogue;

        public bool IsSatisfiedBy(TutorialActionOccurrence occurrence)
        {
            return occurrence != null
                && occurrence.Succeeded
                && occurrence.ActionId == TutorialActionIds.ContinueDialogue;
        }
    }
}
