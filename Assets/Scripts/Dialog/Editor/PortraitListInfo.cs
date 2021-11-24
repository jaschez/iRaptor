using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpeakerData))]
public class PortraitListInfo : Editor
{
    SpeakerData speaker;
    string listInfo;

    void OnEnable()
    {
        speaker = target as SpeakerData;
    }

    private void OnValidate()
    {

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Speaker emotions:\n" + speaker.EmotionsInfo(), EditorStyles.textArea);

    }
}
