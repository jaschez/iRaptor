using System.Collections;
using System.Collections.Generic;
using TMPro;
using CharTween;
using DG.Tweening;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    void Start()
    {
        // Set text
        TMP_Text textMesh = GetComponent<TMP_Text>();

        CharTweener tweener = textMesh.GetCharTweener();
        /* Sequence sequence = DOTween.Sequence();
         for (int i = 0; i < tweener.CharacterCount; i++)
         {
             float timeOffset = Mathf.Lerp(0, .5f, i / (float)tweener.CharacterCount);
             Sequence charSequence = DOTween.Sequence();
             charSequence
                 .Append(tweener.DOOffsetMoveY(i, 5f, .2f).SetEase(Ease.InOutCubic));
             sequence.Insert(timeOffset + 1, charSequence);
         }

         sequence.SetLoops(-1, LoopType.Incremental);*/

        DOTween.Sequence()
    .Join(BounceIn(textMesh)).SetLoops(-1, LoopType.Restart).SetDelay(2f);
    }

    private Sequence BounceIn(TMP_Text text, float distance = 10, int? charsAhead = null)
    {
        Sequence textSequence = DOTween.Sequence();
        CharTweener tweener = text.GetCharTweener();

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
            //charSequence.Insert(.5f, tweener.DOFade(i, 0, 0));
            textSequence.Insert((float)i / count * 0.4f, charSequence);
        }

        textSequence.SetTarget(text);
        return textSequence;
    }
}
