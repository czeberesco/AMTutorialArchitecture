using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Domain.Model;
using AMTutorialArchitecture.Tutorial.Domain.Validation;

namespace AMTutorialArchitecture.Tutorial.Domain.Commands
{
    public enum TutorialStartStatus
    {
        Started,
        AlreadyActive,
        RejectedAnotherSequenceIsActive,
        AlreadyCompleted,
        InvalidDefinition
    }

    public sealed class TutorialStartResult
    {
        private readonly ReadOnlyCollection<TutorialValidationIssue> issues;

        internal TutorialStartResult(
            TutorialStartStatus status,
            TutorialSnapshot snapshot,
            IEnumerable<TutorialValidationIssue> issues = null)
        {
            Status = status;
            Snapshot = snapshot ?? TutorialSnapshot.Inactive;
            this.issues = new ReadOnlyCollection<TutorialValidationIssue>(
                (issues ?? Enumerable.Empty<TutorialValidationIssue>()).ToList());
        }

        public TutorialStartStatus Status { get; }

        public TutorialSnapshot Snapshot { get; }

        public IReadOnlyList<TutorialValidationIssue> Issues => issues;

        public bool Started => Status == TutorialStartStatus.Started;

        public static TutorialStartResult Invalid(IEnumerable<TutorialValidationIssue> issues)
        {
            return new TutorialStartResult(
                TutorialStartStatus.InvalidDefinition,
                TutorialSnapshot.Inactive,
                issues);
        }
    }

    public enum TutorialActionStatus
    {
        Advanced,
        Completed,
        IgnoredNoActiveSequence,
        IgnoredNotRequiredAction,
        IgnoredFailedAction
    }

    public sealed class TutorialActionResult
    {
        internal TutorialActionResult(TutorialActionStatus status, TutorialSnapshot snapshot)
        {
            Status = status;
            Snapshot = snapshot ?? TutorialSnapshot.Inactive;
        }

        public TutorialActionStatus Status { get; }

        public TutorialSnapshot Snapshot { get; }

        public bool Changed => Status == TutorialActionStatus.Advanced || Status == TutorialActionStatus.Completed;
    }

    public enum TutorialSkipStatus
    {
        Skipped,
        RejectedNoActiveSequence,
        RejectedNotSkippable
    }

    public sealed class TutorialSkipResult
    {
        internal TutorialSkipResult(TutorialSkipStatus status, TutorialSnapshot snapshot)
        {
            Status = status;
            Snapshot = snapshot ?? TutorialSnapshot.Inactive;
        }

        public TutorialSkipStatus Status { get; }

        public TutorialSnapshot Snapshot { get; }

        public bool Changed => Status == TutorialSkipStatus.Skipped;
    }
}
