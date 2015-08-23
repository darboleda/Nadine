using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class DialogueBox : MonoBehaviour {

    public Text SpeakerName;
    public Text Message;
    public GameObject ConfirmTicker;
    public GameObject DialogueBoxContainer;

    public Animation Animation;
    public AnimationClip ShowClip;
    public AnimationClip HideClip;

    public void Show()
    {
        DialogueBoxContainer.SetActive(true);
        hiding = false;
        StopAllCoroutines();
        Animation.Play(ShowClip.name);
        //Animation.PlayQueued(ShowClip.name);
    }

    public void Hide()
    {
        if (hiding || !DialogueBoxContainer.activeSelf) return;
        StopAllCoroutines();
        StartCoroutine(AnimateHide());
    }

    private bool hiding;
    private IEnumerator AnimateHide()
    {
        hiding = true;
        yield return StartCoroutine(PlayAnimationAndWait(HideClip));
        DialogueBoxContainer.SetActive(false);
        hiding = false;
    }

    private IEnumerator PlayAnimationAndWait(AnimationClip animation)
    {
        /*
        Animation.PlayQueued(animation.name);
        while (!Animation.IsPlaying(animation.name))
        {
            yield return null;
        }
        */

        Animation.Play(animation.name);

        while (Animation.IsPlaying(animation.name))
        {
            yield return null;
        }

    }

    public void SetConfirmTicker(bool active)
    {
        ConfirmTicker.SetActive(active);
    }

    public void DisplayMessage(string speaker, string message)
    {
        if (string.IsNullOrEmpty(speaker))
        {
            SpeakerName.text = null;
        }
        else
        {
            SpeakerName.text = speaker;
        }

        Message.text = message;
    }

    public void AppendMessage(string message)
    {
        Message.text += message;
    }
}
