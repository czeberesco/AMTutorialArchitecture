using AMTutorialArchitecture.Tutorial.Domain.Actions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    public abstract class RequiredActionAsset : ScriptableObject
    {
        public abstract IRequiredActionDefinition CreateDefinition();
    }
}
