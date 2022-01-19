using System.Collections;
using System.Collections.Generic;
using TMPro;
using CharTween;
using DG.Tweening;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    TMP_Text textMesh;
    CharTweener textTweener;

    Sequence animSequence;

    void Start()
    {
        // Set text
        textMesh = GetComponent<TMP_Text>();
        textTweener = textMesh.GetCharTweener();
    }

    public Sequence Animate(Sequence animation)
    {
        DOTween.Kill(textMesh);
        animation.SetTarget(textMesh);

        animSequence = DOTween.Sequence().Join(animation);
        return animSequence;
    }

    public static Sequence BounceOut(TextAnimator anim, float distance = 10, int? charsAhead = null)
    {
        Sequence textSequence = DOTween.Sequence();
        CharTweener tweener = anim.textTweener;
        
        int count = charsAhead ?? tweener.CharacterCount;

        for (int i = 0; i < count; i++)
        {
            Sequence charSequence = DOTween.Sequence();
            charSequence.Insert(0, tweener.DOFade(i, 0, 0));
            charSequence.Insert(0, tweener.DOOffsetMoveY(i, distance, 0.05f).SetEase(Ease.OutSine));
            charSequence.Insert(0.05f, tweener.DOOffsetMoveY(i, 0, 0.1f).SetEase(Ease.OutSine));
            textSequence.Insert((float)i / count * 0.4f, charSequence);
        }

        tweener.UpdateCharProperties();

        return textSequence;
    }

    public static Sequence BounceIn(TextAnimator anim, float distance = 10, int? charsAhead = null)
    {
        Sequence textSequence = DOTween.Sequence();
        CharTweener tweener = anim.textTweener;

        int count = charsAhead ?? tweener.CharacterCount;
        for (int i = 0; i < count; i++)
        {
            tweener.SetAlpha(i, 0);
            tweener.SetLocalScale(i, 1);
            tweener.ResetPosition(i);
            tweener.UpdateCharProperties();
        }

        for (int i = 0; i < count; i++)
        {
            Sequence charSequence = DOTween.Sequence();
            charSequence.Insert(0, tweener.DOFade(i, 1, 0));
            charSequence.Insert(0, tweener.DOOffsetMoveY(i, distance, 0.05f).SetEase(Ease.OutSine));
            charSequence.Insert(0.05f, tweener.DOOffsetMoveY(i, 0, 0.1f).SetEase(Ease.OutSine));
            textSequence.Insert((float)i / count * 0.4f, charSequence);
        }

        return textSequence;
    }
}
