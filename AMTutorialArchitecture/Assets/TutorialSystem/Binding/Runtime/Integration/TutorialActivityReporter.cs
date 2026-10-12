using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Integration
{
    public sealed class TutorialActivityReporter : MonoBehaviour
    {
        [SerializeField] private Button button;

        private ITutorialActivityService activityService;

        [Inject]
        public void Construct(ITutorialActivityService service)
        {
            activityService = service;
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(ReportActivity);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ReportActivity);
            }
        }

        public void ReportActivity()
        {
            activityService?.ReportActivity();
        }
    }
}
