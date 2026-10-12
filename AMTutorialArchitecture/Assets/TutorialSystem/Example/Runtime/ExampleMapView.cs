using UnityEngine;
using UnityEngine.UI;

namespace AMTutorialArchitecture.Tutorial.Example
{
    public sealed class ExampleMapView : MonoBehaviour
    {
        [SerializeField] private Button openMapButton;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private Text statusText;

        private void OnEnable()
        {
            if (openMapButton != null)
            {
                openMapButton.onClick.AddListener(OpenMap);
            }
        }

        private void OnDisable()
        {
            if (openMapButton != null)
            {
                openMapButton.onClick.RemoveListener(OpenMap);
            }
        }

        public void OpenMap()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(true);
            }

            if (statusText != null)
            {
                statusText.text = "Map opened: the normal feature action still executed.";
            }
        }
    }
}
