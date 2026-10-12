using System.Collections.Generic;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Commands;
using AMTutorialArchitecture.Tutorial.Domain.Model;
using AMTutorialArchitecture.Tutorial.Domain.Validation;
using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using AMTutorialArchitecture.Tutorial.Unity.Services;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AMTutorialArchitecture.Tutorial.Tests
{
    public sealed class TutorialAssetConversionTests
    {
        private readonly List<Object> assets = new List<Object>();
        private readonly List<string> assetPaths = new List<string>();

        [TearDown]
        public void TearDown()
        {
            foreach (var path in assetPaths)
            {
                AssetDatabase.DeleteAsset(path);
            }

            assetPaths.Clear();

            foreach (var asset in assets)
            {
                Object.DestroyImmediate(asset);
            }

            assets.Clear();
        }

        [Test]
        public void AssetExtension_ConvertsAuthoredData_AndStartsService()
        {
            var dialogue = Create<DialogueAsset>();
            Set(dialogue, "speakerId", "Guide");
            Set(dialogue, "text", "Welcome.");

            var continueAction = Create<ContinueDialogueRequiredActionAsset>();
            Set(continueAction, "assetGuid", "test-continue-action-guid");
            var entry = Create<TutorialEntryAsset>();
            Set(entry, "id", "welcome");
            Set(entry, "dialogue", dialogue);
            Set(entry, "indications", new Object[0]);
            Set(entry, "requiredAction", continueAction);

            var sequence = Create<TutorialSequenceAsset>();
            Set(sequence, "id", "first-login");
            Set(sequence, "entries", new[] { entry });

            var model = new TutorialModel();
            var service = new TutorialService(
                new TutorialCommandProcessor(model, new TutorialDefinitionValidator()),
                model);

            var result = service.Start(sequence);

            Assert.That(result.Status, Is.EqualTo(TutorialStartStatus.Started));
            Assert.That(service.Current.Sequence.Id, Is.EqualTo("first-login"));
            Assert.That(service.Current.CurrentEntry.Id, Is.EqualTo("welcome"));
        }

        [Test]
        public void RequiredActionAsset_UsesItsUnityGuidAsTheDomainActionId()
        {
            const string path = "Assets/TutorialSystem/Tests/EditMode/__ActionIdentityTest.asset";
            AssetDatabase.DeleteAsset(path);

            var action = ScriptableObject.CreateInstance<SemanticActionRequiredActionAsset>();
            AssetDatabase.CreateAsset(action, path);
            assetPaths.Add(path);
            var assignedId = action.Id;
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            action = AssetDatabase.LoadAssetAtPath<SemanticActionRequiredActionAsset>(path);
            var expectedGuid = AssetDatabase.AssetPathToGUID(path);
            var serializedAction = new SerializedObject(action);
            var storedGuid = serializedAction.FindProperty("assetGuid").stringValue;

            Assert.That(assignedId, Is.EqualTo(expectedGuid));
            Assert.That(storedGuid, Is.EqualTo(expectedGuid));
            Assert.That(action.Id, Is.EqualTo(expectedGuid));
            Assert.That(action.CreateDefinition().ExpectedActionId, Is.EqualTo(expectedGuid));
        }

        private T Create<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            assets.Add(asset);
            return asset;
        }

        private static void Set(Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(Object target, string propertyName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
