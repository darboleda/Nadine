using UnityEngine;
using System;
using System.Collections.Generic;

public class SimpleDialogue : ScriptableObject
{
    public enum ContinueType
    {
        Event,
        Action,
        Time,
        Option
    }

    public enum ClearType
    {
        Continue,
        SameBox,
        NewWindow
    }

    [Serializable]
    public class Blob
    {
        public ClearType Clear = ClearType.NewWindow;
        public int SpeakerId;
        public string DisplayedSpeaker;
        public string Message;
        public ContinueType Continue = ContinueType.Action;
        public string EventName;
        public float Time;
        public string[] Options;

        public SimpleDialogue Dialogue;
    }

    public DialogueSpeakers SpeakerIds;
    public List<Blob> Dialogue;
}
