using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Rewired;

public class EventBasedDialogueController : MonoBehaviour, IEventListener
{
    private string lastEvent;
    public void Event(string eventName, params object[] args)
    {
        lastEvent = eventName;
        if (CancelEvent == eventName)
        {
            Reset();
        }
    }

    public SimpleDialogue Dialogue;

    public string StartupEvent;
    public string CancelEvent;

    public float ConfirmTimeToWait;

    public DialogueBox Box;

    public void OnEnable()
    {
        StartCoroutine(PerformDialogue(Dialogue));
    }

    public void Reset()
    {
        StopAllCoroutines();
        lastEvent = null;
        StartCoroutine(PerformDialogue(Dialogue));
    }

    private Coroutine WaitForEvent(string eventName)
    {
        return StartCoroutine(WaitForEventHelper(eventName));
    }

    private IEnumerator WaitForEventHelper(string eventName)
    {
        do
        {
            yield return null;
        } while (lastEvent != eventName);

        lastEvent = null;
    }

    public IEnumerator PerformDialogue(SimpleDialogue dialogue)
    {
        Box.Hide();
        yield return WaitForEvent(StartupEvent);

        Player player = ReInput.players.GetPlayer(0);
        Box.Show();

        foreach (SimpleDialogue.Blob blob in dialogue.Dialogue)
        {
            Box.SetConfirmTicker(false);
            switch (blob.Clear)
            {
            case SimpleDialogue.ClearType.Continue:
                Box.AppendMessage(blob.Message);
                break;
                
            default:
                Box.DisplayMessage(blob.DisplayedSpeaker, blob.Message);
                break;
            }
            
            switch(blob.Continue)
            {

            case SimpleDialogue.ContinueType.Action:
                yield return new WaitForSeconds(ConfirmTimeToWait);
                Box.SetConfirmTicker(true);
                do
                {
                    yield return null;
                } while (!player.GetButtonDown("Confirm"));
                break;
                
            case SimpleDialogue.ContinueType.Time:
                yield return new WaitForSeconds(blob.Time);
                break;

            case SimpleDialogue.ContinueType.Event:
                yield return WaitForEvent(blob.EventName);
                break;

            }
        }
        Box.Hide();
        Reset();
    }
}
