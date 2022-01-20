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

    void Awake()
    {
        // Set text
        textMesh = GetComponent<TMP_Text>();
        textTweener = textMesh.GetCharTweener();
    }

    public Sequence BounceOut(float distance = 10, int? charsAhead = null)
    {
        Sequence textSequence = DOTween.Sequence();
        
        int count = charsAhead ?? textTweener.CharacterCount;

        for (int i = 0; i < count; i++)
        {
            Sequence charSequence = DOTween.Sequence();
            charSequence.Insert(0.08f, textTweener.DOFade(i, 0, 0));
            charSequence.Insert(0, textTweener.DOOffsetMoveY(i, distance, 0.05f).SetEase(Ease.OutSine));
            charSequence.Insert(0.05f, textTweener.DOOffsetMoveY(i, 0, 0.01f).SetEase(Ease.OutSine));
            textSequence.Insert(i * 0.02f, charSequence);
        }

        textTweener.UpdateCharProperties();

        return textSequence;
    }

    public Sequence BounceIn(float distance = 10, int? charsAhead = null)
    {
        Sequence textSequence = DOTween.Sequence();

        int count = charsAhead ?? textTweener.CharacterCount;
        for (int i = 0; i < count; i++)
        {
            textTweener.SetAlpha(i, 0);
            textTweener.SetLocalScale(i, 1);
            textTweener.ResetPosition(i);
            textTweener.UpdateCharProperties();
        }

        for (int i = 0; i < count; i++)
        {
            Sequence charSequence = DOTween.Sequence();
            charSequence.Insert(0, textTweener.DOFade(i, 1, 0));
            charSequence.Insert(0, textTweener.DOOffsetMoveY(i, distance, 0.05f).SetEase(Ease.OutSine));
            charSequence.Insert(0.05f, textTweener.DOOffsetMoveY(i, 0, 0.1f).SetEase(Ease.OutSine));
            textSequence.Insert((float)i / count * 0.4f, charSequence);
        }

        return textSequence;
    }

    public void OnSelect() {
        transform.DOLocalMoveX(30, 0.1f).SetEase(Ease.InOutSine);
    }

    public void OnDeselect()
    {
        transform.DOLocalMoveX(0, 0.1f).SetEase(Ease.InOutSine);
    }

    public void ResetPosition()
    {
        transform.localPosition = Vector3.zero;
    }

    public void Stop()
    {
        DOTween.Kill(textMesh);
    }

    public CharTweener GetCharTweener()
    {
        return textTweener;
    }
}
