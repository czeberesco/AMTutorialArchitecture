# Repository Instructions

## Purpose

This repository is a proof of architecture for a scalable in-game tutorial system in Unity. The quality of the architectural boundaries, documentation, tests, and reasoning is as important as the visible demo.

Implement the smallest coherent solution that demonstrates the architecture. Do not add production-scale infrastructure without a concrete requirement.

## Repository Layout

- The Git repository root contains this file and the main `README.md`.
- The Unity project is in `AMTutorialArchitecture/`.
- Read `AMTutorialArchitecture/ProjectSettings/ProjectVersion.txt` before selecting an editor.
- The current project version is Unity `6000.3.10f1`.
- Do not edit or commit generated directories such as `Library/`, `Temp/`, `Logs/`, or `UserSettings/`.
- Keep Unity `.meta` files together with their assets.
- Under `Assets/TutorialSystem/`, runtime code is organized by the supplied architecture: `Model/`, `ViewModel/`, `Binding/`, and `View/`. Keep support-only `Editor/`, `Example/`, and `Tests/` folders separate from those runtime layers.
- `Model/Runtime` and `ViewModel/Runtime` are pure C# assemblies with `noEngineReferences`. `Model/Authoring` is the Unity ScriptableObject conversion boundary. `View` contains concrete presentation components. `Binding` coordinates ViewModels, Views, input, target resolution, gameplay adapters, and Extenject composition.
- Do not recreate a generic `Runtime/Unity`, `Runtime/Domain`, or `Runtime/Application` hierarchy; place new code in the architectural layer that owns the responsibility.

## Architectural Context

The supplied application architecture has four client layers:

1. **Model layer**: models and shared command logic.
2. **ViewModel layer**: receives model messages, exposes observable presentation state, and issues commands.
3. **Binding layer**: translates View events into ViewModel commands and ViewModel property changes into View updates.
4. **View layer**: Unity-specific presentation and input components.

Commands are sent to shared logic and produce command results. Model changes are communicated to ViewModels as messages. Preserve this direction of communication; Views must not mutate tutorial domain state directly.

Shared logic must remain compatible with execution outside Unity. Do not introduce `UnityEngine` dependencies into shared/domain assemblies. Unity authoring assets must be converted into pure immutable definitions before they are consumed by shared logic.

## Tutorial Domain

- A tutorial is an ordered sequence of tutorial entries.
- An entry contains an optional dialogue and zero or more indications. At least a dialogue or one indication must be present.
- Every entry has one explicit required action that determines completion.
- A long dialogue is represented by multiple entries. When it leads to an indicated action, only the last dialogue entry also contains the indication group.
- A dialogue-plus-indication entry completes through its required indicated action, not merely by dismissing the dialogue.
- Activating an indicated element still executes that element's normal game behavior.
- Tutorial progression should normally occur only after the expected semantic action succeeds. A failed gameplay command must not accidentally advance the tutorial.
- A tutorial must not invoke feature-specific View behavior directly. It observes semantic commands, results, or domain signals.
- Presentation target identity and completion-action identity are separate concepts, even when they refer to the same user interaction.
- Runtime progress must identify the active sequence and exact entry so the tutorial can resume at the same point.
- Skipping applies to the remaining sequence, never to only one entry. Skip availability is configured per sequence and is meaningful only for sequences longer than one entry.
- Delayed inactivity hints are client presentation behavior outside the deterministic sequence state machine. They reuse semantic targets and View-owned highlighting, reset on reported player activity, and must not mutate tutorial progress.

## Blocking Policy

- Shared logic decides whether an active indication group is `Blocking` or `NonBlocking` and includes that policy in its command result/presentation state.
- The client decides how to implement the policy with Unity input and UI facilities.
- A blocking entry permits all indicated interactions and must not soft-lock permanently when none of its targets can be resolved to an actionable control.
- A non-blocking entry remains logically active when the player navigates away. Individual presentations hide while their targets are unavailable and return when matching targets register again.
- Losing a target or changing screens does not complete an entry.

## ScriptableObject Authoring

Use ScriptableObjects as immutable authoring data, not as player-specific runtime state.

The intended authoring model contains:

- `TutorialSequenceAsset`: ordered entries and sequence-level configuration.
- `TutorialEntryAsset`: optional dialogue, zero or more indications, one entry-level interaction policy, and one required action.
- `DialogueAsset`: localized dialogue content, speaker identity, and presentation placement data required by the design.
- `IndicationAsset`: the semantic tutorial target only; it contains no highlight style.
- `RequiredActionAsset`: a semantic completion condition and the source of its own stable identity, not a `UnityEvent`, delegate, Button reference, scene callback, or manually maintained string key.
- `TutorialTargetAsset`: a stable target identity shared by authoring assets and View components.
- `DelayedTutorialHintAsset`: client-side inactivity duration and semantic targets; it contains no highlight style or runtime timer state.

Convert authoring assets into pure C# definitions/DTOs at the Unity boundary. Runtime models and saved progress must not mutate ScriptableObjects.

Unity-facing components identify required actions by referencing the `RequiredActionAsset` itself. The asset's Unity GUID is automatically baked into hidden serialized data and converted to a plain stable ID at the Unity/domain boundary. Never use `GetInstanceID`, the asset name, or a manually authored action-ID string: instance IDs are session-local, names are mutable, and Unity object references cannot cross the shared/client-server boundary.

Avoid direct references from tutorial assets to scene GameObjects or transient UI instances. Views register stable target identities with a target registry when they become available and unregister them when they are disabled or destroyed.

Add editor-time validation where possible and runtime diagnostics where scene state can only be known at runtime. Invalid content should report the sequence ID, entry ID, and target/action ID involved.

## Required Actions

Required actions describe semantic outcomes. Expected examples include:

- Continue a dialogue.
- Successfully execute a named command.
- Complete navigation to a screen.
- Receive a domain signal.
- Activate a semantic target when activation itself is the intended outcome.

Prefer strongly typed or stable-ID contracts over matching C# type names or scene hierarchy paths. For authored required actions, use the `RequiredActionAsset` reference in Unity and its baked asset GUID in pure logic, persistence, or transport. Keep the design open to additional action predicates without adding feature-specific branches to the tutorial state machine.

## Dependency Injection and Service Entry Point

Use Extenject as the Unity composition root for the tutorial feature.

- Expose tutorial use cases through an injected `ITutorialService`; do not access the tutorial through a static singleton.
- Keep `ITutorialService`, its implementation, and its dependencies independent of Extenject APIs. Only installers and Unity composition code may reference the container.
- Prefer constructor injection for plain C# classes. Use Extenject method/field injection only at Unity-owned boundaries such as `MonoBehaviour` components.
- Do not inject `DiContainer`, call `Resolve` from application code, or use Extenject as a service locator.
- Bind the tutorial service with an application-level lifetime so active progress can survive scene changes. Scene-level presentation objects and target registrations remain scene-scoped.
- The core service entry point accepts a pure `TutorialSequenceDefinition` (or equivalent immutable authored-data DTO), not a Unity object.
- Provide a Unity-side convenience adapter or extension so gameplay code can call the service with a `TutorialSequenceAsset`. That adapter validates and converts the asset before invoking the pure service contract.
- Starting a sequence is an explicit application operation. It must return a meaningful result for invalid content, an already-active sequence, or a conflicting active sequence; do not silently replace tutorial progress.
- Extenject Signals may be used at the client composition boundary when useful, but domain logic and tests must not depend on Extenject's signal types.

A predefined gameplay location may start a tutorial by receiving `ITutorialService` through injection and passing its configured sequence asset to the Unity adapter. The eventual example scene may demonstrate this with a simple trigger or button, but feature-specific start conditions are outside the tutorial service.

## Target Binding and Highlighting

The tutorial domain chooses **what** receives attention. The resolved target View decides **how** attention is presented.

- Do not put highlight styles, colors, animation types, masks, pointer prefabs, or effect enums in tutorial authoring data, domain models, or ViewModels.
- A target View may optionally expose a small highlight capability such as `ShowHighlight`/`HideHighlight`.
- Concrete Views may use completely different implementations: animation, outline, shader, VFX, pointer, overlay cutout, or another presentation.
- Keep target capabilities focused. Target identity, highlighting, anchoring/bounds, and activation observation should not be forced into one large interface.
- Binding code resolves every active semantic target, detects supported capabilities, shows each presentation, and cleans up the previous presentations.
- Highlight calls must be idempotent and cleanup must handle entry changes, disabled Views, scene unloads, and target re-registration.
- If an indicated target resolves but has no highlight implementation, emit a clear warning once for that binding attempt. Missing highlighting is a client presentation/configuration problem, not a shared-domain failure.

## ViewModel and Binding Rules

- ViewModels expose presentation state and commands without storing references to GameObjects, Components, Transforms, or other Unity Views.
- Binding components contain Unity-specific wiring and resolve live target Views through the registry.
- Views own visual configuration and animations.
- Do not place tutorial progression rules in Views, binding components, or animation callbacks.
- Unsubscribe event handlers and release bindings deterministically.

## Testing Expectations

Prioritize EditMode tests for pure logic:

- Definition validation.
- Entry and sequence transitions.
- Dialogue-only, indication-only, and combined entries.
- Required-action matching and unsuccessful-action behavior.
- Blocking/non-blocking state behavior.
- Skip, interruption, persistence, and exact-entry restoration.

Use focused Unity tests for integration behavior:

- ScriptableObject-to-definition conversion.
- Target registration and resolution.
- Binding and rebinding as Views appear and disappear.
- Highlight capability invocation and cleanup.
- Missing-capability warnings.
- Input-policy integration where practical.
- Delayed-hint timing, display, cleanup, and activity reset behavior.

Tests should verify observable behavior rather than private implementation details.

## Unity Batch Validation

Use Unity `6000.3.10f1`, as recorded in `ProjectVersion.txt`. Resolve the matching editor executable from the development or CI environment; do not assume or document a machine-specific installation path.

The Unity project path is the repository-relative `AMTutorialArchitecture/` directory, not the Git repository root.

At minimum, run a batch compile/import check after meaningful changes:

```powershell
& $UnityEditorExecutable `
  -batchmode -nographics -quit `
  -projectPath 'AMTutorialArchitecture' `
  -logFile 'AMTutorialArchitecture/Logs/BatchCompile.log'
```

In the example above, `$UnityEditorExecutable` must be supplied by the local development or CI environment and must resolve to Unity `6000.3.10f1`.

Run EditMode and PlayMode tests in batch mode when their corresponding test suites exist, and save XML results under `AMTutorialArchitecture/Logs/`.

Inspect the Unity log and process exit code. A successful shell launch alone is not sufficient validation. Report compilation errors, failed tests, and relevant warnings explicitly.

Unity Package Manager and licensing helpers require local IPC. In restricted execution environments, batch validation may require approved elevated execution. Do not treat an IPC startup failure as a project compilation failure until Unity has been retried in an environment where its helper processes can run.

## Documentation

Keep the root `README.md` focused on explaining the architecture to a reviewer. As the implementation develops, document:

- Layer/package responsibilities and dependency direction.
- ScriptableObject authoring to pure-runtime-definition conversion.
- Command/result and tutorial-progression flow.
- Tutorial entry state machine.
- Target registration, binding, and View-owned highlighting.
- Blocking and non-blocking behavior.
- Concrete examples of dialogue-plus-indication, multi-entry dialogue, indication-only, multi-target indication, and delayed inactivity hints.
- How to open, run, and test the proof of architecture.

Use Mermaid diagrams so the schematics remain reviewable and version-controlled as text. Keep diagrams consistent with the implementation.
