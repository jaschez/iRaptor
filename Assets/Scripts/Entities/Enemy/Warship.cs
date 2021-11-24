using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warship : EnemyModule
{
    Rigidbody2D rb;
    Mover mover;

    bool stateDone = false;

    float nextStateTime = 0;

    protected override void InitEnemy()
    {
        SetEnemyType(EnemyType.Warship);
        InitHealth(3);

        rb = GetComponent<Rigidbody2D>();
        mover = gameObject.AddComponent<Mover>();
        mover.InitMover(60);

        maxCarbonUnits = 4;
    }

    void Update()
    {
        if (nextStateTime < Time.time)
        {
            if (Random.Range(0, 10) > 5)
            {
                mover.MoveTo((Vector2)transform.position + Random.insideUnitCircle.normalized * 3f);
            }
            nextStateTime = Time.time + Random.Range(.5f, 2f);
        }
    }

    protected override void OnEnemyDead()
    {
        SoundManager.Play(Sounds.ShipDeath, transform.position);
        CamManager.GetInstance().ShockGame(.05f);
        CamManager.GetInstance().ShakeQuake(5, 1f, false);
    }
}
