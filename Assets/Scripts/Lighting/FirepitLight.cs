using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine;

[RequireComponent(typeof(Light2D))]
public class FirepitLight : MonoBehaviour
{

    Light2D lightPoint;

    [SerializeField]
    float minimum = 50;

    [SerializeField]
    float maximum = 120;

    [SerializeField]
    float rate = 6;

    float randomValue = 60;

    void Start()
    {
        lightPoint = GetComponent<Light2D>();

        StartCoroutine(FireCoroutine());
    }

    void Update()
    {
        lightPoint.pointLightOuterRadius = Mathf.Lerp(lightPoint.pointLightOuterRadius, randomValue, Time.deltaTime * rate);        
    }

    IEnumerator FireCoroutine()
    {
        while (true)
        {
            randomValue = Random.Range(minimum, maximum);
            yield return new WaitForSeconds(.2f);
        }
    }
}
