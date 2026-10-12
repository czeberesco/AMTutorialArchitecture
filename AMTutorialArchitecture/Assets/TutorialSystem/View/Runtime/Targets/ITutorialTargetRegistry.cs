using System;

namespace AMTutorialArchitecture.Tutorial.Unity.Targets
{
    public interface ITutorialTargetRegistry
    {
        event EventHandler<TutorialTargetRegistrationEventArgs> TargetRegistered;

        event EventHandler<TutorialTargetRegistrationEventArgs> TargetUnregistered;

        bool Register(TutorialTargetView target);

        void Unregister(TutorialTargetView target);

        bool TryGet(string targetId, out TutorialTargetView target);
    }

    public sealed class TutorialTargetRegistrationEventArgs : EventArgs
    {
        public TutorialTargetRegistrationEventArgs(TutorialTargetView target)
        {
            Target = target;
        }

        public TutorialTargetView Target { get; }
    }
}
