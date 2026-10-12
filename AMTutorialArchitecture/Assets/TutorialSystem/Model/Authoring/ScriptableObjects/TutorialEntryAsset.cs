using System;
using System.Collections.Generic;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Entry", fileName = "TutorialEntry")]
    public sealed class TutorialEntryAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private DialogueAsset dialogue;
        [SerializeField] private List<IndicationAsset> indications = new List<IndicationAsset>();
        [SerializeField] private TutorialInteractionPolicy interactionPolicy;
        [SerializeField] private RequiredActionAsset requiredAction;

        public string Id => id ?? string.Empty;

        public DialogueAsset Dialogue => dialogue;

        public IReadOnlyList<IndicationAsset> Indications => indications;

        public TutorialInteractionPolicy InteractionPolicy => interactionPolicy;

        public RequiredActionAsset RequiredAction => requiredAction;

        public TutorialEntryDefinition CreateDefinition()
        {
            return new TutorialEntryDefinition(
                Id,
                dialogue != null ? dialogue.CreateDefinition() : null,
                indications.Select(indication => indication != null ? indication.CreateDefinition() : null),
                interactionPolicy,
                requiredAction != null ? requiredAction.CreateDefinition() : null);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
        }
    }
}
