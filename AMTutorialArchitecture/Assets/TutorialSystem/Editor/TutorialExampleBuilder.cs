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
        private const string MapTutorial = Authoring + "/MapTutorial";
        private const string Targets = MapTutorial + "/Targets";
        private const string Steps = MapTutorial + "/Steps";
        private const string SelectHeroStep = Steps + "/01_SelectHero";
        private const string ExplainMapStep = Steps + "/02_ExplainMap";
        private const string OpenMapStep = Steps + "/03_OpenMap";
        private const string UpgradeHeroStep = Steps + "/04_UpgradeHero";
        private const string ChoosePathStep = Steps + "/05_ChoosePath";
        private const string DelayedHints = MapTutorial + "/DelayedHints";
        private const string Scenes = Root + "/Scenes";
        private const string ScenePath = Scenes + "/TutorialExample.unity";
        private const string ProjectContextPath = "Assets/Resources/ProjectContext.prefab";
        private const string SelectHeroActionId = "game.hero.select";
        private const string OpenMapActionId = "game.map.open";
        private const string UpgradeHeroActionId = "game.hero.upgrade";
        private const string ChoosePathActionId = "game.path.choose";

        [MenuItem("Tools/Tutorial/Rebuild Example")]
        public static void Build()
        {
            EnsureFolder("Assets", "TutorialSystem");
            EnsureFolder("Assets/TutorialSystem", "Example");
            EnsureFolder(Root, "Authoring");
            EnsureFolder(Authoring, "MapTutorial");
            EnsureFolder(MapTutorial, "Targets");
            EnsureFolder(MapTutorial, "Steps");
            EnsureFolder(Steps, "01_SelectHero");
            EnsureFolder(Steps, "02_ExplainMap");
            EnsureFolder(Steps, "03_OpenMap");
            EnsureFolder(Steps, "04_UpgradeHero");
            EnsureFolder(Steps, "05_ChoosePath");
            EnsureFolder(MapTutorial, "DelayedHints");
            EnsureFolder(Root, "Scenes");
            EnsureFolder("Assets", "Resources");
            MigrateLegacyAuthoringAssets();

            var content = CreateAuthoringAssets();
            CreateProjectContext();
            CreateScene(content);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Tutorial] Rebuilt example scene at '{ScenePath}'.");
        }

        private static ExampleContent CreateAuthoringAssets()
        {
            var heroTarget = CreateTarget("HeroButtonTarget", "hud.hero-button");
            var mapButtonTarget = LoadOrCreate<TutorialTargetAsset>(Targets + "/MapButtonTarget.asset");
            Set(mapButtonTarget, "id", "hud.map-button");
            var mapStatusTarget = LoadOrCreate<TutorialTargetAsset>(Targets + "/MapStatusTarget.asset");
            Set(mapStatusTarget, "id", "hud.map-status");
            var upgradeTarget = CreateTarget("UpgradeButtonTarget", "hud.upgrade-button");
            var northPathTarget = CreateTarget("NorthPathTarget", "hud.path-north");
            var southPathTarget = CreateTarget("SouthPathTarget", "hud.path-south");
            var rewardTarget = CreateTarget("RewardButtonTarget", "hud.reward-button");

            var selectHeroDialogue = CreateDialogue(
                SelectHeroStep + "/SelectHeroDialogue.asset",
                "Guide",
                "Choose a hero before we begin the expedition.");
            var selectHeroIndication = CreateIndication(
                SelectHeroStep + "/HeroButtonIndication.asset",
                heroTarget);
            var selectHeroAction = CreateSemanticAction(
                SelectHeroStep + "/SelectHeroAction.asset",
                SelectHeroActionId,
                "Select the hero successfully.");
            var selectHeroEntry = CreateEntry(
                SelectHeroStep + "/SelectHeroEntry.asset",
                "select-hero",
                selectHeroDialogue,
                new Object[] { selectHeroIndication },
                1,
                selectHeroAction);

            var explainMapDialogue = CreateDialogue(
                ExplainMapStep + "/MapExplanationDialogue.asset",
                "Guide",
                "Our route crosses dangerous territory. Let me explain how navigation works.");
            var continueAction = LoadOrCreate<ContinueDialogueRequiredActionAsset>(
                ExplainMapStep + "/ContinueDialogueAction.asset");
            var explainMapEntry = CreateEntry(
                ExplainMapStep + "/MapExplanationEntry.asset",
                "explain-map",
                explainMapDialogue,
                new Object[0],
                0,
                continueAction);

            var openMapDialogue = CreateDialogue(
                OpenMapStep + "/OpenMapDialogue.asset",
                "Guide",
                "That is the theory. Now open the map and see the available routes.");
            var mapButtonIndication = LoadOrCreate<IndicationAsset>(OpenMapStep + "/MapButtonIndication.asset");
            Set(mapButtonIndication, "target", mapButtonTarget);
            var openMapAction = LoadOrCreate<SemanticActionRequiredActionAsset>(
                OpenMapStep + "/OpenMapAction.asset");
            Set(openMapAction, "actionId", OpenMapActionId);
            Set(openMapAction, "description", "Open the map successfully.");
            var openMapEntry = CreateEntry(
                OpenMapStep + "/OpenMapEntry.asset",
                "open-map",
                openMapDialogue,
                new Object[] { mapButtonIndication },
                1,
                openMapAction);

            var upgradeIndication = CreateIndication(
                UpgradeHeroStep + "/UpgradeButtonIndication.asset",
                upgradeTarget);
            var upgradeAction = CreateSemanticAction(
                UpgradeHeroStep + "/UpgradeHeroAction.asset",
                UpgradeHeroActionId,
                "Upgrade the selected hero.");
            var upgradeEntry = CreateEntry(
                UpgradeHeroStep + "/UpgradeHeroEntry.asset",
                "upgrade-hero",
                null,
                new Object[] { upgradeIndication },
                0,
                upgradeAction);

            var northIndication = CreateIndication(
                ChoosePathStep + "/NorthPathIndication.asset",
                northPathTarget);
            var southIndication = CreateIndication(
                ChoosePathStep + "/SouthPathIndication.asset",
                southPathTarget);
            var statusIndication = LoadOrCreate<IndicationAsset>(ChoosePathStep + "/MapStatusIndication.asset");
            Set(statusIndication, "target", mapStatusTarget);
            var choosePathAction = CreateSemanticAction(
                ChoosePathStep + "/ChoosePathAction.asset",
                ChoosePathActionId,
                "Choose either available route.");
            var choosePathEntry = CreateEntry(
                ChoosePathStep + "/ChoosePathEntry.asset",
                "choose-path",
                null,
                new Object[] { northIndication, southIndication, statusIndication },
                1,
                choosePathAction);

            var sequence = LoadOrCreate<TutorialSequenceAsset>(MapTutorial + "/MapTutorialSequence.asset");
            Set(sequence, "id", "example.architecture-showcase");
            Set(sequence, "canSkip", true);
            SetArray(sequence, "entries", new Object[]
            {
                selectHeroEntry,
                explainMapEntry,
                openMapEntry,
                upgradeEntry,
                choosePathEntry
            });

            var rewardHint = LoadOrCreate<DelayedTutorialHintAsset>(DelayedHints + "/RewardHint.asset");
            Set(rewardHint, "inactivitySeconds", 2f);
            SetArray(rewardHint, "targets", new Object[] { rewardTarget });

            return new ExampleContent(
                sequence,
                rewardHint,
                heroTarget,
                mapButtonTarget,
                mapStatusTarget,
                upgradeTarget,
                northPathTarget,
                southPathTarget,
                rewardTarget);
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

        private static void CreateScene(ExampleContent content)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();
            CreateSceneContext();

            var canvas = CreateCanvas();
            var background = CreateImage("Background", canvas.transform, new Color(0.055f, 0.075f, 0.12f, 1f));
            Stretch(background.rectTransform);

            var title = CreateText("Title", canvas.transform, "Tutorial Architecture Showcase", 40, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(1000f, 60f));

            var description = CreateText(
                "Description",
                canvas.transform,
                "Dialogue + selection  →  multi-box dialogue  →  highlight only  →  multi-highlight  →  delayed hint",
                20,
                TextAnchor.MiddleCenter);
            SetRect(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(1500f, 50f));

            var startButton = CreateButton(
                "StartTutorialButton",
                canvas.transform,
                "Start Showcase",
                new Color(0.18f, 0.47f, 0.78f));
            SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -155f), new Vector2(280f, 62f));

            var status = CreateText(
                "FeatureStatus",
                canvas.transform,
                "Press Start Showcase to begin.",
                22,
                TextAnchor.MiddleCenter);
            SetRect(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(1200f, 52f));
            ConfigureTextTarget(
                status,
                content.MapStatusTarget,
                "MapStatusHighlight",
                new Color(0.2f, 0.75f, 1f, 0.42f),
                new Vector2(1240f, 66f));

            var heroButton = CreateFeatureButton(
                "HeroButton", canvas.transform, "Select Warrior", new Vector2(-480f, 0f), new Vector2(0.5f, 0.64f));
            var mapButton = CreateFeatureButton(
                "MapButton", canvas.transform, "Open Map", new Vector2(-160f, 0f), new Vector2(0.5f, 0.64f));
            var upgradeButton = CreateFeatureButton(
                "UpgradeButton", canvas.transform, "Upgrade Hero", new Vector2(160f, 0f), new Vector2(0.5f, 0.64f));
            var rewardButton = CreateFeatureButton(
                "RewardButton", canvas.transform, "Claim Reward", new Vector2(480f, 0f), new Vector2(0.5f, 0.64f));
            var northButton = CreateFeatureButton(
                "NorthPathButton", canvas.transform, "Choose North Route", new Vector2(-180f, 0f), new Vector2(0.5f, 0.49f));
            var southButton = CreateFeatureButton(
                "SouthPathButton", canvas.transform, "Choose South Route", new Vector2(180f, 0f), new Vector2(0.5f, 0.49f));

            ConfigureButtonTarget(heroButton, content.HeroTarget, "HeroHighlight", new Color(1f, 0.55f, 0.18f, 0.8f));
            ConfigureButtonTarget(mapButton, content.MapButtonTarget, "MapHighlight", new Color(0.25f, 0.8f, 1f, 0.8f));
            ConfigureButtonTarget(upgradeButton, content.UpgradeTarget, "UpgradeHighlight", new Color(0.8f, 0.35f, 1f, 0.8f));
            ConfigureButtonTarget(northButton, content.NorthPathTarget, "NorthPathHighlight", new Color(0.2f, 0.85f, 0.55f, 0.8f));
            ConfigureButtonTarget(southButton, content.SouthPathTarget, "SouthPathHighlight", new Color(0.2f, 0.85f, 0.55f, 0.8f));
            ConfigureButtonTarget(rewardButton, content.RewardTarget, "RewardHintHighlight", new Color(1f, 0.82f, 0.12f, 0.85f));

            ConfigureExampleAction(heroButton, status, "Warrior selected. The single-box dialogue entry completed.");
            ConfigureExampleAction(mapButton, status, "Map opened. The two-box dialogue example completed.");
            ConfigureExampleAction(upgradeButton, status, "Hero upgraded through a highlight-only entry.");
            ConfigureExampleAction(northButton, status, "North route selected. Wait 2 seconds for the reward hint.");
            ConfigureExampleAction(southButton, status, "South route selected. Wait 2 seconds for the reward hint.");
            ConfigureExampleAction(rewardButton, status, "Reward claimed. The inactivity timer has restarted.");

            ConfigureActionEmitter(heroButton, SelectHeroActionId);
            ConfigureActionEmitter(mapButton, OpenMapActionId);
            ConfigureActionEmitter(upgradeButton, UpgradeHeroActionId);
            ConfigureActionEmitter(northButton, ChoosePathActionId);
            ConfigureActionEmitter(southButton, ChoosePathActionId);

            var startTrigger = startButton.gameObject.AddComponent<TutorialStartTrigger>();
            Set(startTrigger, "sequence", content.Sequence);
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

            var delayedHint = canvas.gameObject.AddComponent<DelayedTutorialHintBinding>();
            Set(delayedHint, "hint", content.RewardHint);
            Set(delayedHint, "armAfterSequenceFinishes", true);

            ConfigureActivityReporter(startButton);
            ConfigureActivityReporter(heroButton);
            ConfigureActivityReporter(mapButton);
            ConfigureActivityReporter(upgradeButton);
            ConfigureActivityReporter(northButton);
            ConfigureActivityReporter(southButton);
            ConfigureActivityReporter(rewardButton);
            ConfigureActivityReporter(tutorialUi.Continue);
            ConfigureActivityReporter(tutorialUi.Skip);

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

        private static Button CreateFeatureButton(
            string name,
            Transform parent,
            string label,
            Vector2 position,
            Vector2 anchor)
        {
            var button = CreateButton(name, parent, label, new Color(0.22f, 0.38f, 0.52f));
            SetRect(button.GetComponent<RectTransform>(), anchor, position, new Vector2(285f, 76f));
            return button;
        }

        private static void ConfigureButtonTarget(
            Button button,
            TutorialTargetAsset target,
            string highlightName,
            Color highlightColor)
        {
            var highlight = CreateImage(highlightName, button.transform, highlightColor);
            highlight.raycastTarget = false;
            highlight.transform.SetAsFirstSibling();
            SetRect(highlight.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(310f, 101f));
            highlight.gameObject.SetActive(false);

            var highlightView = button.gameObject.AddComponent<GameObjectHighlightView>();
            Set(highlightView, "highlightRoot", highlight.gameObject);
            var targetView = button.gameObject.AddComponent<TutorialTargetView>();
            Set(targetView, "target", target);
            Set(targetView, "highlightView", highlightView);
            Set(targetView, "selectable", button);
        }

        private static void ConfigureTextTarget(
            Text text,
            TutorialTargetAsset target,
            string highlightName,
            Color highlightColor,
            Vector2 highlightSize)
        {
            var highlight = CreateImage(highlightName, text.transform, highlightColor);
            highlight.raycastTarget = false;
            highlight.transform.SetAsFirstSibling();
            SetRect(highlight.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, highlightSize);
            highlight.gameObject.SetActive(false);

            var highlightView = text.gameObject.AddComponent<GameObjectHighlightView>();
            Set(highlightView, "highlightRoot", highlight.gameObject);
            var targetView = text.gameObject.AddComponent<TutorialTargetView>();
            Set(targetView, "target", target);
            Set(targetView, "highlightView", highlightView);
        }

        private static void ConfigureExampleAction(Button button, Text status, string message)
        {
            var actionView = button.gameObject.AddComponent<ExampleActionView>();
            Set(actionView, "button", button);
            Set(actionView, "statusText", status);
            Set(actionView, "successMessage", message);
        }

        private static void ConfigureActionEmitter(Button button, string actionId)
        {
            var emitter = button.gameObject.AddComponent<TutorialActionEmitter>();
            Set(emitter, "actionId", actionId);
            Set(emitter, "button", button);
        }

        private static void ConfigureActivityReporter(Button button)
        {
            var reporter = button.gameObject.AddComponent<TutorialActivityReporter>();
            Set(reporter, "button", button);
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

        private static TutorialTargetAsset CreateTarget(string fileName, string id)
        {
            var target = LoadOrCreate<TutorialTargetAsset>($"{Targets}/{fileName}.asset");
            Set(target, "id", id);
            return target;
        }

        private static DialogueAsset CreateDialogue(string path, string speakerId, string text)
        {
            var dialogue = LoadOrCreate<DialogueAsset>(path);
            Set(dialogue, "speakerId", speakerId);
            Set(dialogue, "text", text);
            return dialogue;
        }

        private static IndicationAsset CreateIndication(string path, TutorialTargetAsset target)
        {
            var indication = LoadOrCreate<IndicationAsset>(path);
            Set(indication, "target", target);
            return indication;
        }

        private static SemanticActionRequiredActionAsset CreateSemanticAction(
            string path,
            string actionId,
            string description)
        {
            var action = LoadOrCreate<SemanticActionRequiredActionAsset>(path);
            Set(action, "actionId", actionId);
            Set(action, "description", description);
            return action;
        }

        private static TutorialEntryAsset CreateEntry(
            string path,
            string id,
            DialogueAsset dialogue,
            Object[] indications,
            int interactionPolicy,
            RequiredActionAsset requiredAction)
        {
            var entry = LoadOrCreate<TutorialEntryAsset>(path);
            Set(entry, "id", id);
            Set(entry, "dialogue", dialogue);
            SetArray(entry, "indications", indications);
            SetEnum(entry, "interactionPolicy", interactionPolicy);
            Set(entry, "requiredAction", requiredAction);
            return entry;
        }

        private static void MigrateLegacyAuthoringAssets()
        {
            MoveAssetIfPresent(
                Steps + "/01_ExplainMap/IntroDialogue.asset",
                ExplainMapStep + "/MapExplanationDialogue.asset");
            MoveAssetIfPresent(
                Steps + "/01_ExplainMap/IntroEntry.asset",
                ExplainMapStep + "/MapExplanationEntry.asset");
            MoveAssetIfPresent(
                Steps + "/01_ExplainMap/ContinueDialogueAction.asset",
                ExplainMapStep + "/ContinueDialogueAction.asset");
            MoveAssetIfPresent(
                Steps + "/02_OpenMap/OpenMapDialogue.asset",
                OpenMapStep + "/OpenMapDialogue.asset");
            MoveAssetIfPresent(
                Steps + "/02_OpenMap/OpenMapEntry.asset",
                OpenMapStep + "/OpenMapEntry.asset");
            MoveAssetIfPresent(
                Steps + "/02_OpenMap/MapButtonIndication.asset",
                OpenMapStep + "/MapButtonIndication.asset");
            MoveAssetIfPresent(
                Steps + "/02_OpenMap/MapStatusIndication.asset",
                ChoosePathStep + "/MapStatusIndication.asset");
            MoveAssetIfPresent(
                Steps + "/02_OpenMap/OpenMapAction.asset",
                OpenMapStep + "/OpenMapAction.asset");

            MoveAssetIfPresent(Authoring + "/MapTutorialSequence.asset", MapTutorial + "/MapTutorialSequence.asset");
            MoveAssetIfPresent(
                Authoring + "/MapTutorialSequence/MapTutorialSequence.asset",
                MapTutorial + "/MapTutorialSequence.asset");
            MoveAssetIfPresent(Authoring + "/MapButtonTarget.asset", Targets + "/MapButtonTarget.asset");
            MoveAssetIfPresent(Authoring + "/IntroDialogue.asset", ExplainMapStep + "/MapExplanationDialogue.asset");
            MoveAssetIfPresent(
                Authoring + "/MapTutorialSequence/IntroEntry/IntroDialogue.asset",
                ExplainMapStep + "/MapExplanationDialogue.asset");
            MoveAssetIfPresent(Authoring + "/IntroEntry.asset", ExplainMapStep + "/MapExplanationEntry.asset");
            MoveAssetIfPresent(
                Authoring + "/MapTutorialSequence/IntroEntry/IntroEntry.asset",
                ExplainMapStep + "/MapExplanationEntry.asset");
            MoveAssetIfPresent(
                Authoring + "/ContinueDialogueAction.asset",
                ExplainMapStep + "/ContinueDialogueAction.asset");
            MoveAssetIfPresent(
                Authoring + "/CommonActions/ContinueDialogueAction.asset",
                ExplainMapStep + "/ContinueDialogueAction.asset");
            MoveAssetIfPresent(Authoring + "/OpenMapDialogue.asset", OpenMapStep + "/OpenMapDialogue.asset");
            MoveAssetIfPresent(Authoring + "/OpenMapEntry.asset", OpenMapStep + "/OpenMapEntry.asset");
            MoveAssetIfPresent(
                Authoring + "/MapButtonIndication.asset",
                OpenMapStep + "/MapButtonIndication.asset");
            MoveAssetIfPresent(Authoring + "/OpenMapAction.asset", OpenMapStep + "/OpenMapAction.asset");

            DeleteFolderIfEmpty(Authoring + "/MapTutorialSequence/IntroEntry");
            DeleteFolderIfEmpty(Authoring + "/MapTutorialSequence");
            DeleteFolderIfEmpty(Authoring + "/CommonActions");
            DeleteFolderIfEmpty(Steps + "/01_ExplainMap");
            DeleteFolderIfEmpty(Steps + "/02_OpenMap");
        }

        private static void MoveAssetIfPresent(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null
                || AssetDatabase.LoadMainAssetAtPath(destinationPath) != null)
            {
                return;
            }

            var error = AssetDatabase.MoveAsset(sourcePath, destinationPath);
            if (!string.IsNullOrEmpty(error))
            {
                throw new System.InvalidOperationException(
                    $"Could not move tutorial example asset from '{sourcePath}' to '{destinationPath}': {error}");
            }
        }

        private static void DeleteFolderIfEmpty(string path)
        {
            if (AssetDatabase.IsValidFolder(path)
                && AssetDatabase.FindAssets(string.Empty, new[] { path }).Length == 0)
            {
                AssetDatabase.DeleteAsset(path);
            }
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

        private static void Set(Object target, string propertyName, float value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).floatValue = value;
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
            public ExampleContent(
                TutorialSequenceAsset sequence,
                DelayedTutorialHintAsset rewardHint,
                TutorialTargetAsset heroTarget,
                TutorialTargetAsset mapButtonTarget,
                TutorialTargetAsset mapStatusTarget,
                TutorialTargetAsset upgradeTarget,
                TutorialTargetAsset northPathTarget,
                TutorialTargetAsset southPathTarget,
                TutorialTargetAsset rewardTarget)
            {
                Sequence = sequence;
                RewardHint = rewardHint;
                HeroTarget = heroTarget;
                MapButtonTarget = mapButtonTarget;
                MapStatusTarget = mapStatusTarget;
                UpgradeTarget = upgradeTarget;
                NorthPathTarget = northPathTarget;
                SouthPathTarget = southPathTarget;
                RewardTarget = rewardTarget;
            }

            public TutorialSequenceAsset Sequence { get; }

            public DelayedTutorialHintAsset RewardHint { get; }

            public TutorialTargetAsset HeroTarget { get; }

            public TutorialTargetAsset MapButtonTarget { get; }

            public TutorialTargetAsset MapStatusTarget { get; }

            public TutorialTargetAsset UpgradeTarget { get; }

            public TutorialTargetAsset NorthPathTarget { get; }

            public TutorialTargetAsset SouthPathTarget { get; }

            public TutorialTargetAsset RewardTarget { get; }
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
