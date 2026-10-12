using System.Collections;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Example;
using AMTutorialArchitecture.Tutorial.Unity.Integration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Tests
{
    public sealed class TutorialExampleSceneTests
    {
        [UnityTest]
        public IEnumerator ExampleScene_CompletesDialogueAndBlockingIndicationFlow()
        {
            SceneManager.LoadScene("TutorialExample");
            yield return null;

            var service = ProjectContext.Instance.Container.Resolve<ITutorialService>();
            var startTrigger = Object.FindFirstObjectByType<TutorialStartTrigger>();
            Assert.That(startTrigger, Is.Not.Null);

            startTrigger.Begin();
            yield return null;

            Assert.That(service.Current.IsActive, Is.True);
            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("explain-map"));

            service.SubmitAction(TutorialActionOccurrence.Success(TutorialActionIds.ContinueDialogue));
            yield return null;

            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("open-map"));
            Assert.That(service.Current.CurrentEntry.InteractionPolicy,
                Is.EqualTo(TutorialInteractionPolicy.Blocking));
            Assert.That(
                service.Current.CurrentEntry.Indications.Select(indication => indication.TargetId),
                Is.EquivalentTo(new[] { "hud.map-button", "hud.map-status" }));

            var highlight = FindSceneObject("CustomMapHighlight");
            var statusHighlight = FindSceneObject("CustomMapStatusHighlight");
            var startButton = FindSceneObject("StartTutorialButton").GetComponent<Button>();
            var mapButton = FindSceneObject("MapButton").GetComponent<Button>();
            var skipButton = FindSceneObject("SkipButton").GetComponent<Button>();

            Assert.That(highlight.activeSelf, Is.True, "The target View should choose and show its highlight.");
            Assert.That(statusHighlight.activeSelf, Is.True, "Every indicated target should be highlighted.");
            Assert.That(startButton.interactable, Is.False, "Blocking policy should disable unrelated interactions.");
            Assert.That(mapButton.interactable, Is.True, "Blocking policy must keep the required target interactive.");
            Assert.That(skipButton.interactable, Is.True, "Sequence skip remains available when configured.");

            mapButton.onClick.Invoke();
            yield return null;

            Assert.That(service.Current.IsActive, Is.False);
            Assert.That(highlight.activeSelf, Is.False);
            Assert.That(statusHighlight.activeSelf, Is.False);
            Assert.That(startButton.interactable, Is.True);
            Assert.That(Object.FindFirstObjectByType<ExampleMapView>(), Is.Not.Null);
            Assert.That(FindSceneObject("MapPanel").activeSelf, Is.True,
                "The normal feature action should execute as well as tutorial progression.");
        }

        private static GameObject FindSceneObject(string objectName)
        {
            var activeScene = SceneManager.GetActiveScene();
            var match = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(gameObject => gameObject.name == objectName && gameObject.scene == activeScene);
            Assert.That(match, Is.Not.Null, $"Expected scene object '{objectName}'.");
            return match;
        }
    }
}
