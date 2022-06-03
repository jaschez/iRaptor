using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHole : MonoBehaviour, Interactable
{

    SpriteRenderer sr;

    public delegate void EnteredExit();
    public static event EnteredExit OnExitEnter;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, .3f);
        AllowExit();
    }

    void AllowExit()
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
    }

    public void Interact()
    {
        OnExitEnter?.Invoke();
    }
}
