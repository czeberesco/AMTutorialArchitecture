using System;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Entry", fileName = "TutorialEntry")]
    public sealed class TutorialEntryAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private DialogueAsset dialogue;
        [SerializeField] private IndicationAsset indication;
        [SerializeField] private RequiredActionAsset requiredAction;

        public string Id => id ?? string.Empty;

        public DialogueAsset Dialogue => dialogue;

        public IndicationAsset Indication => indication;

        public RequiredActionAsset RequiredAction => requiredAction;

        public TutorialEntryDefinition CreateDefinition()
        {
            return new TutorialEntryDefinition(
                Id,
                dialogue != null ? dialogue.CreateDefinition() : null,
                indication != null ? indication.CreateDefinition() : null,
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
