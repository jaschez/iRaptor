using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : Shooter
{

    float startFire = 0;

    protected override void Start()
    {
        base.Start();

        cooldown = 1f;
        damage = 4;

        startFire = Time.time + Random.Range(0f, 2f);
    }

    void Update()
    {
        if (CanShoot() && Time.time > startFire)
        {
            Fire();
        }
        
    }
}
