using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Domain.Actions;

namespace AMTutorialArchitecture.Tutorial.Domain.Definitions
{
    public sealed class TutorialEntryDefinition
    {
        private readonly ReadOnlyCollection<IndicationDefinition> indications;

        public TutorialEntryDefinition(
            string id,
            DialogueDefinition dialogue,
            IEnumerable<IndicationDefinition> indications,
            TutorialInteractionPolicy interactionPolicy,
            IRequiredActionDefinition requiredAction)
        {
            Id = id ?? string.Empty;
            Dialogue = dialogue;
            this.indications = new ReadOnlyCollection<IndicationDefinition>(
                (indications ?? Enumerable.Empty<IndicationDefinition>()).ToList());
            InteractionPolicy = interactionPolicy;
            RequiredAction = requiredAction;
        }

        public string Id { get; }

        public DialogueDefinition Dialogue { get; }

        public IReadOnlyList<IndicationDefinition> Indications => indications;

        public TutorialInteractionPolicy InteractionPolicy { get; }

        public IRequiredActionDefinition RequiredAction { get; }

        public bool HasDialogue => Dialogue != null;

        public bool HasIndication => indications.Count > 0;
    }
}
