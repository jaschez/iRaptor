using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogReceiver : MonoBehaviour
{

    DialogManager manager;

    DialogSpeaker interactableNPC;
    DialogSpeaker tempNPC;

    [SerializeField] private GameObject interactSign;
    [SerializeField] private SpeakerData data;

    private void Start()
    {
        manager = DialogManager.GetInstance();

        if (data != null)
        {
            //We add info about our speaker
            manager.AddSpeakerData(data);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!manager.IsInDialog() && interactableNPC)
            {
                manager.StartDialogue(interactableNPC.StartNode);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out interactableNPC))
        {
            interactSign.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out tempNPC))
        {
            if (tempNPC == interactableNPC)
            {
                interactSign.SetActive(false);
                interactableNPC = null;
            }
        }
    }
}
