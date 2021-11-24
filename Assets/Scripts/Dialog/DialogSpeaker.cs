using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSpeaker : MonoBehaviour
{
    [SerializeField] private YarnProgram dialogScript;
    [SerializeField] private SpeakerData data;

    DialogManager manager;

    public string StartNode { get; private set; } = "Start";

    void Start()
    {

        manager = DialogManager.GetInstance();

        if (dialogScript != null)
        {
            //We add the character script to the Dialog Runner
            manager.AddScript(dialogScript);
        }
        else
        {
            Debug.LogWarning("No se ha asignado un dialogo a este personaje");
        }

        if (data != null) {
            //We add info about our speaker
            manager.AddSpeakerData(data);
            StartNode = data.SpeakerName + ".Start";
        }
    }
}
