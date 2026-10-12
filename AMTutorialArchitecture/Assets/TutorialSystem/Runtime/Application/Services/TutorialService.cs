using System;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Commands;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Domain.Model;

namespace AMTutorialArchitecture.Tutorial.Application.Services
{
    public sealed class TutorialService : ITutorialService
    {
        private readonly TutorialCommandProcessor processor;
        private readonly TutorialModel model;

        public TutorialService(TutorialCommandProcessor processor, TutorialModel model)
        {
            this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public event EventHandler<TutorialStateChangedEventArgs> StateChanged;

        public TutorialSnapshot Current => model.Snapshot;

        public TutorialStartResult Start(TutorialSequenceDefinition sequence)
        {
            var previous = Current;
            var result = processor.Execute(new StartTutorialCommand(sequence));
            if (result.Status == TutorialStartStatus.Started)
            {
                Publish(previous, result.Snapshot, TutorialStateChangeReason.Started);
            }

            return result;
        }

        public TutorialActionResult SubmitAction(TutorialActionOccurrence occurrence)
        {
            var previous = Current;
            var result = processor.Execute(new SubmitTutorialActionCommand(occurrence));
            switch (result.Status)
            {
                case TutorialActionStatus.Advanced:
                    Publish(previous, result.Snapshot, TutorialStateChangeReason.Advanced);
                    break;
                case TutorialActionStatus.Completed:
                    Publish(previous, result.Snapshot, TutorialStateChangeReason.Completed);
                    break;
            }

            return result;
        }

        public TutorialSkipResult Skip()
        {
            var previous = Current;
            var result = processor.Execute(new SkipTutorialCommand());
            if (result.Status == TutorialSkipStatus.Skipped)
            {
                Publish(previous, result.Snapshot, TutorialStateChangeReason.Skipped);
            }

            return result;
        }

        private void Publish(
            TutorialSnapshot previous,
            TutorialSnapshot current,
            TutorialStateChangeReason reason)
        {
            StateChanged?.Invoke(this, new TutorialStateChangedEventArgs(previous, current, reason));
        }
    }
}
