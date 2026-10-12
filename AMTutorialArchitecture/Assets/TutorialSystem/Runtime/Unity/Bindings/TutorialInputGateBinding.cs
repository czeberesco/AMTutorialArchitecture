using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Application.ViewModels;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Unity.Targets;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Bindings
{
    public sealed class TutorialInputGateBinding : MonoBehaviour
    {
        [SerializeField] private Transform interactionRoot;
        [SerializeField] private List<Selectable> alwaysAllowed = new List<Selectable>();

        private readonly Dictionary<Selectable, bool> previousStates = new Dictionary<Selectable, bool>();
        private TutorialViewModel viewModel;
        private ITutorialTargetRegistry registry;
        private bool subscribed;
        private string lastWarningKey;

        [Inject]
        public void Construct(TutorialViewModel tutorialViewModel, ITutorialTargetRegistry targetRegistry)
        {
            viewModel = tutorialViewModel;
            registry = targetRegistry;
            SubscribeIfPossible();
            Refresh();
        }

        private void OnEnable()
        {
            SubscribeIfPossible();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
            Restore();
        }

        private void SubscribeIfPossible()
        {
            if (subscribed || viewModel == null || registry == null || !isActiveAndEnabled)
            {
                return;
            }

            viewModel.PropertyChanged += OnChanged;
            registry.TargetRegistered += OnTargetChanged;
            registry.TargetUnregistered += OnTargetChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            viewModel.PropertyChanged -= OnChanged;
            registry.TargetRegistered -= OnTargetChanged;
            registry.TargetUnregistered -= OnTargetChanged;
            subscribed = false;
        }

        private void OnChanged(object sender, PropertyChangedEventArgs args)
        {
            Refresh();
        }

        private void OnTargetChanged(object sender, TutorialTargetRegistrationEventArgs args)
        {
            if (args.Target.TargetId == viewModel.TargetId)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            Restore();
            if (!isActiveAndEnabled
                || viewModel == null
                || registry == null
                || !viewModel.HasIndication
                || viewModel.InteractionPolicy != TutorialInteractionPolicy.Blocking)
            {
                return;
            }

            if (!registry.TryGet(viewModel.TargetId, out var target) || target.Selectable == null)
            {
                WarnOnce("Blocking input was requested, but the target has no registered Selectable. " +
                         "Input remains unblocked to avoid a soft lock.");
                return;
            }

            var root = interactionRoot != null ? interactionRoot : transform;
            foreach (var selectable in root.GetComponentsInChildren<Selectable>(true))
            {
                if (selectable == null || selectable == target.Selectable || alwaysAllowed.Contains(selectable))
                {
                    continue;
                }

                previousStates[selectable] = selectable.interactable;
                selectable.interactable = false;
            }

            lastWarningKey = null;
        }

        private void Restore()
        {
            foreach (var pair in previousStates)
            {
                if (pair.Key != null)
                {
                    pair.Key.interactable = pair.Value;
                }
            }

            previousStates.Clear();
        }

        private void WarnOnce(string message)
        {
            var key = $"{viewModel.SequenceId}:{viewModel.EntryId}:{viewModel.TargetId}";
            if (key == lastWarningKey)
            {
                return;
            }

            lastWarningKey = key;
            Debug.LogWarning(
                $"[Tutorial] {message} Sequence='{viewModel.SequenceId}', Entry='{viewModel.EntryId}', " +
                $"Target='{viewModel.TargetId}'.",
                this);
        }
    }
}
