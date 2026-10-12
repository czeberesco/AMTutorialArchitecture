using System.Collections.Generic;
using AMTutorialArchitecture.Tutorial.Example;
using AMTutorialArchitecture.Tutorial.Unity.Authoring;
using AMTutorialArchitecture.Tutorial.Unity.Bindings;
using AMTutorialArchitecture.Tutorial.Unity.Composition;
using AMTutorialArchitecture.Tutorial.Unity.Integration;
using AMTutorialArchitecture.Tutorial.Unity.Targets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Editor
{
    public static class TutorialExampleBuilder
    {
        private const string Root = "Assets/TutorialSystem/Example";
        private const string Authoring = Root + "/Authoring";
        private const string Scenes = Root + "/Scenes";
        private const string ScenePath = Scenes + "/TutorialExample.unity";
        private const string ProjectContextPath = "Assets/Resources/ProjectContext.prefab";
        private const string OpenMapActionId = "game.map.open";

        [MenuItem("Tools/Tutorial/Rebuild Example")]
        public static void Build()
        {
            EnsureFolder("Assets", "TutorialSystem");
            EnsureFolder("Assets/TutorialSystem", "Example");
            EnsureFolder(Root, "Authoring");
            EnsureFolder(Root, "Scenes");
            EnsureFolder("Assets", "Resources");

            var content = CreateAuthoringAssets();
            CreateProjectContext();
            CreateScene(content.Sequence, content.Target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Tutorial] Rebuilt example scene at '{ScenePath}'.");
        }

        private static ExampleContent CreateAuthoringAssets()
        {
            var target = LoadOrCreate<TutorialTargetAsset>(Authoring + "/MapButtonTarget.asset");
            Set(target, "id", "hud.map-button");

            var introDialogue = LoadOrCreate<DialogueAsset>(Authoring + "/IntroDialogue.asset");
            Set(introDialogue, "speakerId", "Guide");
            Set(introDialogue, "text", "Every expedition starts with knowing where to go.");

            var actionDialogue = LoadOrCreate<DialogueAsset>(Authoring + "/OpenMapDialogue.asset");
            Set(actionDialogue, "speakerId", "Guide");
            Set(actionDialogue, "text", "Open the map to choose our destination.");

            var indication = LoadOrCreate<IndicationAsset>(Authoring + "/MapButtonIndication.asset");
            Set(indication, "target", target);
            SetEnum(indication, "interactionPolicy", 1);

            var continueAction = LoadOrCreate<ContinueDialogueRequiredActionAsset>(
                Authoring + "/ContinueDialogueAction.asset");

            var openMapAction = LoadOrCreate<SemanticActionRequiredActionAsset>(
                Authoring + "/OpenMapAction.asset");
            Set(openMapAction, "actionId", OpenMapActionId);
            Set(openMapAction, "description", "Open the map successfully.");

            var introEntry = LoadOrCreate<TutorialEntryAsset>(Authoring + "/IntroEntry.asset");
            Set(introEntry, "id", "explain-map");
            Set(introEntry, "dialogue", introDialogue);
            Set(introEntry, "indication", (Object)null);
            Set(introEntry, "requiredAction", continueAction);

            var actionEntry = LoadOrCreate<TutorialEntryAsset>(Authoring + "/OpenMapEntry.asset");
            Set(actionEntry, "id", "open-map");
            Set(actionEntry, "dialogue", actionDialogue);
            Set(actionEntry, "indication", indication);
            Set(actionEntry, "requiredAction", openMapAction);

            var sequence = LoadOrCreate<TutorialSequenceAsset>(Authoring + "/MapTutorialSequence.asset");
            Set(sequence, "id", "example.map-tutorial");
            Set(sequence, "canSkip", true);
            SetArray(sequence, "entries", new Object[] { introEntry, actionEntry });

            return new ExampleContent(sequence, target);
        }

        private static void CreateProjectContext()
        {
            var root = new GameObject("ProjectContext");
            try
            {
                var context = root.AddComponent<ProjectContext>();
                var installer = root.AddComponent<TutorialProjectInstaller>();
                context.Installers = new MonoInstaller[] { installer };
                PrefabUtility.SaveAsPrefabAsset(root, ProjectContextPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void CreateScene(TutorialSequenceAsset sequence, TutorialTargetAsset target)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();
            CreateSceneContext();

            var canvas = CreateCanvas();
            var background = CreateImage("Background", canvas.transform, new Color(0.055f, 0.075f, 0.12f, 1f));
            Stretch(background.rectTransform);

            var title = CreateText("Title", canvas.transform, "Tutorial Architecture Proof", 42, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(900f, 70f));

            var description = CreateText(
                "Description",
                canvas.transform,
                "Start the sequence, advance the first dialogue, then use the highlighted Map button.",
                22,
                TextAnchor.MiddleCenter);
            SetRect(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1100f, 60f));

            var startButton = CreateButton("StartTutorialButton", canvas.transform, "Start Tutorial", new Color(0.18f, 0.47f, 0.78f));
            SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.55f), new Vector2(-230f, 0f), new Vector2(300f, 90f));

            var mapButton = CreateButton("MapButton", canvas.transform, "Open Map", new Color(0.31f, 0.65f, 0.38f));
            SetRect(mapButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.55f), new Vector2(230f, 0f), new Vector2(300f, 90f));

            var highlight = CreateImage("CustomMapHighlight", mapButton.transform, new Color(1f, 0.75f, 0.1f, 0.75f));
            highlight.raycastTarget = false;
            highlight.transform.SetAsFirstSibling();
            SetRect(highlight.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(330f, 120f));
            highlight.gameObject.SetActive(false);

            var highlightView = mapButton.gameObject.AddComponent<GameObjectHighlightView>();
            Set(highlightView, "highlightRoot", highlight.gameObject);
            var targetView = mapButton.gameObject.AddComponent<TutorialTargetView>();
            Set(targetView, "target", target);
            Set(targetView, "highlightView", highlightView);
            Set(targetView, "selectable", mapButton);

            var status = CreateText("FeatureStatus", canvas.transform, "Map is closed.", 24, TextAnchor.MiddleCenter);
            SetRect(status.rectTransform, new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(900f, 60f));

            var mapPanel = CreateImage("MapPanel", canvas.transform, new Color(0.12f, 0.24f, 0.2f, 0.96f));
            SetRect(mapPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(760f, 360f));
            var mapLabel = CreateText("MapLabel", mapPanel.transform, "WORLD MAP\n\nThe tutorial completed after the feature action succeeded.", 28, TextAnchor.MiddleCenter);
            Stretch(mapLabel.rectTransform, 30f);
            mapPanel.gameObject.SetActive(false);

            var mapFeature = mapButton.gameObject.AddComponent<ExampleMapView>();
            Set(mapFeature, "openMapButton", mapButton);
            Set(mapFeature, "mapPanel", mapPanel.gameObject);
            Set(mapFeature, "statusText", status);

            var actionEmitter = mapButton.gameObject.AddComponent<TutorialActionEmitter>();
            Set(actionEmitter, "actionId", OpenMapActionId);
            Set(actionEmitter, "button", mapButton);

            var startTrigger = startButton.gameObject.AddComponent<TutorialStartTrigger>();
            Set(startTrigger, "sequence", sequence);
            Set(startTrigger, "button", startButton);

            var tutorialUi = CreateTutorialUi(canvas.transform);
            var viewBinding = canvas.gameObject.AddComponent<TutorialViewBinding>();
            Set(viewBinding, "dialogueRoot", tutorialUi.DialoguePanel.gameObject);
            Set(viewBinding, "dialoguePanel", tutorialUi.DialoguePanel);
            Set(viewBinding, "speakerText", tutorialUi.Speaker);
            Set(viewBinding, "bodyText", tutorialUi.Body);
            Set(viewBinding, "progressText", tutorialUi.Progress);
            Set(viewBinding, "continueButton", tutorialUi.Continue);
            Set(viewBinding, "skipButton", tutorialUi.Skip);

            canvas.gameObject.AddComponent<TutorialTargetBinding>();
            var inputGate = canvas.gameObject.AddComponent<TutorialInputGateBinding>();
            Set(inputGate, "interactionRoot", canvas.transform);
            SetArray(inputGate, "alwaysAllowed", new Object[] { tutorialUi.Skip });

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.12f);
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            var input = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            input.AssignDefaultActions();
        }

        private static void CreateSceneContext()
        {
            var contextObject = new GameObject("SceneContext");
            var context = contextObject.AddComponent<SceneContext>();
            var installer = contextObject.AddComponent<TutorialSceneInstaller>();
            context.Installers = new MonoInstaller[] { installer };
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("TutorialExampleCanvas", typeof(RectTransform));
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static TutorialUi CreateTutorialUi(Transform parent)
        {
            var dialoguePanel = CreateImage("TutorialDialogue", parent, new Color(0.035f, 0.045f, 0.07f, 0.97f));
            SetRect(dialoguePanel.rectTransform, new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(720f, 240f));

            var speaker = CreateText("Speaker", dialoguePanel.transform, string.Empty, 25, TextAnchor.MiddleLeft);
            speaker.color = new Color(1f, 0.78f, 0.25f);
            SetRect(speaker.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -35f), new Vector2(650f, 45f));

            var body = CreateText("Body", dialoguePanel.transform, string.Empty, 24, TextAnchor.UpperLeft);
            SetRect(body.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0f, 0f), new Vector2(650f, 105f));

            var progress = CreateText("Progress", dialoguePanel.transform, string.Empty, 18, TextAnchor.MiddleLeft);
            SetRect(progress.rectTransform, new Vector2(0f, 0f), new Vector2(45f, 25f), new Vector2(100f, 35f));

            var continueButton = CreateButton("ContinueButton", dialoguePanel.transform, "Continue", new Color(0.18f, 0.47f, 0.78f));
            SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(-100f, 35f), new Vector2(170f, 55f));

            var skipButton = CreateButton("SkipButton", parent, "Skip Tutorial", new Color(0.35f, 0.37f, 0.42f));
            SetRect(skipButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-120f, -45f), new Vector2(200f, 55f));

            dialoguePanel.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(false);
            return new TutorialUi(dialoguePanel.rectTransform, speaker, body, progress, continueButton, skipButton);
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color)
        {
            var image = CreateImage(name, parent, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText("Label", image.transform, label, 25, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            var text = gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void Set(Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void Set(Object target, string propertyName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void Set(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).enumValueIndex = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetArray(Object target, string propertyName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private sealed class ExampleContent
        {
            public ExampleContent(TutorialSequenceAsset sequence, TutorialTargetAsset target)
            {
                Sequence = sequence;
                Target = target;
            }

            public TutorialSequenceAsset Sequence { get; }

            public TutorialTargetAsset Target { get; }
        }

        private sealed class TutorialUi
        {
            public TutorialUi(
                RectTransform dialoguePanel,
                Text speaker,
                Text body,
                Text progress,
                Button continueButton,
                Button skipButton)
            {
                DialoguePanel = dialoguePanel;
                Speaker = speaker;
                Body = body;
                Progress = progress;
                Continue = continueButton;
                Skip = skipButton;
            }

            public RectTransform DialoguePanel { get; }

            public Text Speaker { get; }

            public Text Body { get; }

            public Text Progress { get; }

            public Button Continue { get; }

            public Button Skip { get; }
        }
    }
}
