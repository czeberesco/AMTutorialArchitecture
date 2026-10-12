using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        private readonly List<TutorialTargetView> boundTargets = new List<TutorialTargetView>();
        private readonly HashSet<string> warningKeys = new HashSet<string>();
        private bool subscribed;
        private string activeEntryKey;

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
            ClearTargets();
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
            if (viewModel.TargetIds.Contains(args.Target.TargetId))
            {
                Refresh();
            }
        }

        private void OnTargetUnregistered(object sender, TutorialTargetRegistrationEventArgs args)
        {
            if (boundTargets.Contains(args.Target))
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (!isActiveAndEnabled || viewModel == null || registry == null)
            {
                return;
            }

            ClearTargets();
            var entryKey = $"{viewModel.SequenceId}:{viewModel.EntryId}";
            if (activeEntryKey != entryKey)
            {
                activeEntryKey = entryKey;
                warningKeys.Clear();
            }

            if (!viewModel.HasIndication)
            {
                return;
            }

            foreach (var targetId in viewModel.TargetIds)
            {
                if (string.IsNullOrWhiteSpace(targetId))
                {
                    continue;
                }

                if (!registry.TryGet(targetId, out var target))
                {
                    if (viewModel.InteractionPolicy == TutorialInteractionPolicy.Blocking)
                    {
                        WarnOnce(targetId, "missing-target",
                            $"Blocking target '{targetId}' is not registered. Available targets remain usable.");
                    }

                    continue;
                }

                boundTargets.Add(target);
                if (!target.TryShowHighlight())
                {
                    WarnOnce(targetId, "missing-highlight",
                        $"Target '{targetId}' resolved to '{target.name}' but has no " +
                        $"{nameof(ITutorialHighlightView)} implementation.");
                }
            }
        }

        private void ClearTargets()
        {
            foreach (var target in boundTargets)
            {
                if (target != null)
                {
                    target.HideHighlight();
                }
            }

            boundTargets.Clear();
        }

        private void WarnOnce(string targetId, string reason, string message)
        {
            var key = $"{viewModel.SequenceId}:{viewModel.EntryId}:{targetId}:{reason}";
            if (!warningKeys.Add(key))
            {
                return;
            }

            Debug.LogWarning(
                $"[Tutorial] {message} Sequence='{viewModel.SequenceId}', Entry='{viewModel.EntryId}'.",
                this);
        }
    }
}
