using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Targets
{
    public sealed class GameObjectHighlightView : MonoBehaviour, ITutorialHighlightView
    {
        [SerializeField] private GameObject highlightRoot;

        private void Awake()
        {
            HideHighlight();
        }

        public void ShowHighlight()
        {
            if (highlightRoot != null)
            {
                highlightRoot.SetActive(true);
            }
        }

        public void HideHighlight()
        {
            if (highlightRoot != null && highlightRoot != gameObject)
            {
                highlightRoot.SetActive(false);
            }
        }
    }
}
