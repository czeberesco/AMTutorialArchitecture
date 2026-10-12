using System;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;

namespace AMTutorialArchitecture.Tutorial.Domain.Commands
{
    public sealed class StartTutorialCommand
    {
        public StartTutorialCommand(TutorialSequenceDefinition sequence)
        {
            Sequence = sequence;
        }

        public TutorialSequenceDefinition Sequence { get; }
    }

    public sealed class SubmitTutorialActionCommand
    {
        public SubmitTutorialActionCommand(TutorialActionOccurrence occurrence)
        {
            Occurrence = occurrence ?? throw new ArgumentNullException(nameof(occurrence));
        }

        public TutorialActionOccurrence Occurrence { get; }
    }

    public sealed class SkipTutorialCommand
    {
    }
}
