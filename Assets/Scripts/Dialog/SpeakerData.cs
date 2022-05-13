using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Speaker")]
public class SpeakerData : ScriptableObject
{

    [SerializeField] private string speakerName;
    [SerializeField] private Sound talkingSound = Sound.None;
    [SerializeField] private EmotionPortrait[] emotionPortraits;

    private Dictionary<string, Sprite> speakerEmotions;

    public string SpeakerName { get { return speakerName; } }

    //Generate emotions dictionary
    public void Setup()
    {
        if (speakerEmotions == null) {

            speakerEmotions = new Dictionary<string, Sprite>();

            foreach (EmotionPortrait emotion in emotionPortraits)
            {
                if (!speakerEmotions.ContainsKey(emotion.emotionType.ToString()))
                {
                    speakerEmotions.Add(emotion.emotionType.ToString(), emotion.portrait);
                }
            }
        }
    }

    public Sprite GetSpeakerSprite(string emotion)
    {
        if (speakerEmotions.TryGetValue(emotion, out Sprite s)) {
            return s;
        }
        else
        {
            Debug.LogError("No se ha encontrado el sprite para el estado de ánimo " + emotion);
            return null;
        }
    }

    public Sound GetTalkingSound()
    {
        return talkingSound;
    }

    public string EmotionsInfo()
    {
        string info = "{";

        if (emotionPortraits != null) {
            foreach (EmotionPortrait e in emotionPortraits)
            {
                info += e.emotionType.ToString() + ", ";
            }
        }

        info += "}";

        return info;
    }

    [Serializable]
    struct EmotionPortrait
    {
        public Emotion emotionType;
        public Sprite portrait;
    }

    public enum Emotion
    {
        neutral, happy, sad
    }
}
