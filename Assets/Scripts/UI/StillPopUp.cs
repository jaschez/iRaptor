using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StillPopUp : MonoBehaviour
{

    public Transform target;

    RectTransform rt;

    public Vector2 offset;
    Vector2 animationOffset;

    Text stillText;

    float msgValue = 0;
    float visualMsgValue = 1;

    //bool animEnded = true;

    private void Start()
    {
        stillText = GetComponent<Text>();
        rt = GetComponent<RectTransform>();

        Color modColor = stillText.color;
        modColor.a = 0;
        stillText.color = modColor;
    }

    public void UpdateValue(int value)
    {
        msgValue += value;

        //if (animEnded)
        //{
        StopAllCoroutines();
        StartCoroutine(PopUp());

            //animEnded = false;
        //}
    }

    IEnumerator PopUp()
    {
        float duration = 2;

        float endTime = Time.time + duration * .7f;
        float vanishTime = endTime + duration * .3f;

        Vector3 targetPos;

        Color modColor = stillText.color;
        modColor.a = 1;
        stillText.color = modColor;

        animationOffset = Vector2.zero;

        rt.position = (Vector2)target.position + offset;

        while (Time.time < endTime)
        {
            visualMsgValue = Mathf.Lerp(visualMsgValue, msgValue + 1, Time.deltaTime * 4);
            stillText.text = "+" + ((int)visualMsgValue).ToString();

            animationOffset = Vector2.Lerp(animationOffset, Vector2.up * 5, Time.deltaTime * 5);
            targetPos = (Vector2)target.position + offset + animationOffset;
            targetPos.z = 0;

            rt.position = targetPos;

            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);

            yield return new WaitForFixedUpdate();
        }

        visualMsgValue = msgValue;

        StartCoroutine(Blink(3, vanishTime - Time.time));

        while (Time.time < vanishTime)
        {
            targetPos = (Vector2)target.position + offset + animationOffset;
            rt.position = targetPos;
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);
            yield return new WaitForFixedUpdate();
        }

        modColor.a = 0;
        stillText.color = modColor;

        msgValue = 0;
        visualMsgValue = 1;

        yield return null;
    }

    IEnumerator Blink(int blinks, float duration)
    {

        Color modColor = stillText.color;

        for (int i = 0; i < blinks; i++)
        {
            modColor.a = 0;
            stillText.color = modColor;
            yield return new WaitForSeconds((duration / blinks) / 2);

            modColor.a = 1;
            stillText.color = modColor;
            yield return new WaitForSeconds((duration / blinks) / 2);
        }
    }
}
