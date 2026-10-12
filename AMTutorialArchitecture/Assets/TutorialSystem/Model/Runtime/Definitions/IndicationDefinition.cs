using System;

namespace AMTutorialArchitecture.Tutorial.Domain.Definitions
{
    public sealed class IndicationDefinition
    {
        public IndicationDefinition(string targetId)
        {
            TargetId = targetId ?? string.Empty;
        }

        public string TargetId { get; }
    }
}
