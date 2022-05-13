using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewAudioAsset", menuName = "Audio/AudioAssets")]
public class AudioAssets : ScriptableObject
{
    [SerializeField]
    List<AudioAsset> AudioList;

    [SerializeField]
    public AudioMixer Mixer;

    [SerializeField]
    List<MixerAsset> MixerList;

    [SerializeField]
    List<VolumeParameter> VolumeParameters;

    public Dictionary<Sound, AudioClip> GenerateSoundDictionary()
    {
        Dictionary<Sound, AudioClip> GeneratedDictionary = new Dictionary<Sound, AudioClip>();

        foreach (AudioAsset asset in AudioList)
        {
            if (!GeneratedDictionary.ContainsKey(asset.SoundType))
            {
                GeneratedDictionary.Add(asset.SoundType, asset.Audio);
            }
        }

        return GeneratedDictionary;
    }

    public Dictionary<AudioMixerType, AudioMixerGroup> GenerateMixerDictionary()
    {
        Dictionary<AudioMixerType, AudioMixerGroup> GeneratedDictionary = new Dictionary<AudioMixerType, AudioMixerGroup>();

        foreach (MixerAsset asset in MixerList)
        {
            if (!GeneratedDictionary.ContainsKey(asset.MixerType))
            {
                GeneratedDictionary.Add(asset.MixerType, asset.Mixer);
            }
        }

        return GeneratedDictionary;
    }

    public Dictionary<AudioMixerType, string> GenerateVolumeParameterDictionary()
    {
        Dictionary<AudioMixerType, string> GeneratedDictionary = new Dictionary<AudioMixerType, string>();

        foreach (VolumeParameter asset in VolumeParameters)
        {
            if (!GeneratedDictionary.ContainsKey(asset.MixerType))
            {
                GeneratedDictionary.Add(asset.MixerType, asset.Parameter);
            }
        }

        return GeneratedDictionary;
    }

    [System.Serializable]
    public struct AudioAsset
    {
        public AudioClip Audio;
        public Sound SoundType;
    }

    [System.Serializable]
    public struct MixerAsset
    {
        public AudioMixerGroup Mixer;
        public AudioMixerType MixerType;
    }

    [System.Serializable]
    public struct VolumeParameter
    {
        public string Parameter;
        public AudioMixerType MixerType;
    }
}
