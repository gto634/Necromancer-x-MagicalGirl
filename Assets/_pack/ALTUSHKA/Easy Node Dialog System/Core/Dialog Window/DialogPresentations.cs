using System.Collections.Generic;
using UnityEngine;

namespace ALTUSHKA.EasyNodeDialogSystem
{
    public enum DialogCloseReason
    {
        Completed = 0,
        Cancelled = 1
    }

    public sealed class DialogSessionInfo
    {
        public string DialogName;
        public string NodeId;
    }

    public sealed class DialogLinePresentation
    {
        public string DialogName;
        public string NodeId;
        public string SpeakerKey;
        public string SpeakerDisplayName;
        public string Text;
        public string ColorHex;
        public AudioClip VoiceClip;
        public bool IsInstant;
        public IReadOnlyList<CharacterActionData> CharacterActions;
    }

    public sealed class DialogChoicePresentation
    {
        public int Index;
        public string Text;
    }

    public enum DialogDiagnosticKind
    {
        StartFailed = 0,
        MissingNode = 1,
        MissingLineData = 2,
        EmptyChoices = 3,
        InvalidRedirect = 4,
        MissingReference = 5,
        InvalidCondition = 6
    }

    /// <summary>Facts only (kind + context). Human text is built by DialogDebugger.</summary>
    public sealed class DialogDiagnosticReport
    {
        public DialogDiagnosticKind Kind;
        public string Context;
    }
}
