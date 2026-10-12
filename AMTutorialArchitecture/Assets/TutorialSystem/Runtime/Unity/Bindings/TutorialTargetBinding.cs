using System.ComponentModel;
using AMTutorialArchitecture.Tutorial.Application.ViewModels;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Unity.Targets;
using UnityEngine;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Bindings
{
    public sealed class TutorialTargetBinding : MonoBehaviour
    {
        private TutorialViewModel viewModel;
        private ITutorialTargetRegistry registry;
        private TutorialTargetView boundTarget;
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
            ClearTarget();
        }

        private void SubscribeIfPossible()
        {
            if (subscribed || viewModel == null || registry == null || !isActiveAndEnabled)
            {
                return;
            }

            viewModel.PropertyChanged += OnViewModelChanged;
            registry.TargetRegistered += OnTargetRegistered;
            registry.TargetUnregistered += OnTargetUnregistered;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            viewModel.PropertyChanged -= OnViewModelChanged;
            registry.TargetRegistered -= OnTargetRegistered;
            registry.TargetUnregistered -= OnTargetUnregistered;
            subscribed = false;
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs args)
        {
            Refresh();
        }

        private void OnTargetRegistered(object sender, TutorialTargetRegistrationEventArgs args)
        {
            if (args.Target.TargetId == viewModel.TargetId)
            {
                Refresh();
            }
        }

        private void OnTargetUnregistered(object sender, TutorialTargetRegistrationEventArgs args)
        {
            if (boundTarget == args.Target)
            {
                ClearTarget();
            }
        }

        private void Refresh()
        {
            if (!isActiveAndEnabled || viewModel == null || registry == null)
            {
                return;
            }

            ClearTarget();
            if (!viewModel.HasIndication || string.IsNullOrWhiteSpace(viewModel.TargetId))
            {
                lastWarningKey = null;
                return;
            }

            if (!registry.TryGet(viewModel.TargetId, out var target))
            {
                if (viewModel.InteractionPolicy == TutorialInteractionPolicy.Blocking)
                {
                    WarnOnce("missing-target",
                        $"Blocking target '{viewModel.TargetId}' is not registered. Input remains unblocked to avoid a soft lock.");
                }

                return;
            }

            boundTarget = target;
            if (!boundTarget.TryShowHighlight())
            {
                WarnOnce("missing-highlight",
                    $"Target '{viewModel.TargetId}' resolved to '{boundTarget.name}' but has no " +
                    $"{nameof(ITutorialHighlightView)} implementation.");
            }
            else
            {
                lastWarningKey = null;
            }
        }

        private void ClearTarget()
        {
            if (boundTarget != null)
            {
                boundTarget.HideHighlight();
                boundTarget = null;
            }
        }

        private void WarnOnce(string reason, string message)
        {
            var key = $"{viewModel.SequenceId}:{viewModel.EntryId}:{viewModel.TargetId}:{reason}";
            if (lastWarningKey == key)
            {
                return;
            }

            lastWarningKey = key;
            Debug.LogWarning(
                $"[Tutorial] {message} Sequence='{viewModel.SequenceId}', Entry='{viewModel.EntryId}'.",
                this);
        }
    }
}
