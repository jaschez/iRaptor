using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Summoner : MonoBehaviour
{
    [SerializeField]
    private GameObject summoningObject;

    private SpriteRenderer sr;
    private GameObject srObj;

    public void Initialize(GameObject summoningObj, float delay, float timeToSummon)
    {
        summoningObject = summoningObj;
        summoningObject.SetActive(false);

        sr = gameObject.GetComponentInChildren<SpriteRenderer>(true);
        srObj = sr.gameObject;

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
        sr.DOFade(1, .3f);

        StartCoroutine(DelayedShake(delay, timeToSummon));
    }

    IEnumerator DelayedShake(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        Tweener shaking = srObj.transform.DOShakePosition(duration, 2, 30);
        shaking.Goto(shaking.Duration());
        shaking.PlayBackwards();
        shaking.OnRewind(() =>
        {
            summoningObject.SetActive(true);
            EnemyModule enemy = summoningObject.GetComponent<EnemyModule>();
            enemy.Flash();
            SoundManager.Play(Sound.Pump, transform.position);
            gameObject.SetActive(false);
        });
    }
}
