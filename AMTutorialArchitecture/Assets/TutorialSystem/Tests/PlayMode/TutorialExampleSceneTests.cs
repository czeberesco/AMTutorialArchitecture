using System.Collections;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Unity.Bindings;
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
        public IEnumerator ExampleScene_DemonstratesEveryTutorialPresentationVariant()
        {
            SceneManager.LoadScene("TutorialExample");
            yield return null;

            var service = ProjectContext.Instance.Container.Resolve<ITutorialService>();
            var startTrigger = Object.FindFirstObjectByType<TutorialStartTrigger>();
            Assert.That(startTrigger, Is.Not.Null);

            startTrigger.Begin();
            yield return null;

            Assert.That(service.Current.IsActive, Is.True);
            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("select-hero"));

            var heroButton = FindSceneObject("HeroButton").GetComponent<Button>();
            var mapButton = FindSceneObject("MapButton").GetComponent<Button>();
            var upgradeButton = FindSceneObject("UpgradeButton").GetComponent<Button>();
            var northButton = FindSceneObject("NorthPathButton").GetComponent<Button>();
            var southButton = FindSceneObject("SouthPathButton").GetComponent<Button>();
            var rewardButton = FindSceneObject("RewardButton").GetComponent<Button>();
            var startButton = FindSceneObject("StartTutorialButton").GetComponent<Button>();
            var continueButton = FindSceneObject("ContinueButton").GetComponent<Button>();
            var skipButton = FindSceneObject("SkipButton").GetComponent<Button>();

            Assert.That(FindSceneObject("HeroHighlight").activeSelf, Is.True);
            Assert.That(FindSceneObject("TutorialDialogue").activeSelf, Is.True);
            Assert.That(heroButton.interactable, Is.True);
            Assert.That(startButton.interactable, Is.False);

            heroButton.onClick.Invoke();
            yield return null;

            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("explain-map"));
            Assert.That(service.Current.CurrentEntry.HasDialogue, Is.True);
            Assert.That(service.Current.CurrentEntry.HasIndication, Is.False);
            Assert.That(continueButton.gameObject.activeSelf, Is.True);

            continueButton.onClick.Invoke();
            yield return null;

            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("open-map"));
            Assert.That(service.Current.CurrentEntry.HasDialogue, Is.True);
            Assert.That(service.Current.CurrentEntry.InteractionPolicy,
                Is.EqualTo(TutorialInteractionPolicy.Blocking));
            Assert.That(
                service.Current.CurrentEntry.Indications.Select(indication => indication.TargetId),
                Is.EquivalentTo(new[] { "hud.map-button" }));
            Assert.That(FindSceneObject("MapHighlight").activeSelf, Is.True);

            mapButton.onClick.Invoke();
            yield return null;

            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("upgrade-hero"));
            Assert.That(service.Current.CurrentEntry.HasDialogue, Is.False);
            Assert.That(service.Current.CurrentEntry.InteractionPolicy,
                Is.EqualTo(TutorialInteractionPolicy.NonBlocking));
            Assert.That(FindSceneObject("UpgradeHighlight").activeSelf, Is.True);
            Assert.That(startButton.interactable, Is.True, "A non-blocking highlight must leave unrelated input available.");

            upgradeButton.onClick.Invoke();
            yield return null;

            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("choose-path"));
            Assert.That(
                service.Current.CurrentEntry.Indications.Select(indication => indication.TargetId),
                Is.EquivalentTo(new[] { "hud.path-north", "hud.path-south", "hud.map-status" }));
            Assert.That(FindSceneObject("NorthPathHighlight").activeSelf, Is.True);
            Assert.That(FindSceneObject("SouthPathHighlight").activeSelf, Is.True);
            Assert.That(FindSceneObject("MapStatusHighlight").activeSelf, Is.True);
            Assert.That(northButton.interactable, Is.True);
            Assert.That(southButton.interactable, Is.True);
            Assert.That(heroButton.interactable, Is.False);
            Assert.That(skipButton.interactable, Is.True);

            northButton.onClick.Invoke();
            yield return null;

            Assert.That(service.Current.IsActive, Is.False);
            Assert.That(FindSceneObject("NorthPathHighlight").activeSelf, Is.False);
            Assert.That(FindSceneObject("SouthPathHighlight").activeSelf, Is.False);
            Assert.That(FindSceneObject("MapStatusHighlight").activeSelf, Is.False);
            Assert.That(startButton.interactable, Is.True);

            yield return new WaitForSecondsRealtime(2.2f);

            var delayedHint = Object.FindFirstObjectByType<DelayedTutorialHintBinding>();
            Assert.That(delayedHint, Is.Not.Null);
            Assert.That(delayedHint.IsVisible, Is.True);
            Assert.That(FindSceneObject("RewardHintHighlight").activeSelf, Is.True);

            rewardButton.onClick.Invoke();
            yield return null;

            Assert.That(delayedHint.IsVisible, Is.False);
            Assert.That(FindSceneObject("RewardHintHighlight").activeSelf, Is.False);
            Assert.That(FindSceneObject("FeatureStatus").GetComponent<Text>().text, Does.Contain("Reward claimed"));

            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(FindSceneObject("RewardHintHighlight").activeSelf, Is.False,
                "Any reported interaction should reset the inactivity timer.");
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
