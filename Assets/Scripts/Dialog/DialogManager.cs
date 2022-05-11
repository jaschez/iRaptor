using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

[RequireComponent(typeof(DialogueRunner), typeof(DialogueUI))]
public class DialogManager : MonoBehaviour
{

    public TextMeshProUGUI speakerNameText;
    public Image speakerImage;

    DialogueRunner dialogueRunner;
    DialogueUI dialogueUI;

    Dictionary<string, SpeakerData> speakers;

    static DialogManager instance;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
        }

        speakers = new Dictionary<string, SpeakerData>();

        dialogueRunner = GetComponent<DialogueRunner>();
        dialogueUI = GetComponent<DialogueUI>();

        dialogueRunner.AddCommandHandler("SetSpeaker", SetSpeaker);
    }

    public void StartDialogue(string startNode)
    {
        dialogueRunner.StartDialogue(startNode);
    }

    public bool IsInDialog()
    {
        return dialogueRunner.IsDialogueRunning;
    }

    public void AddSpeakerData(SpeakerData speaker)
    {
        if (!speakers.ContainsKey(speaker.SpeakerName))
        {
            speaker.Setup();
            speakers.Add(speaker.SpeakerName, speaker);
        }
    }

    public void AddScript(YarnProgram script)
    {
        dialogueRunner.Add(script);
    }

    // Update is called once per frame
    void Update()
    {

        if (Controls.GetAttackKeyDown())
        {
            if(IsInDialog())
                dialogueUI.MarkLineComplete();
        }
    }

    void SetSpeaker(string[] param)
    {
        string speakerName = param[0];
        string emotion = param.Length > 1? param[1] : SpeakerData.Emotion.neutral.ToString();

        if (speakers.TryGetValue(speakerName, out SpeakerData data))
        {
            speakerNameText.text = speakerName;
            speakerImage.sprite = data.GetSpeakerSprite(emotion);
        }
        else
        {
            Debug.LogError("Error: No se ha encontrado el speaker");
        }
    }

    public static DialogManager GetInstance()
    {
        return instance;
    }
}
