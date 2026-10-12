using System;

namespace AMTutorialArchitecture.Tutorial.Domain.Actions
{
    public sealed class TutorialActionOccurrence
    {
        public TutorialActionOccurrence(string actionId, bool succeeded)
        {
            if (string.IsNullOrWhiteSpace(actionId))
            {
                throw new ArgumentException("An action ID is required.", nameof(actionId));
            }

            ActionId = actionId;
            Succeeded = succeeded;
        }

        public string ActionId { get; }

        public bool Succeeded { get; }

        public static TutorialActionOccurrence Success(string actionId)
        {
            return new TutorialActionOccurrence(actionId, true);
        }

        public static TutorialActionOccurrence Failure(string actionId)
        {
            return new TutorialActionOccurrence(actionId, false);
        }
    }
}
