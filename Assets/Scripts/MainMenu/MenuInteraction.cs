using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MenuInteraction : MonoBehaviour
{
    public ParticleSystem backgroundParticles;

    MenuStates state = MenuStates.Intro;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && state == MenuStates.Intro)
        {
            state = MenuStates.Menu;
            StartCoroutine(StopBackgroundNoise());
        }
    }

    IEnumerator StopBackgroundNoise()
    {
        float endTime = Time.time + 2f;
        var noise = backgroundParticles.noise;
        while (endTime < Time.time)
        {
            //noise.strength.constant// Mathf.Lerp(noise.strength.constant, 0, Time.deltaTime*4f);
            yield return null;
        }

        yield return null;
    }

    public enum MenuStates
    {
        Intro, Menu, Files, LoadGame
    }
}
