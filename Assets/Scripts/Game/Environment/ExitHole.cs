using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHole : MonoBehaviour
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
        LevelManager.OnLevelClear += AllowExit;
    }

    private void OnDestroy()
    {
        LevelManager.OnLevelClear -= AllowExit;
    }

    void AllowExit()
    {
        levelCleared = true;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        Entity entity = other.GetComponent<Entity>();

        if (entity){

            if (entity.GetEntityType() == EntityType.Player)
            {

                if (levelCleared)
                {
                    canExit = true;
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {

        if (Input.GetKeyDown(KeyCode.E) && canExit)
        {
            OnExitEnter?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        canExit = false;
    }
}
