using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffUIComponent : MonoBehaviour
{

    [SerializeField]
    Image descriptiveImg;

    [SerializeField]
    Image usesBar;

    [SerializeField]
    Text usesText;

    int maxUses;

    int currentUses = 0;
    float visualUses = 0;

    public void InitBuff(int maxUses, Sprite descriptiveSprite)
    {
        this.maxUses = maxUses;
        descriptiveImg.sprite = descriptiveSprite;

        usesBar.fillAmount = 0;

        StartCoroutine(ChangeValue(maxUses));
    }

    public void AddBuff(int addedUses)
    {
        maxUses += addedUses;

        StartCoroutine(ChangeValue(currentUses + addedUses));
    }

    public void UseBuff()
    {
        currentUses--;
        visualUses = currentUses;

        usesText.text = currentUses + "/" + maxUses;
        usesBar.fillAmount = currentUses / (float)maxUses;
    }

    IEnumerator ChangeValue(int value)
    {
        float endTime = Time.time + 1;
        currentUses = value;

        while(endTime > Time.time)
        {
            usesBar.fillAmount = Mathf.Lerp(usesBar.fillAmount, currentUses / (float)maxUses, Time.deltaTime * 10);

            visualUses = Mathf.Lerp(visualUses, currentUses + 5, Time.deltaTime * 3);

            visualUses = Mathf.Clamp(visualUses, 0, currentUses);

            usesText.text = (int)visualUses + "/" + maxUses;

            yield return null;
        }

        usesBar.fillAmount = currentUses / (float)maxUses;

        visualUses = currentUses;
        usesText.text = visualUses + "/" + maxUses;

        yield return null;
    }
}
