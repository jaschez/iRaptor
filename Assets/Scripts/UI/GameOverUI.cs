using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Transform gameOverSign;
    [SerializeField] GameObject buttons;

    bool canInteract = true;

    private void Awake()
    {
        background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
        gameOverSign.localScale = new Vector2(2100, 2100);
        buttons.SetActive(false);
    }

    public void Show()
    {
        background.DOFade(.5f, 2f).OnStart(()=> {
            gameOverSign.DOScale(50, 1f).SetEase(Ease.InSine).OnComplete(()=>
            {
                gameOverSign.DOShakePosition(.8f, 10, 30);
                buttons.SetActive(true);
            });
        }).SetDelay(.5f);
    }

    public void Return()
    {
        if (canInteract) {
            TransitionSystem.GetInstance().SwitchToScene(SceneSystem.GameScenes.Lobby, TransitionSystem.Transition.FadeOut, .4f);
            canInteract = false;
        }
    }

    public void Retry()
    {
        if (canInteract)
        {
            TransitionSystem.GetInstance().SwitchToScene(SceneSystem.GameScenes.Level, TransitionSystem.Transition.FadeOut, .4f);
            canInteract = false;
        }
    }
}
