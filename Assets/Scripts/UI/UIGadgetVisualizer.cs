using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGadgetVisualizer : MonoBehaviour
{
    public GameObject unitPrefab;

    Transform origin;

    List<Slider> units;

    List<Slider> unitsCooldown;

    public float width;
    public float offset;

    int maxUses;
    int currentUses;

    private void Start()
    {

    }

    public void Init(int currentUses, int maxUses)
    {
        units = new List<Slider>();
        unitsCooldown = new List<Slider>();

        origin = GetComponent<Transform>();

        this.currentUses = currentUses;
        this.maxUses = maxUses;

        for (int i = 0; i < maxUses; i++)
        {
            GameObject unit = Instantiate(unitPrefab, transform);
            GameObject unitCooldown;

            RectTransform rt = unit.GetComponent<RectTransform>();
            RectTransform rtC;

            Slider slider = unit.GetComponent<Slider>();
            Slider sliderC;


            rt.localPosition = Vector2.right * (width / maxUses) * i;
            rt.sizeDelta = new Vector2((width / maxUses) - offset, rt.sizeDelta.y);

            unitCooldown = Instantiate(unit, transform);
            unitCooldown.transform.GetChild(0).gameObject.SetActive(false);

            rtC = unitCooldown.GetComponent<RectTransform>();
            sliderC = unitCooldown.GetComponent<Slider>();

            sliderC.fillRect.GetComponent<Image>().color = new Color(0, 0, 0, .1f);

            if (i < currentUses)
            {
                slider.value = 1;

                if (i == currentUses - 1)
                {
                    sliderC.value = 0;
                }
                else
                {
                    sliderC.value = 1;
                }
            }
            else
            {
                slider.value = 0;
                sliderC.value = 1;
            }

            units.Add(slider);
            unitsCooldown.Add(sliderC);
        }
    }

    public void SpendUse(int use = 1)
    {
        int prevUses = currentUses;

        currentUses -= use;

        for (int i = currentUses; i < prevUses; i++)
        {
            StartCoroutine(SpentAnimation(i));
        }

        if (currentUses > 0) {
            StartCoroutine(CheckCoolDown());
        }
        else
        {
            Invoke("SendRanOutMsg", .2f);
        }
    }

    void SendRanOutMsg()
    {
        UIVisualizer.GetInstance().PopUp(PopUpType.Info, "RAN OUT", PlayerModule.GetInstance().transform, .4f, 14, 4, 2);
        SoundManager.Play(Sounds.RunOut, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);
    }

    public void AddUse(int use = 1)
    {
        int prevUses = currentUses;

        if (currentUses < maxUses)
        {
            currentUses += use;

            for (int i = prevUses; i < currentUses; i++)
            {
                units[i].value = 1;
                unitsCooldown[i].value = 0;

                if (i > 0)
                {
                    unitsCooldown[i - 1].value = 1;
                }
            }
        }
    }

    IEnumerator SpentAnimation(int useIndex)
    {
        float animTime = Time.time + .2f;

        while (animTime > Time.time)
        {
            units[useIndex].value = Mathf.Lerp(units[useIndex].value, 0, Time.deltaTime * 20);

            yield return null;
        }

        units[useIndex].value = 0;

        yield return null;
    }

    IEnumerator CheckCoolDown()
    {

        PlayerModule player = (PlayerModule)PlayerModule.GetInstance();

        while (player.CheckGadgetCooldownPerc() > 0)
        {
            if (currentUses > 0) {
                unitsCooldown[currentUses - 1].value = player.CheckGadgetCooldownPerc();
            }
            else
            {
                StopAllCoroutines();
            }
            yield return null;
        }

        unitsCooldown[currentUses - 1].value = 0;

        RectTransform rt = units[currentUses - 1].fillRect;

        float lerpTime = Time.time + .5f;
        Vector2 origSize = rt.sizeDelta;

        Image unitImg = rt.GetComponent<Image>();
        Color origUnitColor = unitImg.color;
        Color lerpColor = new Color (.8f, .8f, .8f, 1);

        rt.sizeDelta += Vector2.up * 200;
        unitImg.color = Color.white;

        SoundManager.Play(Sounds.Pump, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

        UIVisualizer.GetInstance().PopUp(PopUpType.Info, "PUMPED!", player.transform, .3f, 12, 2, 1);

        while (Time.time < lerpTime)
        {
            unitImg.color = Color.Lerp(unitImg.color, origUnitColor, Time.deltaTime * 3);
            rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, origSize, Time.deltaTime * 15);
            yield return null;
        }

        rt.sizeDelta = origSize;
        unitImg.color = origUnitColor;

        yield return null;
    }
}
