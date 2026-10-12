namespace AMTutorialArchitecture.Tutorial.Domain.Actions
{
    public interface IRequiredActionDefinition
    {
        string Description { get; }

        string ExpectedActionId { get; }

        bool IsSatisfiedBy(TutorialActionOccurrence occurrence);
    }
}
