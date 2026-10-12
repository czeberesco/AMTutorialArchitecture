using System;
using System.Collections.Generic;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Domain.Validation;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Unity.Authoring
{
    [CreateAssetMenu(menuName = "Tutorial/Sequence", fileName = "TutorialSequence")]
    public sealed class TutorialSequenceAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private bool canSkip;
        [SerializeField] private List<TutorialEntryAsset> entries = new List<TutorialEntryAsset>();

        public string Id => id ?? string.Empty;

        public bool CanSkip => canSkip;

        public IReadOnlyList<TutorialEntryAsset> Entries => entries;

        public bool TryCreateDefinition(
            out TutorialSequenceDefinition definition,
            out TutorialValidationResult validation)
        {
            var issues = new List<TutorialValidationIssue>();
            var entryDefinitions = new List<TutorialEntryDefinition>();

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    issues.Add(TutorialValidationIssue.Error(
                        $"Sequence asset '{name}' contains a null entry at index {index}."));
                    entryDefinitions.Add(null);
                    continue;
                }

                entryDefinitions.Add(entry.CreateDefinition());
            }

            definition = new TutorialSequenceDefinition(Id, canSkip, entryDefinitions);
            var domainValidation = new TutorialDefinitionValidator().Validate(definition);
            issues.AddRange(domainValidation.Issues);
            validation = new TutorialValidationResult(issues);
            return validation.IsValid;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N");
            }
        }
    }
}
