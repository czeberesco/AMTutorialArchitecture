using AMTutorialArchitecture.Tutorial.Domain.Actions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Required Action/Semantic Action", fileName = "TutorialSemanticAction")]
    public sealed class SemanticActionRequiredActionAsset : RequiredActionAsset
    {
        [SerializeField, TextArea] private string description;

        public override IRequiredActionDefinition CreateDefinition()
        {
            return new SemanticActionRequiredActionDefinition(Id, description);
        }
    }
}
