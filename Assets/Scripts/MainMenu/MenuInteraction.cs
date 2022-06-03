using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuInteraction : MonoBehaviour
{
    static MenuInteraction instance;

    public Image logo;
    public Image logoMenu;
    public Image blackBg;
    public Image consoleBg;

    public Animator particlesAnim;
    public Animator planet;

    public ParallaxMenu parallax;

    public TextAnimator continueText;

    public TextAnimator[] optionsMenu;
    public TextAnimator[] playMenu;

    public Sequence continueSequence;

    public GameObject firstMenu;

    GameObject currentMenu;
    TextAnimator[] currentMenuAnimators;

    Material IntegrateMat;
    Material IntegrateMatMenu;

    TransitionSystem transitionSystem;

    Vector3 camPos;

    MenuStates state = MenuStates.Intro;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(this);
            }
        }
    }

    void Start()
    {
        transitionSystem = TransitionSystem.GetInstance();
        transitionSystem.SetTransitionColor(Color.white);
        transitionSystem.Apply(TransitionSystem.Transition.FadeIn, .2f);

        InitializeButtonTexts();
        
        continueSequence = continueText.BounceIn().SetLoops(-1, LoopType.Restart).SetDelay(1.5f);

        IntegrateMat = logo.material;
        IntegrateMatMenu = logoMenu.material;
        IntegrateMat.SetFloat("_Fade", 1);
        IntegrateMatMenu.SetFloat("_Fade", 0);

        camPos.z = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0) && state == MenuStates.Intro)
        {
            state = MenuStates.Menu;

            particlesAnim.SetTrigger("start");
            continueSequence.Kill();
            continueText.BounceOut().onComplete = () => continueText.gameObject.SetActive(false);

            parallax.EnableParallax(true);

            StartCoroutine(FadeTitle());
        }
    }

    void InitializeButtonTexts()
    {
        foreach (TextAnimator animator in optionsMenu)
        {
            animator.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        }
    }

    public void SwitchMenu(GameObject menu)
    {
        StartCoroutine(SwitchMenuAnimation(menu));
    }

    public void DissolveMenu()
    {
        FadeOutMenu(currentMenuAnimators);
        IntegrateMatMenu.DOFloat(0, "_Fade", .5f);
        parallax.EnableParallax(false);
        planet.SetTrigger("arrival");
        particlesAnim.SetTrigger("arrival");

        Tweener shake = planet.gameObject.transform.DOShakePosition(6f, .4f, 30);
        shake.Goto(6f);
        shake.PlayBackwards();

        SoundManager.FadeMixerVolume(AudioMixerType.None, 0, 3f);

        blackBg.DOFade(1, 2).SetDelay(3).OnComplete(()=>
        {
            DialogManager.GetInstance().StartDialogue("Console.Start");
        });

        SavingSystem.NewSave();
    }

    public void LoadGame()
    {
        SavingSystem.Initialize();
        TransitionToLobby();
    }

    void TransitionToLobby()
    {
        transitionSystem.SetTransitionColor(Color.black);
        transitionSystem.SwitchToScene(SceneSystem.GameScenes.Lobby, TransitionSystem.Transition.FadeOut, .5f);
        SoundManager.FadeMixerVolume(AudioMixerType.None, 0, .8f);
    }

    IEnumerator SwitchMenuAnimation(GameObject menu)
    {
        TextAnimator[] animators = GetTextAnimatorsFromMenu(menu);

        if (currentMenu != null)
        {
            FadeOutMenu(currentMenuAnimators);
            yield return new WaitForSeconds(.5f);
            currentMenu.SetActive(false);
        }

        menu.SetActive(true);

        FadeInMenu(animators);

        currentMenu = menu;
        currentMenuAnimators = animators;
    }

    void FadeInMenu(TextAnimator[] animators)
    {
        float delay = 0;
        int i = 0;

        foreach (TextAnimator animator in animators)
        {
            animator.ResetPosition();
            animator.BounceIn().SetDelay(delay)
                .OnComplete(() => {
                    animator.transform.parent.GetComponent<Button>().interactable = true;
                });

            animator.GetComponent<TextMeshProUGUI>().color = Color.white;

            delay += .1f;
            i++;
        }
    }

    void FadeOutMenu(TextAnimator[] animators)
    {
        float delay = 0;
        foreach (TextAnimator animator in animators)
        {
            animator.transform.parent.GetComponent<Button>().interactable = false;
            animator.BounceOut().SetDelay(delay)
                .OnComplete(() => {
                    animator.Stop();
                    animator.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
                });

            delay += .1f;
        }
    }

    public void ShowConsole()
    {
        consoleBg.DOFade(.5f, 3f);
    }

    TextAnimator[] GetTextAnimatorsFromMenu(GameObject menu)
    {
        List<TextAnimator> animators = new List<TextAnimator>();

        foreach (Button btn in menu.GetComponentsInChildren<Button>())
        {
            animators.Add(btn.targetGraphic.GetComponent<TextAnimator>());
        }

        return animators.ToArray();
    }

    public void Exit()
    {
        Application.Quit();
    }

    IEnumerator FadeTitle()
    {
        float fadeValue = 1;
        float fadeMenuValue = 0;

        float currentTime = Time.time;
        bool faded = false;

        while (fadeValue > 0 || fadeMenuValue < 1) {
            fadeValue = Mathf.Lerp(fadeValue, -0.1f, Time.deltaTime*4);

            IntegrateMat.SetFloat("_Fade", fadeValue);
            IntegrateMatMenu.SetFloat("_Fade", fadeMenuValue);

            if (Time.time - currentTime > .3f)
            {
                fadeMenuValue = Mathf.Lerp(fadeMenuValue, 1.1f, Time.deltaTime * 3);
            }

            if (Time.time - currentTime > .5f && !faded)
            {
                SwitchMenu(firstMenu);

                faded = true;
            }

            yield return null;
        }

        IntegrateMat.SetFloat("_Fade", 0);
        IntegrateMatMenu.SetFloat("_Fade", 1);
    }

    public static MenuInteraction GetInstance()
    {
        return instance;
    }

    public enum MenuStates
    {
        Intro, Menu, Files, LoadGame
    }
}
