using System;
using AMTutorialArchitecture.Tutorial.Domain.Actions;

namespace AMTutorialArchitecture.Tutorial.Domain.Definitions
{
    public sealed class TutorialEntryDefinition
    {
        public TutorialEntryDefinition(
            string id,
            DialogueDefinition dialogue,
            IndicationDefinition indication,
            IRequiredActionDefinition requiredAction)
        {
            Id = id ?? string.Empty;
            Dialogue = dialogue;
            Indication = indication;
            RequiredAction = requiredAction;
        }

        public string Id { get; }

        public DialogueDefinition Dialogue { get; }

        public IndicationDefinition Indication { get; }

        public IRequiredActionDefinition RequiredAction { get; }

        public bool HasDialogue => Dialogue != null;

        public bool HasIndication => Indication != null;
    }
}
