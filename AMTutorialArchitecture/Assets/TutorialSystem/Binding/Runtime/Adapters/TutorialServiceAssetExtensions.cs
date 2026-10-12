using System;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Commands;
using AMTutorialArchitecture.Tutorial.Unity.Authoring;

namespace AMTutorialArchitecture.Tutorial.Unity.Services
{
    public static class TutorialServiceAssetExtensions
    {
        public static TutorialStartResult Start(
            this ITutorialService service,
            TutorialSequenceAsset sequenceAsset)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (sequenceAsset == null)
            {
                return TutorialStartResult.Invalid(new[]
                {
                    Domain.Validation.TutorialValidationIssue.Error("Tutorial sequence asset is null.")
                });
            }

            sequenceAsset.TryCreateDefinition(out var definition, out var validation);
            if (!validation.IsValid)
            {
                return TutorialStartResult.Invalid(validation.Issues);
            }

            return service.Start(definition);
        }
    }
}
