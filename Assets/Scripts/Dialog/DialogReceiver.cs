using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogReceiver : MonoBehaviour
{

    DialogManager manager;

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
}
