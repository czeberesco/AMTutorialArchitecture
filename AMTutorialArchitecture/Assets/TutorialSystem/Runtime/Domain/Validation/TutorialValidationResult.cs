using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AMTutorialArchitecture.Tutorial.Domain.Validation
{
    public sealed class TutorialValidationResult
    {
        private readonly ReadOnlyCollection<TutorialValidationIssue> issues;

        public TutorialValidationResult(IEnumerable<TutorialValidationIssue> issues)
        {
            this.issues = new ReadOnlyCollection<TutorialValidationIssue>(
                (issues ?? Enumerable.Empty<TutorialValidationIssue>()).ToList());
        }

        public IReadOnlyList<TutorialValidationIssue> Issues => issues;

        public bool IsValid => issues.All(issue => issue.Severity != TutorialValidationSeverity.Error);
    }
}
