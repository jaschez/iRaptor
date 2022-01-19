using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MenuInteraction : MonoBehaviour
{
    public Animator particlesAnim;

    public TextAnimator continueText;

    public Sequence continueSequence;

    MenuStates state = MenuStates.Intro;

    void Start()
    {
        continueSequence = continueText.Animate(TextAnimator.BounceIn(continueText)).SetLoops(-1, LoopType.Restart).SetDelay(1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && state == MenuStates.Intro)
        {
            state = MenuStates.Menu;

            particlesAnim.SetTrigger("start");
            continueSequence.Kill();
            continueText.Animate(TextAnimator.BounceOut(continueText)).onComplete = () => continueText.gameObject.SetActive(false);
        }
    }

    public enum MenuStates
    {
        Intro, Menu, Files, LoadGame
    }
}
