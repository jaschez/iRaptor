using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Introduction : MonoBehaviour
{

    public AudioSource soundIn;
    public AudioSource soundOut;

    public TextAnimator text;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        text.BounceIn(1).SetDelay(.3f).DOTimeScale(.8f, 1);
        text.BounceOut(1f).SetDelay(2.8f).OnComplete(() => {
            text.gameObject.SetActive(false);
            SoundManager.FadeMixerVolume(AudioMixerType.None, 0, .5f);
            Invoke("Load", .5f);
        });

        Invoke("PlaySounds", 2.8f);
    }

    void Load()
    {
        SceneSystem.LoadScene(SceneSystem.GameScenes.MainMenu);
    }

    void PlaySounds()
    {
        //soundOut.Play();
        soundIn.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
