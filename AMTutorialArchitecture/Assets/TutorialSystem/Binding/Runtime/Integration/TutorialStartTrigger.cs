using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using AMTutorialArchitecture.Tutorial.Unity.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Integration
{
    public sealed class TutorialStartTrigger : MonoBehaviour
    {
        [SerializeField] private TutorialSequenceAsset sequence;
        [SerializeField] private Button button;
        [SerializeField] private bool startAutomatically;

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
                button.onClick.AddListener(Begin);
            }
        }

        private void Start()
        {
            if (startAutomatically)
            {
                Begin();
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(Begin);
            }
        }

        public void Begin()
        {
            if (tutorialService == null)
            {
                Debug.LogError("[Tutorial] Start trigger was not injected.", this);
                return;
            }

            var result = tutorialService.Start(sequence);
            if (!result.Started)
            {
                Debug.Log($"[Tutorial] Sequence start result: {result.Status}.", this);
            }
        }
    }
}
