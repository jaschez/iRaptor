using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMove : MonoBehaviour
{

    RectTransform rt;

    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.rotation = Quaternion.Euler(Mathf.Sin(Time.time*.3f) * 20, Mathf.Cos(Time.time * .3f) * 20, Mathf.Sin(Time.time * .5f) * 8);
    }
}
