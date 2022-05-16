using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shooter : MonoBehaviour
{
    public Transform projectileStartingPos;

    ObjectPooler pooler;

    Vector3 position;

    protected int Damage { get; private set; } = 1;

    protected float Cooldown { get; private set; } = 0.1f;
    protected float Velocity { get; private set; } = 20;

    protected ProjectileType BulletT { get; private set; } = ProjectileType.Main;

    protected List<Projectile.Effect> ProjectileEffects { get; private set; } = new List<Projectile.Effect>();

    private float cooldownFinishTime = 0;

    protected bool enemyBullet = true;

    public virtual void Initialize()
    {
        pooler = ObjectPooler.GetInstance();
    }

    protected void FireBullet(float angle)
    {
        FireBullet(Quaternion.Euler(0,0, angle));
    }

    protected void FireBullet(Quaternion orientation)
    {
        cooldownFinishTime = Time.time + Cooldown;

        if (!enemyBullet) {
            position = projectileStartingPos.position;
        }
        else
        {
            position = transform.position + orientation * Vector2.up * 8;
        }

        GameObject projectile = pooler.Spawn(BulletT, position, orientation);
        Projectile projectileModule = projectile.GetComponent<Projectile>();

        if (projectileModule != null)
        {
            projectileModule.InitBullet();
            ModifyProjectile(projectileModule);
        }
        else
        {
            Debug.LogError("Bullet component is null!");
        }
    }

    protected virtual void ModifyProjectile(Projectile projectile)
    {

    }

    protected void SetDamage(int damage)
    {
        Damage = damage;
    }

    protected void SetCooldown(float cooldown)
    {
        Cooldown = cooldown;
    }

    protected void SetVelocity(float velocity)
    {
        Velocity = velocity;
    }

    protected void SetProjectileType(ProjectileType bulletType)
    {
        BulletT = bulletType;
    }

    protected void SetEffects(List<Projectile.Effect> bulletEffects)
    {
        ProjectileEffects = bulletEffects;
    }

    protected void AddEffect(Projectile.Effect effect)
    {
        ProjectileEffects.Add(effect);
    }

    public bool CanShoot()
    {
        return Time.time > cooldownFinishTime;
    }
}
