using System;
using System.Collections.Generic;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Targets
{
    public sealed class TutorialTargetRegistry : ITutorialTargetRegistry
    {
        private readonly Dictionary<string, TutorialTargetView> targets =
            new Dictionary<string, TutorialTargetView>(StringComparer.Ordinal);

        public event EventHandler<TutorialTargetRegistrationEventArgs> TargetRegistered;

        public event EventHandler<TutorialTargetRegistrationEventArgs> TargetUnregistered;

        public bool Register(TutorialTargetView target)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.TargetId))
            {
                return false;
            }

            if (targets.TryGetValue(target.TargetId, out var existing) && existing != target)
            {
                Debug.LogWarning(
                    $"[Tutorial] Target ID '{target.TargetId}' is already registered by '{existing.name}'. " +
                    $"Registration from '{target.name}' was rejected.",
                    target);
                return false;
            }

            targets[target.TargetId] = target;
            TargetRegistered?.Invoke(this, new TutorialTargetRegistrationEventArgs(target));
            return true;
        }

        public void Unregister(TutorialTargetView target)
        {
            if (target == null || string.IsNullOrWhiteSpace(target.TargetId))
            {
                return;
            }

            if (!targets.TryGetValue(target.TargetId, out var existing) || existing != target)
            {
                return;
            }

            targets.Remove(target.TargetId);
            TargetUnregistered?.Invoke(this, new TutorialTargetRegistrationEventArgs(target));
        }

        public bool TryGet(string targetId, out TutorialTargetView target)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                target = null;
                return false;
            }

            return targets.TryGetValue(targetId, out target) && target != null;
        }
    }
}
