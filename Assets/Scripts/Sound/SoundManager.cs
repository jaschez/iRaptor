using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    static SoundManager instance;

    public AudioAssets audioAssets;

    AudioMixer mixer;

    Dictionary<Sound, AudioClip> soundDictionary;
    Dictionary<AudioMixerType, AudioMixerGroup> mixerDictionary;
    Dictionary<AudioMixerType, string> volumeDictionary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Destroy(gameObject);
        }

        soundDictionary = audioAssets.GenerateSoundDictionary();
        mixerDictionary = audioAssets.GenerateMixerDictionary();
        volumeDictionary = audioAssets.GenerateVolumeParameterDictionary();
        mixer = audioAssets.Mixer;
    }

    public static AudioSource Play(Sound sound, Vector3 emittingPoint, Transform parent = null, bool destroy = true, bool playOnAwake = true)
    {
        return PlayClipAt(instance.soundDictionary[sound], emittingPoint, parent, destroy, playOnAwake);
    }

    public static SoundManager GetInstance()
    {
        return instance;
    }

    static AudioSource PlayClipAt(AudioClip clip, Vector3 pos, Transform parent = null, bool destroy = true, bool playOnAwake = true)
    {
        GameObject tempGO = new GameObject("One Shot Audio");
        AudioSource aSource = tempGO.AddComponent<AudioSource>();

        tempGO.transform.position = pos;

        if (parent != null)
        {
            tempGO.transform.SetParent(parent);
        }

        aSource.clip = clip;
        aSource.outputAudioMixerGroup = instance.mixerDictionary[AudioMixerType.GameSound];

        aSource.playOnAwake = playOnAwake;

        if (playOnAwake)
        {
            aSource.Play();
        }

        if (destroy)
        {
            Destroy(tempGO, clip.length);
        }

        return aSource;
    }

    public static void SetMixerVolume(AudioMixerType mixer, float volume)
    {
        volume = MapVolume(volume);

        instance.mixer.SetFloat(instance.volumeDictionary[mixer] ,Mathf.Log10(volume) * 20f);
    }

    public static void FadeMixerVolume(AudioMixerType mixer, float endValue, float time)
    {
        foreach(AudioSource audio in FindObjectsOfType<AudioSource>())
        {
            if (mixer != AudioMixerType.None)
            {
                if (audio.outputAudioMixerGroup != null) {
                    if (audio.outputAudioMixerGroup.GetHashCode() == instance.mixerDictionary[mixer].GetHashCode())
                    {
                        FadeVolume(audio, endValue, time);
                    }
                }
            }
            else
            {
                FadeVolume(audio, endValue, time);
            }
        } 
    }

    static float MapVolume(float volume)
    {
        volume = Mathf.Max(0.0001f, volume);
        volume = Mathf.Min(volume, 1f);

        return volume;
    }

    public static void FadeVolume(AudioSource audio, float volume, float time)
    {
        volume = Mathf.Max(0.0001f, volume);
        volume = Mathf.Min(volume, 1f);

        audio.DOFade(volume, time);
    }
}

public enum Sound{
    Shoot,
    ShipDeath,
    ChargePowerup,
    ActivatePU,
    Dash,
    Pump,
    Break,
    Drop,
    EngineLoop,
    EngineStart,
    EngineOff,
    RunOut,
    NoEnergy,
    EnemyImpact,
    PlayerImpact,
    Audio_ShipAmbience,
    Numbrian_alert,
    Numbrian_talk1,
    Numbrian_damage,
    Numbrian_death,
    None
}

public enum AudioMixerType
{
    Music,
    Sound,
    GameMusic,
    GameSound,
    Master,
    None
}