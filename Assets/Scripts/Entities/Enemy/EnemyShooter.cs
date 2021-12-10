using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : Shooter
{

    float autoStartFire = 0;
    bool automatic = false;

    protected override void Start()
    {
        base.Start();

        autoStartFire = Time.time + Random.Range(0f, 2f);
    }

    public void Init(float cooldown, int damage, bool automatic)
    {
        this.cooldown = cooldown;
        this.damage = damage;
        this.automatic = automatic;
    }

    void Update()
    {
        if (automatic)
        {
            if (CanShoot() && Time.time > autoStartFire)
            {
                Fire();
            }
        }
    }

    public void FireShooter(Quaternion rotation)
    {
        if (CanShoot())
        {
            Fire(rotation);
        }
    }

    public void SetAuto(bool automatic)
    {
        this.automatic = automatic;
    }
}