using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class MyEvent : UnityEvent{
    
}

public class Menu : MonoBehaviour
{
    public UnityEvent OnFadeIn;
    public MyEvent OnFadeOut;

    // Start is called before the first frame update
    void Start()
    {
        OnFadeIn.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
