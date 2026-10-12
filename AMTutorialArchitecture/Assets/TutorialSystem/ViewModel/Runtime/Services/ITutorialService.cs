using System;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Commands;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Domain.Model;

namespace AMTutorialArchitecture.Tutorial.Application.Services
{
    public interface ITutorialService
    {
        event EventHandler<TutorialStateChangedEventArgs> StateChanged;

        TutorialSnapshot Current { get; }

        TutorialStartResult Start(TutorialSequenceDefinition sequence);

        TutorialActionResult SubmitAction(TutorialActionOccurrence occurrence);

        TutorialSkipResult Skip();
    }
}
