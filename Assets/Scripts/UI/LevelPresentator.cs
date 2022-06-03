using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelPresentator : MonoBehaviour
{
    [SerializeField] TMP_Text levelNumber;
    [SerializeField] TMP_Text levelName;

    float blinkingTime = .02f;

    private void Awake()
    {
        levelNumber.gameObject.SetActive(false);
        levelName.gameObject.SetActive(false);
    }

    public void Initialize(int level, string name)
    {
        levelNumber.text = "Level " + level;
        levelName.text = "~" + name + "~";

        StartCoroutine(Present());
    }

    IEnumerator Present()
    {
        StartCoroutine(Show(levelNumber, 3, blinkingTime, true));
        yield return new WaitForSeconds(.2f);
        StartCoroutine(Show(levelName, 3, blinkingTime, true));
        yield return new WaitForSeconds(3f);
        StartCoroutine(Show(levelNumber, 3, blinkingTime, false));
        yield return new WaitForSeconds(.2f);
        StartCoroutine(Show(levelName, 3, blinkingTime, false));
    }

    IEnumerator Show(TMP_Text text, int blinks, float blinkingTime, bool visible)
    {
        if (visible)
        {
            text.gameObject.SetActive(true);
        }

        for (int i = 0; i < blinks; i++)
        {
            text.color = Color.black;
            yield return new WaitForSeconds(blinkingTime);
            text.color = Color.white;
            yield return new WaitForSeconds(blinkingTime);
        }

        if (!visible)
        {
            text.gameObject.SetActive(false);
        }
    }
}
