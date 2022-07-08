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

    protected override void OnEntityImpact(Entity entity)
    {
        if (entity.GetEntityType() == EntityType.Player)
        {
            if (!entity.GetComponent<DashModule>().IsDashing())
            {
                ImpactEntity(entity);
            }
        }
    }

    protected override void MovementUpdate()
    {

    }
}
