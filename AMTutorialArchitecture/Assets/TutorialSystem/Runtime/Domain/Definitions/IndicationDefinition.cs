using System;

namespace AMTutorialArchitecture.Tutorial.Domain.Definitions
{
    public sealed class IndicationDefinition
    {
        public IndicationDefinition(string targetId, TutorialInteractionPolicy interactionPolicy)
        {
            TargetId = targetId ?? string.Empty;
            InteractionPolicy = interactionPolicy;
        }

        public string TargetId { get; }

        public TutorialInteractionPolicy InteractionPolicy { get; }
    }
}
