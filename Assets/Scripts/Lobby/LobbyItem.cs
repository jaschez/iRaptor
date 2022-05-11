using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LobbyItem : MonoBehaviour
{
    WorkbenchParticles workbenchReference;

    SpriteRenderer spriteRenderer;

    Sprite packageSprite;
    
    public ItemData itemData { get; private set; }

    public GameObject infoObj;

    public TextMeshPro nameTxt;
    public TextMeshPro descriptionTxt;
    public TextMeshPro pricetagTxt;

    bool isVisible = false;

    public void Initialize(ItemData _item, WorkbenchParticles workbench)
    {
        itemData = _item;

        workbenchReference = workbench;

        spriteRenderer = GetComponent<SpriteRenderer>();
        packageSprite = spriteRenderer.sprite;

        nameTxt.text = itemData.Name;
        descriptionTxt.text = itemData.Description;
        pricetagTxt.text = itemData.Price.ToString();
    }

    public void Show()
    {
        spriteRenderer.sprite = itemData.Image;

        Vector2 original = transform.localScale;
        transform.DOScaleX(1.6f, .1f).OnComplete(() => {
            transform.DOScaleX(1, 1f).SetEase(Ease.OutElastic);
            transform.DOScaleY(2f, .1f).OnComplete(() => transform.DOScaleY(1, .8f).SetEase(Ease.OutElastic));
        });

        pricetagTxt.gameObject.SetActive(true);

        isVisible = true;
    }

    public void Hide()
    {
        transform.DOScaleX(1.6f, .1f).OnComplete(() => {
            transform.DOScaleX(1, 1f).SetEase(Ease.OutElastic);
            transform.DOScaleY(2f, .1f).OnComplete(() => transform.DOScaleY(1, .8f).SetEase(Ease.OutElastic));
            spriteRenderer.sprite = packageSprite;
        });

        pricetagTxt.gameObject.SetActive(false);

        isVisible = false;
        HideInfo();
    }

    public void ShowInfo()
    {
        infoObj.SetActive(true);
    }

    public void HideInfo()
    {
        infoObj.SetActive(false);
    }

    void OnMouseEnter ()
    {
        if (isVisible) {
            ShowInfo();
        }
    }

    private void OnMouseDown()
    {
        if (isVisible)
        {
            workbenchReference.Unlock(this);
        }
    }

    void OnMouseExit()
    {
        HideInfo();
    }
}
