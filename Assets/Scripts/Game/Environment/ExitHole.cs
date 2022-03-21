using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHole : MonoBehaviour, Interactable
{

    SpriteRenderer sr;

    bool levelCleared = false;
    bool canExit = false;

    public delegate void EnteredExit();
    public static event EnteredExit OnExitEnter;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, .3f);
        AllowExit();
    }

    private void OnEnable()
    {
        //LevelManager.OnLevelClear += AllowExit;
    }

    private void OnDestroy()
    {
        //LevelManager.OnLevelClear -= AllowExit;
    }

    void AllowExit()
    {
        levelCleared = true;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
    }

    public void Interact()
    {
        OnExitEnter?.Invoke();
    }
}
