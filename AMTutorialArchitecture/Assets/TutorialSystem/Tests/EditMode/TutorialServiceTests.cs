using System.Collections.Generic;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Application.ViewModels;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Commands;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Domain.Model;
using AMTutorialArchitecture.Tutorial.Domain.Validation;
using NUnit.Framework;

namespace AMTutorialArchitecture.Tutorial.Tests
{
    public sealed class TutorialServiceTests
    {
        private const string OpenMapAction = "game.map.open";
        private ITutorialService service;

        [SetUp]
        public void SetUp()
        {
            var model = new TutorialModel();
            var processor = new TutorialCommandProcessor(model, new TutorialDefinitionValidator());
            service = new TutorialService(processor, model);
        }

        [Test]
        public void Start_PublishesFirstEntry_AndExpectedActionsAdvanceToCompletion()
        {
            using (var viewModel = new TutorialViewModel(service))
            {
                var result = service.Start(CreateTwoEntrySequence());

                Assert.That(result.Status, Is.EqualTo(TutorialStartStatus.Started));
                Assert.That(viewModel.IsActive, Is.True);
                Assert.That(viewModel.EntryId, Is.EqualTo("explain-map"));
                Assert.That(viewModel.HasDialogue, Is.True);
                Assert.That(viewModel.HasIndication, Is.False);
                Assert.That(viewModel.CanContinueDialogue, Is.True);

                viewModel.ContinueCommand.Execute();

                Assert.That(viewModel.EntryId, Is.EqualTo("open-map"));
                Assert.That(viewModel.HasDialogue, Is.True);
                Assert.That(viewModel.HasIndication, Is.True);
                Assert.That(viewModel.CanContinueDialogue, Is.False);
                Assert.That(viewModel.TargetId, Is.EqualTo("hud.map-button"));
                Assert.That(viewModel.InteractionPolicy, Is.EqualTo(TutorialInteractionPolicy.Blocking));

                var completion = service.SubmitAction(TutorialActionOccurrence.Success(OpenMapAction));

                Assert.That(completion.Status, Is.EqualTo(TutorialActionStatus.Completed));
                Assert.That(viewModel.IsActive, Is.False);
            }
        }

        [Test]
        public void FailedOrUnrelatedAction_DoesNotAdvanceEntry()
        {
            service.Start(CreateIndicationOnlySequence());

            var failure = service.SubmitAction(TutorialActionOccurrence.Failure(OpenMapAction));
            var unrelated = service.SubmitAction(TutorialActionOccurrence.Success("game.inventory.open"));

            Assert.That(failure.Status, Is.EqualTo(TutorialActionStatus.IgnoredFailedAction));
            Assert.That(unrelated.Status, Is.EqualTo(TutorialActionStatus.IgnoredNotRequiredAction));
            Assert.That(service.Current.IsActive, Is.True);
            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("open-map"));
        }

        [Test]
        public void Start_DoesNotSilentlyReplaceAnotherActiveSequence()
        {
            var first = service.Start(CreateTwoEntrySequence("first"));
            var second = service.Start(CreateTwoEntrySequence("second"));

            Assert.That(first.Status, Is.EqualTo(TutorialStartStatus.Started));
            Assert.That(second.Status, Is.EqualTo(TutorialStartStatus.RejectedAnotherSequenceIsActive));
            Assert.That(service.Current.Sequence.Id, Is.EqualTo("first"));
        }

        [Test]
        public void Skip_CompletesWholeSequence_WhenSequenceIsSkippable()
        {
            service.Start(CreateTwoEntrySequence(canSkip: true));

            var result = service.Skip();

            Assert.That(result.Status, Is.EqualTo(TutorialSkipStatus.Skipped));
            Assert.That(service.Current.IsActive, Is.False);
        }

        [Test]
        public void CombinedEntry_CannotUseContinueDialogueAsRequiredAction()
        {
            var entry = new TutorialEntryDefinition(
                "invalid-combined",
                new DialogueDefinition("Guide", "Open the map.", TutorialDialogueSide.Left),
                new IndicationDefinition("hud.map-button", TutorialInteractionPolicy.Blocking),
                new ContinueDialogueRequiredActionDefinition());
            var sequence = new TutorialSequenceDefinition("invalid", false, new[] { entry });

            var result = service.Start(sequence);

            Assert.That(result.Status, Is.EqualTo(TutorialStartStatus.InvalidDefinition));
            Assert.That(result.Issues, Has.Some.Matches<TutorialValidationIssue>(
                issue => issue.Severity == TutorialValidationSeverity.Error));
        }

        private static TutorialSequenceDefinition CreateTwoEntrySequence(
            string id = "map-tutorial",
            bool canSkip = true)
        {
            return new TutorialSequenceDefinition(
                id,
                canSkip,
                new List<TutorialEntryDefinition>
                {
                    new TutorialEntryDefinition(
                        "explain-map",
                        new DialogueDefinition("Guide", "Let me show you the map.", TutorialDialogueSide.Left),
                        null,
                        new ContinueDialogueRequiredActionDefinition()),
                    new TutorialEntryDefinition(
                        "open-map",
                        new DialogueDefinition("Guide", "Open the map now.", TutorialDialogueSide.Left),
                        new IndicationDefinition("hud.map-button", TutorialInteractionPolicy.Blocking),
                        new SemanticActionRequiredActionDefinition(OpenMapAction))
                });
        }

        private static TutorialSequenceDefinition CreateIndicationOnlySequence()
        {
            return new TutorialSequenceDefinition(
                "map-indication",
                false,
                new[]
                {
                    new TutorialEntryDefinition(
                        "open-map",
                        null,
                        new IndicationDefinition("hud.map-button", TutorialInteractionPolicy.NonBlocking),
                        new SemanticActionRequiredActionDefinition(OpenMapAction))
                });
        }
    }
}
