using System;
using AMTutorialArchitecture.Tutorial.Domain.Model;
using AMTutorialArchitecture.Tutorial.Domain.Validation;

namespace AMTutorialArchitecture.Tutorial.Domain.Commands
{
    public sealed class TutorialCommandProcessor
    {
        private readonly TutorialModel model;
        private readonly TutorialDefinitionValidator validator;

        public TutorialCommandProcessor(TutorialModel model, TutorialDefinitionValidator validator)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public TutorialStartResult Execute(StartTutorialCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var validation = validator.Validate(command.Sequence);
            if (!validation.IsValid)
            {
                return new TutorialStartResult(
                    TutorialStartStatus.InvalidDefinition,
                    model.Snapshot,
                    validation.Issues);
            }

            var current = model.Snapshot;
            if (current.IsActive)
            {
                var status = current.Sequence.Id == command.Sequence.Id
                    ? TutorialStartStatus.AlreadyActive
                    : TutorialStartStatus.RejectedAnotherSequenceIsActive;
                return new TutorialStartResult(status, current);
            }

            if (model.IsCompleted(command.Sequence.Id))
            {
                return new TutorialStartResult(TutorialStartStatus.AlreadyCompleted, current);
            }

            model.Start(command.Sequence);
            return new TutorialStartResult(
                TutorialStartStatus.Started,
                model.Snapshot,
                validation.Issues);
        }

        public TutorialActionResult Execute(SubmitTutorialActionCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var current = model.Snapshot;
            if (!current.IsActive)
            {
                return new TutorialActionResult(
                    TutorialActionStatus.IgnoredNoActiveSequence,
                    current);
            }

            var requiredAction = current.CurrentEntry.RequiredAction;
            if (!command.Occurrence.Succeeded
                && requiredAction.ExpectedActionId == command.Occurrence.ActionId)
            {
                return new TutorialActionResult(TutorialActionStatus.IgnoredFailedAction, current);
            }

            if (!requiredAction.IsSatisfiedBy(command.Occurrence))
            {
                return new TutorialActionResult(
                    TutorialActionStatus.IgnoredNotRequiredAction,
                    current);
            }

            if (model.IsLastEntry())
            {
                model.CompleteActiveSequence();
                return new TutorialActionResult(TutorialActionStatus.Completed, model.Snapshot);
            }

            model.Advance();
            return new TutorialActionResult(TutorialActionStatus.Advanced, model.Snapshot);
        }

        public TutorialSkipResult Execute(SkipTutorialCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var current = model.Snapshot;
            if (!current.IsActive)
            {
                return new TutorialSkipResult(TutorialSkipStatus.RejectedNoActiveSequence, current);
            }

            if (!current.CanSkip)
            {
                return new TutorialSkipResult(TutorialSkipStatus.RejectedNotSkippable, current);
            }

            model.CompleteActiveSequence();
            return new TutorialSkipResult(TutorialSkipStatus.Skipped, model.Snapshot);
        }
    }
}
