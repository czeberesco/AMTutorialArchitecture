using System;
using System.Collections.Generic;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using AMTutorialArchitecture.Tutorial.Unity.Integration;
using AMTutorialArchitecture.Tutorial.Unity.Targets;
using UnityEngine;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Bindings
{
    public sealed class DelayedTutorialHintBinding : MonoBehaviour
    {
        [SerializeField] private DelayedTutorialHintAsset hint;
        [SerializeField] private bool armAfterSequenceFinishes = true;

        private readonly List<TutorialTargetView> highlightedTargets = new List<TutorialTargetView>();
        private readonly HashSet<string> warningKeys = new HashSet<string>();
        private ITutorialService tutorialService;
        private ITutorialActivityService activityService;
        private ITutorialTargetRegistry targetRegistry;
        private float inactiveSeconds;
        private bool armed;
        private bool visible;
        private bool subscribed;

        public bool IsVisible => visible;

        [Inject]
        public void Construct(
            ITutorialService service,
            ITutorialActivityService playerActivityService,
            ITutorialTargetRegistry registry)
        {
            tutorialService = service;
            activityService = playerActivityService;
            targetRegistry = registry;
            armed = !armAfterSequenceFinishes && !tutorialService.Current.IsActive;
            SubscribeIfPossible();
        }

        private void OnEnable()
        {
            SubscribeIfPossible();
        }

        private void OnDisable()
        {
            Unsubscribe();
            HideHint();
        }

        private void Update()
        {
            if (!armed || visible || hint == null || tutorialService == null || tutorialService.Current.IsActive)
            {
                return;
            }

            inactiveSeconds += Time.unscaledDeltaTime;
            if (inactiveSeconds >= Mathf.Max(0f, hint.InactivitySeconds))
            {
                ShowHint();
            }
        }

        private void SubscribeIfPossible()
        {
            if (subscribed
                || tutorialService == null
                || activityService == null
                || targetRegistry == null
                || !isActiveAndEnabled)
            {
                return;
            }

            tutorialService.StateChanged += OnTutorialStateChanged;
            activityService.ActivityReported += OnActivityReported;
            targetRegistry.TargetRegistered += OnTargetRegistered;
            targetRegistry.TargetUnregistered += OnTargetUnregistered;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            tutorialService.StateChanged -= OnTutorialStateChanged;
            activityService.ActivityReported -= OnActivityReported;
            targetRegistry.TargetRegistered -= OnTargetRegistered;
            targetRegistry.TargetUnregistered -= OnTargetUnregistered;
            subscribed = false;
        }

        private void OnTutorialStateChanged(object sender, TutorialStateChangedEventArgs args)
        {
            HideHint();
            inactiveSeconds = 0f;

            if (args.Current.IsActive)
            {
                armed = false;
                return;
            }

            if (!armAfterSequenceFinishes
                || args.Reason == TutorialStateChangeReason.Completed
                || args.Reason == TutorialStateChangeReason.Skipped)
            {
                armed = true;
            }
        }

        private void OnActivityReported(object sender, EventArgs args)
        {
            HideHint();
            inactiveSeconds = 0f;
        }

        private void OnTargetRegistered(object sender, TutorialTargetRegistrationEventArgs args)
        {
            if (visible && GetTargetIds().Contains(args.Target.TargetId))
            {
                RefreshHighlights();
            }
        }

        private void OnTargetUnregistered(object sender, TutorialTargetRegistrationEventArgs args)
        {
            if (highlightedTargets.Contains(args.Target))
            {
                RefreshHighlights();
            }
        }

        private void ShowHint()
        {
            visible = true;
            RefreshHighlights();
        }

        private void RefreshHighlights()
        {
            ClearHighlights();
            foreach (var targetId in GetTargetIds())
            {
                if (!targetRegistry.TryGet(targetId, out var target))
                {
                    WarnOnce(targetId, "missing-target",
                        $"Delayed hint target '{targetId}' is not registered.");
                    continue;
                }

                highlightedTargets.Add(target);
                if (!target.TryShowHighlight())
                {
                    WarnOnce(targetId, "missing-highlight",
                        $"Delayed hint target '{targetId}' has no {nameof(ITutorialHighlightView)} implementation.");
                }
            }
        }

        private IEnumerable<string> GetTargetIds()
        {
            return hint == null
                ? Enumerable.Empty<string>()
                : hint.Targets
                    .Where(target => target != null && !string.IsNullOrWhiteSpace(target.Id))
                    .Select(target => target.Id)
                    .Distinct(StringComparer.Ordinal);
        }

        private void HideHint()
        {
            ClearHighlights();
            visible = false;
        }

        private void ClearHighlights()
        {
            foreach (var target in highlightedTargets)
            {
                if (target != null)
                {
                    target.HideHighlight();
                }
            }

            highlightedTargets.Clear();
        }

        private void WarnOnce(string targetId, string reason, string message)
        {
            if (!warningKeys.Add($"{targetId}:{reason}"))
            {
                return;
            }

            Debug.LogWarning($"[Tutorial] {message}", this);
        }
    }
}
