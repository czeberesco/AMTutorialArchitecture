using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Integration
{
    public sealed class TutorialActionEmitter : MonoBehaviour
    {
        [SerializeField] private string actionId;
        [SerializeField] private Button button;

        private ITutorialService tutorialService;

        [Inject]
        public void Construct(ITutorialService service)
        {
            tutorialService = service;
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(ReportSuccess);
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ReportSuccess);
            }
        }

        public void ReportSuccess()
        {
            if (tutorialService == null || string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            tutorialService.SubmitAction(TutorialActionOccurrence.Success(actionId));
        }

        public void ReportFailure()
        {
            if (tutorialService == null || string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            tutorialService.SubmitAction(TutorialActionOccurrence.Failure(actionId));
        }
    }
}
