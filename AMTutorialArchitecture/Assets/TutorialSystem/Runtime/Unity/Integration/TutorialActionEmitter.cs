using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Integration
{
    public sealed class TutorialActionEmitter : MonoBehaviour
    {
        [SerializeField] private RequiredActionAsset action;
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
            if (tutorialService == null || action == null || string.IsNullOrWhiteSpace(action.Id))
            {
                return;
            }

            tutorialService.SubmitAction(TutorialActionOccurrence.Success(action.Id));
        }

        public void ReportFailure()
        {
            if (tutorialService == null || action == null || string.IsNullOrWhiteSpace(action.Id))
            {
                return;
            }

            tutorialService.SubmitAction(TutorialActionOccurrence.Failure(action.Id));
        }
    }
}
