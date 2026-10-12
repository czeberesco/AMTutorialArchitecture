using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AMTutorialArchitecture.Tutorial.Application.Services;
using AMTutorialArchitecture.Tutorial.Domain.Actions;
using AMTutorialArchitecture.Tutorial.Domain.Definitions;
using AMTutorialArchitecture.Tutorial.Domain.Model;

namespace AMTutorialArchitecture.Tutorial.Application.ViewModels
{
    public sealed class TutorialViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ITutorialService service;
        private TutorialSnapshot snapshot;
        private IReadOnlyList<string> targetIds;
        private bool disposed;

        public TutorialViewModel(ITutorialService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            snapshot = service.Current;
            targetIds = GetTargetIds(snapshot);
            ContinueCommand = new RelayCommand(ContinueDialogue, () => CanContinueDialogue);
            SkipCommand = new RelayCommand(Skip, () => CanSkip);
            service.StateChanged += OnStateChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand ContinueCommand { get; }

        public RelayCommand SkipCommand { get; }

        public bool IsActive => snapshot.IsActive;

        public bool HasDialogue => snapshot.CurrentEntry?.HasDialogue == true;

        public bool HasIndication => snapshot.CurrentEntry?.HasIndication == true;

        public bool CanContinueDialogue => HasDialogue && !HasIndication;

        public bool CanSkip => snapshot.CanSkip;

        public string SequenceId => snapshot.Sequence?.Id ?? string.Empty;

        public string EntryId => snapshot.CurrentEntry?.Id ?? string.Empty;

        public int CurrentEntryNumber => snapshot.IsActive ? snapshot.CurrentEntryIndex + 1 : 0;

        public int EntryCount => snapshot.EntryCount;

        public string SpeakerId => snapshot.CurrentEntry?.Dialogue?.SpeakerId ?? string.Empty;

        public string DialogueText => snapshot.CurrentEntry?.Dialogue?.Text ?? string.Empty;

        public TutorialDialogueSide DialogueSide => snapshot.CurrentEntry?.Dialogue?.Side
            ?? TutorialDialogueSide.Left;

        public IReadOnlyList<string> TargetIds => targetIds;

        public TutorialInteractionPolicy InteractionPolicy => snapshot.CurrentEntry?.InteractionPolicy
            ?? TutorialInteractionPolicy.NonBlocking;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            service.StateChanged -= OnStateChanged;
            disposed = true;
        }

        private void ContinueDialogue()
        {
            service.SubmitAction(TutorialActionOccurrence.Success(TutorialActionIds.ContinueDialogue));
        }

        private void Skip()
        {
            service.Skip();
        }

        private void OnStateChanged(object sender, TutorialStateChangedEventArgs args)
        {
            snapshot = args.Current;
            targetIds = GetTargetIds(snapshot);
            RaiseAllPropertiesChanged();
        }

        private static IReadOnlyList<string> GetTargetIds(TutorialSnapshot current)
        {
            return current.CurrentEntry?.Indications
                .Where(indication => indication != null)
                .Select(indication => indication.TargetId)
                .ToArray()
                ?? Array.Empty<string>();
        }

        private void RaiseAllPropertiesChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            ContinueCommand.RaiseCanExecuteChanged();
            SkipCommand.RaiseCanExecuteChanged();
        }
    }
}
