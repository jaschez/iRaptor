using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    static UIManager instance;

    [SerializeField]
    List<UIComponent> Components;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Initialize()
    {
        foreach (UIComponent comp in Components)
        {
            comp.Initialize();
            comp.HookEvents();
        }
    }

    public static UIManager GetInstance()
    {
        return instance;
    }
}
