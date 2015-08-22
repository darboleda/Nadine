using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using Rewired;

public class TestDialogueBox : MonoBehaviour {

    public SimpleDialogue Dialogue;

    public Text SpeakerName;
    public Text Message;
    public GameObject ConfirmTicker;
    public GameObject DialogueBoxContainer;

    public float ConfirmTimeToWait;

    public void OnEnable()
    {
        StartCoroutine(PerformDialogue(Dialogue));
    }


    public IEnumerator PerformDialogue(SimpleDialogue dialogue)
    {
        Player player = ReInput.players.GetPlayer(0);

        DialogueBoxContainer.SetActive(true);

        foreach (SimpleDialogue.Blob blob in dialogue.Dialogue)
        {
            ConfirmTicker.SetActive(false);

            SpeakerName.text = blob.DisplayedSpeaker;

            switch (blob.Clear)
            {
            case SimpleDialogue.ClearType.Continue:
                Message.text += blob.Message;
                break;

            default:
                Message.text = blob.Message;
                break;
            }

            switch(blob.Continue)
            {
            
            case SimpleDialogue.ContinueType.Action:
            {
                yield return new WaitForSeconds(ConfirmTimeToWait);
                ConfirmTicker.SetActive(true);
                do
                {
                    yield return null;
                } while (!player.GetButtonDown("Confirm"));
                break;
            }

            case SimpleDialogue.ContinueType.Time:
            {

                yield return new WaitForSeconds(blob.Time);
                break;
            }

            default:
                break;
            }
        }

        DialogueBoxContainer.SetActive(false);
    }
}
