using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : Shooter
{
    float autoStartFire = 0;
    bool automatic = false;

    protected void Start()
    {
        Initialize();

        autoStartFire = Time.time + Random.Range(0f, 2f);
    }

    public void Init(float cooldown, float velocity, int damage, ProjectileType bulletType, bool automatic)
    {
        SetCooldown(cooldown);
        SetVelocity(velocity);
        SetDamage(damage);
        SetProjectileType(bulletType);
        this.automatic = automatic;
    }

    void Update()
    {
        if (automatic)
        {
            if (CanShoot() && Time.time > autoStartFire)
            {
                FireBullet(0);
            }
        }
    }

    public void FireShooter(Quaternion rotation)
    {
        if (CanShoot())
        {
            FireBullet(rotation);
        }
    }

    public void SetAuto(bool automatic)
    {
        this.automatic = automatic;
    }
}