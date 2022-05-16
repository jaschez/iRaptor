using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Projectile
{
    public override void InitBullet()
    {
        base.InitBullet();

        SetVelocity(150);
        SetDamage(4);
        SetProjectileType(ProjectileType.Numbrian);
        SetEnemyBullet(true);
    }

    protected override void MovementUpdate()
    {
        transform.Translate(Time.deltaTime * Velocity * Orientation);
    }
}
