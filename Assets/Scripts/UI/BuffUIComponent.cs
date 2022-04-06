using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffUIComponent : MonoBehaviour
{

    [SerializeField]
    Image descriptiveImg;

    public void InitBuff(Sprite descriptiveSprite)
    {
        descriptiveImg.sprite = descriptiveSprite;
    }
}
