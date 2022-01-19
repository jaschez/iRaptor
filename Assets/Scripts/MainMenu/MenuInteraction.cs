using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuInteraction : MonoBehaviour
{
    public GameObject logo;

    public Animator particlesAnim;
    public Animator planet;

    public TextAnimator continueText;
    public TextAnimator[] optionsMenu;

    public Sequence continueSequence;

    Vector3 camPos;

    MenuStates state = MenuStates.Intro;

    void Start()
    {
        continueSequence = continueText.BounceIn().SetLoops(-1, LoopType.Restart).SetDelay(1.5f);

        foreach (TextAnimator animator in optionsMenu)
        {
            animator.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        }

        camPos.z = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        camPos.x = Input.mousePosition.x / 800f;
        camPos.y = Input.mousePosition.y / 800f;

        if(state == MenuStates.Menu)
            transform.position = Vector3.Lerp(transform.position, camPos, Time.deltaTime*5);

        if (Input.GetMouseButtonDown(0) && state == MenuStates.Intro)
        {
            state = MenuStates.Menu;

            particlesAnim.SetTrigger("start");
            continueSequence.Kill();
            continueText.BounceOut().onComplete = () => continueText.gameObject.SetActive(false);

            logo.SetActive(false);

            Invoke("FadeInMenu", .5f);
        }
    }

    void FadeInMenu()
    {
        float delay = 0;
        foreach (TextAnimator animator in optionsMenu)
        {
            animator.gameObject.SetActive(true);
            animator.BounceIn().SetDelay(delay);

            animator.GetComponent<TextMeshProUGUI>().color = Color.white;

            delay += .1f;
        }
    }

    public void PressPlay()
    {
        particlesAnim.SetTrigger("play");
        planet.SetTrigger("start");

        float delay = 0;
        foreach (TextAnimator animator in optionsMenu)
        {
            animator.gameObject.SetActive(true);
            animator.BounceOut().SetDelay(delay)
                .OnComplete(() => animator.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0));

            animator.GetComponent<TextMeshProUGUI>().color = Color.white;

            delay += .1f;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public enum MenuStates
    {
        Intro, Menu, Files, LoadGame
    }
}
