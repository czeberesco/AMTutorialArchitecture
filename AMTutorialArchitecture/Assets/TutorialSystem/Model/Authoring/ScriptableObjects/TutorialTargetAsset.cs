using System;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Target", fileName = "TutorialTarget")]
    public sealed class TutorialTargetAsset : ScriptableObject
    {
        [SerializeField] private string id;

        public string Id => id ?? string.Empty;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
        }
    }
}
