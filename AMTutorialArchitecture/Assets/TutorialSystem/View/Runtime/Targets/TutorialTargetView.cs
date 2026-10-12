using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Targets
{
    public sealed class TutorialTargetView : MonoBehaviour
    {
        [SerializeField] private TutorialTargetAsset target;
        [SerializeField] private MonoBehaviour highlightView;
        [SerializeField] private Selectable selectable;

        private ITutorialTargetRegistry registry;
        private bool registered;

        public string TargetId => target != null ? target.Id : string.Empty;

        public Selectable Selectable => selectable;

        [Inject]
        public void Construct(ITutorialTargetRegistry targetRegistry)
        {
            registry = targetRegistry;
            RegisterIfPossible();
        }

        public bool TryShowHighlight()
        {
            if (!(highlightView is ITutorialHighlightView highlight))
            {
                return false;
            }

            highlight.ShowHighlight();
            return true;
        }

        public void HideHighlight()
        {
            if (highlightView is ITutorialHighlightView highlight)
            {
                highlight.HideHighlight();
            }
        }

        private void OnEnable()
        {
            RegisterIfPossible();
        }

        private void OnDisable()
        {
            HideHighlight();
            Unregister();
        }

        private void OnValidate()
        {
            if (selectable == null)
            {
                selectable = GetComponent<Selectable>();
            }

            if (highlightView != null && !(highlightView is ITutorialHighlightView))
            {
                Debug.LogWarning(
                    $"[Tutorial] Highlight component on '{name}' must implement {nameof(ITutorialHighlightView)}.",
                    this);
            }
        }

        private void RegisterIfPossible()
        {
            if (registered || !isActiveAndEnabled || registry == null)
            {
                return;
            }

            if (target == null || string.IsNullOrWhiteSpace(TargetId))
            {
                Debug.LogWarning($"[Tutorial] Target view '{name}' has no valid target asset.", this);
                return;
            }

            registered = registry.Register(this);
        }

        private void Unregister()
        {
            if (!registered || registry == null)
            {
                return;
            }

            registry.Unregister(this);
            registered = false;
        }
    }
}
