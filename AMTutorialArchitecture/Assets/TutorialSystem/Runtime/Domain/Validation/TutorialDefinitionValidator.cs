using System.Collections.Generic;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;

namespace AMTutorialArchitecture.Tutorial.Domain.Validation
{
    public sealed class TutorialDefinitionValidator
    {
        public TutorialValidationResult Validate(TutorialSequenceDefinition sequence)
        {
            var issues = new List<TutorialValidationIssue>();

            if (sequence == null)
            {
                issues.Add(TutorialValidationIssue.Error("Tutorial sequence is null."));
                return new TutorialValidationResult(issues);
            }

            if (string.IsNullOrWhiteSpace(sequence.Id))
            {
                issues.Add(TutorialValidationIssue.Error("Tutorial sequence ID is required."));
            }

            if (sequence.Entries.Count == 0)
            {
                issues.Add(TutorialValidationIssue.Error($"Tutorial sequence '{sequence.Id}' has no entries."));
            }

            if (sequence.CanSkip && sequence.Entries.Count <= 1)
            {
                issues.Add(TutorialValidationIssue.Warning(
                    $"Tutorial sequence '{sequence.Id}' enables skipping but contains at most one entry."));
            }

            var entryIds = new HashSet<string>();
            for (var index = 0; index < sequence.Entries.Count; index++)
            {
                ValidateEntry(sequence, sequence.Entries[index], index, entryIds, issues);
            }

            return new TutorialValidationResult(issues);
        }

        private static void ValidateEntry(
            TutorialSequenceDefinition sequence,
            TutorialEntryDefinition entry,
            int index,
            ISet<string> entryIds,
            ICollection<TutorialValidationIssue> issues)
        {
            var location = $"Sequence '{sequence.Id}', entry {index}";
            if (entry == null)
            {
                issues.Add(TutorialValidationIssue.Error($"{location} is null."));
                return;
            }

            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                issues.Add(TutorialValidationIssue.Error($"{location} has no ID."));
            }
            else if (!entryIds.Add(entry.Id))
            {
                issues.Add(TutorialValidationIssue.Error(
                    $"Sequence '{sequence.Id}' contains duplicate entry ID '{entry.Id}'."));
            }

            if (!entry.HasDialogue && !entry.HasIndication)
            {
                issues.Add(TutorialValidationIssue.Error(
                    $"{location} ('{entry.Id}') needs dialogue, indication, or both."));
            }

            if (entry.RequiredAction == null)
            {
                issues.Add(TutorialValidationIssue.Error(
                    $"{location} ('{entry.Id}') has no required action."));
            }
            else if (string.IsNullOrWhiteSpace(entry.RequiredAction.ExpectedActionId))
            {
                issues.Add(TutorialValidationIssue.Error(
                    $"{location} ('{entry.Id}') has a required action with no action ID."));
            }

            if (entry.HasDialogue && string.IsNullOrWhiteSpace(entry.Dialogue.Text))
            {
                issues.Add(TutorialValidationIssue.Warning(
                    $"{location} ('{entry.Id}') contains empty dialogue text."));
            }

            if (entry.HasIndication && string.IsNullOrWhiteSpace(entry.Indication.TargetId))
            {
                issues.Add(TutorialValidationIssue.Error(
                    $"{location} ('{entry.Id}') has an indication with no target ID."));
            }

            if (entry.HasIndication
                && entry.RequiredAction is ContinueDialogueRequiredActionDefinition)
            {
                issues.Add(TutorialValidationIssue.Error(
                    $"{location} ('{entry.Id}') has an indication but completes by continuing dialogue."));
            }
        }
    }
}
