using System;

namespace AMTutorialArchitecture.Tutorial.Unity.Integration
{
    public interface ITutorialActivityService
    {
        event EventHandler ActivityReported;

        void ReportActivity();
    }

    public sealed class TutorialActivityService : ITutorialActivityService
    {
        public event EventHandler ActivityReported;

        public void ReportActivity()
        {
            ActivityReported?.Invoke(this, EventArgs.Empty);
        }
    }
}
