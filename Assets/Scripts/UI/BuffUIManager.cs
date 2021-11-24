using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffUIManager : MonoBehaviour
{

    public float yOrigin = 150;
    public float buffsOffset = 50;

    public GameObject UIBuffPrefab;

    public Sprite[] buffImgs;

    Dictionary<PowerUpDrop.PowerUpType, BuffUIComponent> UIbuffs;

    List<BuffUIComponent> buffsList;

    private void Awake()
    {
        UIbuffs = new Dictionary<PowerUpDrop.PowerUpType, BuffUIComponent>();

        buffsList = new List<BuffUIComponent>();

        int count = (int)PowerUpDrop.PowerUpType.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(UIBuffPrefab, transform);

            go.SetActive(false);

            buffsList.Add(go.GetComponent<BuffUIComponent>());
        }
    }

    public void LoadBuffs(Dictionary<PowerUpDrop.PowerUpType, BuffUIComponent> UIbuffs)
    {
        this.UIbuffs = UIbuffs;
    }

    public void AddUIBuff(PowerUpDrop.PowerUpType type, int maxUses)
    {

        if (!UIbuffs.ContainsKey(type))
        {
            BuffUIComponent buffTemp = buffsList[(int)type];
            GameObject go = buffTemp.gameObject;

            go.SetActive(true);

            buffTemp.InitBuff(maxUses, buffImgs[(int)type]);         //buffImgs[(int)type]
            UIbuffs.Add(type, buffTemp);

            go.GetComponent<RectTransform>().localPosition = new Vector2(140, yOrigin - buffsOffset * (UIbuffs.Count - 1));
        }
        else
        {
            UIbuffs[type].AddBuff(maxUses);

            RectTransform rt = UIbuffs[type].GetComponent<RectTransform>();

            rt.localPosition = new Vector2(-40, rt.localPosition.y);
        }

        RectTransform rtB = UIbuffs[type].GetComponent<RectTransform>();
        Vector2 targetPos = new Vector2(0, rtB.localPosition.y);

        StartCoroutine(MoveBuffToPosition(rtB, targetPos));

    }

    public void UseBuffs()
    {
        foreach (KeyValuePair<PowerUpDrop.PowerUpType, BuffUIComponent> entry in UIbuffs)
        {
            entry.Value.UseBuff();
        }
    }

    public void DeleteUIBuff(PowerUpDrop.PowerUpType type)
    {
        if (UIbuffs.ContainsKey(type))
        {

            RectTransform rtB = UIbuffs[type].GetComponent<RectTransform>();
            Vector2 targetPos = new Vector2(230, rtB.localPosition.y);

            StartCoroutine(MoveBuffToPosition(rtB, targetPos));
            StartCoroutine(DeactivateObject(UIbuffs[type].gameObject, .5f));

            UIbuffs.Remove(type);

            StopCoroutine("UpdateBuffPositions");

            //UpdateBuffsPosition
            StartCoroutine(UpdateBuffPositions(.5f));
        }
    }

    IEnumerator MoveBuffToPosition(RectTransform rt, Vector2 targetPos)
    {
        float endTime = Time.time + 1;

        while (endTime > Time.time)
        {
            rt.localPosition = Vector2.Lerp(rt.localPosition, targetPos, Time.deltaTime * 6);

            yield return null;
        }

        rt.localPosition = targetPos;

        yield return null;
    }

    IEnumerator UpdateBuffPositions(float delayTime)
    {
        float endTime = Time.time + 2;

        List<RectTransform> rts = new List<RectTransform>();

        foreach (KeyValuePair<PowerUpDrop.PowerUpType, BuffUIComponent> entry in UIbuffs)
        {
            RectTransform rt = entry.Value.GetComponent<RectTransform>();
            rts.Add(rt);
        }

        yield return new WaitForSeconds(delayTime);

        while (endTime > Time.time)
        {

            for (int i = 0; i < rts.Count; i++)
            {

                RectTransform rt = rts[i];
                Vector2 targetPos = new Vector2(0, yOrigin - buffsOffset * i);

                rt.localPosition = Vector2.Lerp(rt.localPosition, targetPos, Time.deltaTime * 6);
            }

            yield return null;
        }

        for (int i = 0; i < rts.Count; i++) {
        
            Vector2 targetPos = new Vector2(0, yOrigin - buffsOffset * i);
            rts[i].localPosition = targetPos;
        }

        yield return null;
    }

    IEnumerator DeactivateObject(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        obj.SetActive(false);

        yield return null;
    }
}
