using UnityEngine;
using UnityEngine.UI;

namespace AMTutorialArchitecture.Tutorial.Example
{
    public sealed class ExampleActionView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text statusText;
        [SerializeField] private string successMessage;

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(Execute);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Execute);
            }
        }

        public void Execute()
        {
            if (statusText != null)
            {
                statusText.text = successMessage;
            }
        }
    }
}
