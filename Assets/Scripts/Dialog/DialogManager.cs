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
    public TextMeshProUGUI dialogText;

    public Image speakerImage;

    DialogueRunner dialogueRunner;
    DialogueUI dialogueUI;

    Dictionary<string, SpeakerData> speakers;

    Sound talkingSound = Sound.None;

    int remainingNewLines = 0;
    string remainingText = "";

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
        dialogueRunner.AddCommandHandler("ShowConsole", ShowConsole);
        dialogueRunner.AddCommandHandler("SetSpeed", SetSpeed);
        dialogueRunner.AddCommandHandler("SkipLines", SkipLines);
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

    //COMMANDS
    void SetSpeaker(string[] param)
    {
        string speakerName = param[0];
        string emotion = param.Length > 1? param[1] : SpeakerData.Emotion.neutral.ToString();

        if (speakers.TryGetValue(speakerName, out SpeakerData data))
        {
            if (speakerNameText != null && speakerImage != null) {
                speakerNameText.text = speakerName;
                speakerImage.sprite = data.GetSpeakerSprite(emotion);
            }

            talkingSound = data.GetTalkingSound();
        }
        else
        {
            Debug.LogError("Error: No se ha encontrado el speaker");
        }
    }

    void ShowConsole(string[] param)
    {
        MenuInteraction.GetInstance()?.ShowConsole();
    }

    void SetSpeed(string[] param)
    {
        float speed = 1f / int.Parse(param[0]);
        dialogueUI.textSpeed = speed;
    }

    void SkipLines(string[] param)
    {
        remainingNewLines = int.Parse(param[0]);
    }

    public void UpdateText(string text)
    {
        char lineBreak = '¬';
        string updatedText = text.Replace(lineBreak,'\n');

        string[] lines = updatedText.Split('\n');
        int maxLines = 10;

        if (lines.Length - maxLines > 0)
        {
            int startIndex = updatedText.Length;

            for (int i = lines.Length - 1; i > lines.Length - maxLines; i--)
            {
                startIndex -= lines[i].Length;
            }

            updatedText = updatedText.Substring(startIndex);
        }

        dialogText.text = remainingText + updatedText;

        if (updatedText.LastIndexOf(' ') != updatedText.Length - 1)
        {
            PlaySound();
        }
    }

    public void CheckLineSkip()
    {
        if (remainingNewLines > 0)
        {
            remainingNewLines--;
            remainingText = dialogText.text+"\n";
            dialogueUI.MarkLineComplete();
        }
        else
        {
            remainingText = "";
        }
    }

    void PlaySound()
    {
        if (talkingSound != Sound.None) {
            SoundManager.Play(talkingSound, Camera.main.transform.position).volume = .4f;
        }
    }

    public static DialogManager GetInstance()
    {
        return instance;
    }
}
