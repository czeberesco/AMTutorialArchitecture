using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Indication", fileName = "TutorialIndication")]
    public sealed class IndicationAsset : ScriptableObject
    {
        [SerializeField] private TutorialTargetAsset target;
        [SerializeField] private TutorialInteractionPolicy interactionPolicy;

        public TutorialTargetAsset Target => target;

        public IndicationDefinition CreateDefinition()
        {
            return new IndicationDefinition(target != null ? target.Id : string.Empty, interactionPolicy);
        }
    }
}
