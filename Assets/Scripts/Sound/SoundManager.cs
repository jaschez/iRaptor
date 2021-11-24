using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static SoundManager instance;

    public AudioClip[] sounds;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static AudioSource Play(Sounds sound, Vector3 emittingPoint, Transform parent = null, bool destroy = true, bool playOnAwake = true)
    {
        return PlayClipAt(instance.sounds[(int)sound], emittingPoint, parent, destroy, playOnAwake);
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
}

public enum Sounds{
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
    PlayerImpact
}