using AMTutorialArchitecture.Tutorial.Domain.Actions;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Required Action/Continue Dialogue", fileName = "TutorialContinueDialogue")]
    public sealed class ContinueDialogueRequiredActionAsset : RequiredActionAsset
    {
        public override IRequiredActionDefinition CreateDefinition()
        {
            return new ContinueDialogueRequiredActionDefinition();
        }
    }
}
