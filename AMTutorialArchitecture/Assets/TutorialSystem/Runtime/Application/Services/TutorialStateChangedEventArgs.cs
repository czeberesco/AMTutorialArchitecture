using System;
using AMTutorialArchitecture.Tutorial.Domain.Model;

namespace AMTutorialArchitecture.Tutorial.Application.Services
{
    public enum TutorialStateChangeReason
    {
        Started,
        Advanced,
        Completed,
        Skipped
    }

    public sealed class TutorialStateChangedEventArgs : EventArgs
    {
        public TutorialStateChangedEventArgs(
            TutorialSnapshot previous,
            TutorialSnapshot current,
            TutorialStateChangeReason reason)
        {
            Previous = previous ?? TutorialSnapshot.Inactive;
            Current = current ?? TutorialSnapshot.Inactive;
            Reason = reason;
        }

        public TutorialSnapshot Previous { get; }

        public TutorialSnapshot Current { get; }

        public TutorialStateChangeReason Reason { get; }
    }
}
