using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DialogSpeaker))]
public class NPC : MonoBehaviour, Interactable
{
    DialogManager manager;
    DialogSpeaker speaker;

    // Start is called before the first frame update
    void Start()
    {
        manager = DialogManager.GetInstance();
        speaker = GetComponent<DialogSpeaker>();
    }

    public void Interact()
    {
        if (!manager.IsInDialog())
        {
            manager.StartDialogue(speaker.StartNode);
        }
    }
}
