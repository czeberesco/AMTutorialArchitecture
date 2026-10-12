using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Dialogue", fileName = "TutorialDialogue")]
    public sealed class DialogueAsset : ScriptableObject
    {
        [SerializeField] private string speakerId;
        [SerializeField, TextArea(2, 6)] private string text;
        [SerializeField] private TutorialDialogueSide side;

        public DialogueDefinition CreateDefinition()
        {
            return new DialogueDefinition(speakerId, text, side);
        }
    }
}
