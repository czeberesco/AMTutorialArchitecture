using AMTutorialArchitecture.Tutorial.Domain.Definitions;

namespace AMTutorialArchitecture.Tutorial.Domain.Model
{
    public sealed class TutorialSnapshot
    {
        private TutorialSnapshot()
        {
            IsActive = false;
            CurrentEntryIndex = -1;
        }

        internal TutorialSnapshot(TutorialSequenceDefinition sequence, int currentEntryIndex)
        {
            IsActive = true;
            Sequence = sequence;
            CurrentEntryIndex = currentEntryIndex;
            CurrentEntry = sequence.Entries[currentEntryIndex];
        }

        public static TutorialSnapshot Inactive { get; } = new TutorialSnapshot();

        public bool IsActive { get; }

        public TutorialSequenceDefinition Sequence { get; }

        public TutorialEntryDefinition CurrentEntry { get; }

        public int CurrentEntryIndex { get; }

        public int EntryCount => Sequence?.Entries.Count ?? 0;

        public bool CanSkip => IsActive && Sequence.CanSkip && EntryCount > 1;
    }
}
