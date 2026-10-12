using System.ComponentModel;
using AMTutorialArchitecture.Tutorial.Application.ViewModels;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AMTutorialArchitecture.Tutorial.Unity.Bindings
{
    public sealed class TutorialViewBinding : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private RectTransform dialoguePanel;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text progressText;
        [SerializeField] private Button continueButton;

        [Header("Sequence")]
        [SerializeField] private Button skipButton;

        private TutorialViewModel viewModel;
        private bool subscribed;

        [Inject]
        public void Construct(TutorialViewModel tutorialViewModel)
        {
            viewModel = tutorialViewModel;
            SubscribeIfPossible();
            Refresh();
        }

        private void OnEnable()
        {
            SubscribeIfPossible();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void SubscribeIfPossible()
        {
            if (subscribed || viewModel == null || !isActiveAndEnabled)
            {
                return;
            }

            viewModel.PropertyChanged += OnViewModelChanged;
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            viewModel.PropertyChanged -= OnViewModelChanged;
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(OnSkipClicked);
            }

            subscribed = false;
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs args)
        {
            Refresh();
        }

        private void OnContinueClicked()
        {
            viewModel.ContinueCommand.Execute();
        }

        private void OnSkipClicked()
        {
            viewModel.SkipCommand.Execute();
        }

        private void Refresh()
        {
            if (viewModel == null)
            {
                return;
            }

            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(viewModel.HasDialogue);
            }

            if (speakerText != null)
            {
                speakerText.text = viewModel.SpeakerId;
            }

            if (bodyText != null)
            {
                bodyText.text = viewModel.DialogueText;
            }

            if (progressText != null)
            {
                progressText.text = viewModel.IsActive
                    ? $"{viewModel.CurrentEntryNumber}/{viewModel.EntryCount}"
                    : string.Empty;
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(viewModel.CanContinueDialogue);
                continueButton.interactable = viewModel.ContinueCommand.CanExecute();
            }

            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(viewModel.CanSkip);
                skipButton.interactable = viewModel.SkipCommand.CanExecute();
            }

            ApplyDialogueSide(viewModel.DialogueSide);
        }

        private void ApplyDialogueSide(TutorialDialogueSide side)
        {
            if (dialoguePanel == null)
            {
                return;
            }

            var onLeft = side == TutorialDialogueSide.Left;
            dialoguePanel.anchorMin = new Vector2(onLeft ? 0f : 1f, dialoguePanel.anchorMin.y);
            dialoguePanel.anchorMax = new Vector2(onLeft ? 0f : 1f, dialoguePanel.anchorMax.y);
            dialoguePanel.pivot = new Vector2(onLeft ? 0f : 1f, dialoguePanel.pivot.y);
            var position = dialoguePanel.anchoredPosition;
            dialoguePanel.anchoredPosition = new Vector2(onLeft ? Mathf.Abs(position.x) : -Mathf.Abs(position.x), position.y);
        }
    }
}
