using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpMessage : MonoBehaviour
{

    public Text UItext;

    public RectTransform rt;

    PopUpType type;

    string msg;

    float lifeTime = .3f;
    float movement = 6f;
    float orderOffset = 0;

    static float infoPopUps = 0;

    int blinks = 3;

    Transform target;

    Vector2 offset;

    private void Awake()
    {
        infoPopUps = 0;
    }

    public void Init(PopUpType type, string msg, Transform target, float lifeTime, int size, float movement, int blinks)
    {

        this.type = type;
        this.msg = msg;
        this.target = target;
        this.lifeTime = lifeTime;
        this.movement = movement;
        this.blinks = blinks;

        UItext.resizeTextMaxSize = size;

        switch (type)
        {
            case PopUpType.Good:
                UItext.color = Color.green;
                break;

            case PopUpType.Bad:
                UItext.color = new Color(.7f, .1f, .1f);
                break;

            case PopUpType.Info:
                UItext.color = Color.white;
                orderOffset = AddPopUpHeight();
                break;

            default:
                UItext.color = Color.grey;
                break;
        }

        if (type == PopUpType.Info)
        {
            StartCoroutine(PopUpMsg());
        }
        else
        {
            StartCoroutine(PopUpStat());
        }
    }

    public void Init(PopUpType type, string msg, Color col, Transform target, float lifeTime, int size, float movement, int blinks)
    {
        Init(type, msg, target, lifeTime, size, movement, blinks);

        UItext.color = col;
    }

    public static void ResetInfoPopupOffset()
    {
        infoPopUps = 0;
    }

    float AddPopUpHeight()
    {
        float aux = infoPopUps;
        infoPopUps += (UItext.resizeTextMaxSize / 5f) * 2;
        return aux;
    }

    void ClearInfoPopUp()
    {
        infoPopUps -= (UItext.resizeTextMaxSize / 5f) * 2;
    }

    IEnumerator PopUpStat()
    {

        float initTime = Time.time;
        float lifeLimit = initTime + lifeTime;
        float spawnLimit = initTime + lifeTime / 3f;

        float randomXOffset = Random.Range(-5f,5f);

        Color UIcolor = UItext.color;

        UItext.text = msg;

        UIcolor.a = 0;

        while (Time.time < lifeLimit)
        {
            if (Time.time < spawnLimit)
            {
                UIcolor.a = 1;
            }
            else
            {
                UIcolor.a = 0;
            }

            offset = Vector2.up * Mathf.Sin((Time.time - initTime)*4/lifeTime) * movement + Vector2.up * orderOffset +
                Vector2.right * Mathf.Cos((Time.time - initTime) * 3/lifeTime) * randomXOffset;

            UItext.color = Color.Lerp(UItext.color, UIcolor, Time.deltaTime * 3/lifeTime);
            rt.position = target.position + (Vector3)offset;

            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject, .01f);

        yield return null;
    }

    IEnumerator PopUpMsg()
    {
        int index = 0;

        char[] msgArray = msg.ToCharArray();

        float initTime = Time.time;
        float lifeLimit = initTime + lifeTime;
        float deathLimit = initTime + lifeTime * 5f / 6f;

        float pause = lifeTime / (msg.Length * 5);
        float nextPauseTime = 0;

        float yOffset = 14;

        Color UIcolor = UItext.color;
        UIcolor.a = 1;

        offset = Vector2.up * yOffset;

        while (index < msgArray.Length)
        {
            if (Time.time > nextPauseTime) {
                nextPauseTime = Time.time + pause;

                UItext.text += msgArray[index];
                index++;
            }

            rt.position = target.position + (Vector3)offset + Vector3.up * orderOffset;

            yield return new WaitForFixedUpdate();
        }

        while (Time.time < deathLimit)
        {
            rt.position = target.position + (Vector3)offset + Vector3.up * orderOffset;

            yield return new WaitForFixedUpdate();
        }

        while (Time.time < lifeLimit)
        {
            offset = Vector2.Lerp(offset, Vector2.up * (movement + yOffset), Time.deltaTime * 10 / lifeTime);

            rt.position = target.position + (Vector3)offset + Vector3.up * orderOffset;

            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(BlinkText(blinks, .06f));

        while (Time.time < lifeLimit + .5f)
        {

            rt.position = target.position + (Vector3)offset + Vector3.up * orderOffset;

            yield return new WaitForFixedUpdate();
        }

        if (type == PopUpType.Info)
        {
            ClearInfoPopUp();
        }

        Destroy(gameObject, .3f);

        yield return null;
    }

    IEnumerator BlinkText(int num, float speed)
    {
        Color UIcolor = UItext.color;

        for (int i = 0; i < num; i++)
        {

            UIcolor.a = 0;
            UItext.color = UIcolor;

            yield return new WaitForSeconds(speed);

            UIcolor.a = 1;
            UItext.color = UIcolor;

            yield return new WaitForSeconds(speed);

        }

        UIcolor.a = 0;
        UItext.color = UIcolor;

    }
}

public enum PopUpType
{
    Good,
    Bad,
    Info
}
