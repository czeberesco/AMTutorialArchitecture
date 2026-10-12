using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AMTutorialArchitecture.Tutorial.Domain.Definitions
{
    public sealed class TutorialSequenceDefinition
    {
        private readonly ReadOnlyCollection<TutorialEntryDefinition> entries;

        public TutorialSequenceDefinition(string id, bool canSkip, IEnumerable<TutorialEntryDefinition> entries)
        {
            Id = id ?? string.Empty;
            CanSkip = canSkip;
            this.entries = new ReadOnlyCollection<TutorialEntryDefinition>(
                (entries ?? Enumerable.Empty<TutorialEntryDefinition>()).ToList());
        }

        public string Id { get; }

        public bool CanSkip { get; }

        public IReadOnlyList<TutorialEntryDefinition> Entries => entries;
    }
}
