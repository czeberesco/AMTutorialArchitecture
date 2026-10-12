using System;

namespace AMTutorialArchitecture.Tutorial.Domain.Definitions
{
    public sealed class DialogueDefinition
    {
        public DialogueDefinition(string speakerId, string text, TutorialDialogueSide side)
        {
            SpeakerId = speakerId ?? string.Empty;
            Text = text ?? string.Empty;
            Side = side;
        }

        public string SpeakerId { get; }

        public string Text { get; }

        public TutorialDialogueSide Side { get; }
    }
}
