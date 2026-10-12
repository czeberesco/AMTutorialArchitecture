using System.Collections.Generic;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Delayed Hint", fileName = "DelayedTutorialHint")]
    public sealed class DelayedTutorialHintAsset : ScriptableObject
    {
        [Min(0f)]
        [SerializeField] private float inactivitySeconds = 3f;
        [SerializeField] private List<TutorialTargetAsset> targets = new List<TutorialTargetAsset>();

        public float InactivitySeconds => inactivitySeconds;

        public IReadOnlyList<TutorialTargetAsset> Targets => targets;
    }
}
