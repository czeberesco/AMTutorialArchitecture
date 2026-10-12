using System;
using System.Collections.Generic;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;

namespace AMTutorialArchitecture.Tutorial.Domain.Model
{
    public sealed class TutorialModel
    {
        private readonly HashSet<string> completedSequenceIds = new HashSet<string>(StringComparer.Ordinal);
        private TutorialSequenceDefinition activeSequence;
        private int currentEntryIndex = -1;

        public TutorialSnapshot Snapshot => activeSequence == null
            ? TutorialSnapshot.Inactive
            : new TutorialSnapshot(activeSequence, currentEntryIndex);

        public bool IsCompleted(string sequenceId)
        {
            return !string.IsNullOrWhiteSpace(sequenceId) && completedSequenceIds.Contains(sequenceId);
        }

        internal void Start(TutorialSequenceDefinition sequence)
        {
            activeSequence = sequence;
            currentEntryIndex = 0;
        }

        internal bool IsLastEntry()
        {
            return activeSequence != null && currentEntryIndex >= activeSequence.Entries.Count - 1;
        }

        internal void Advance()
        {
            currentEntryIndex++;
        }

        internal void CompleteActiveSequence()
        {
            if (activeSequence != null)
            {
                completedSequenceIds.Add(activeSequence.Id);
            }

            activeSequence = null;
            currentEntryIndex = -1;
        }
    }
}
