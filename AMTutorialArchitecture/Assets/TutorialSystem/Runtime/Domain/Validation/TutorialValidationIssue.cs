namespace AMTutorialArchitecture.Tutorial.Domain.Validation
{
    public enum TutorialValidationSeverity
    {
        Warning = 0,
        Error = 1
    }

    public sealed class TutorialValidationIssue
    {
        public TutorialValidationIssue(TutorialValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message ?? string.Empty;
        }

        public TutorialValidationSeverity Severity { get; }

        public string Message { get; }

        public static TutorialValidationIssue Error(string message)
        {
            return new TutorialValidationIssue(TutorialValidationSeverity.Error, message);
        }

        public static TutorialValidationIssue Warning(string message)
        {
            return new TutorialValidationIssue(TutorialValidationSeverity.Warning, message);
        }
    }
}
