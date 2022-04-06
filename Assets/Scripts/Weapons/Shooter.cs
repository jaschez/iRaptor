using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{

    public Transform projectileStartingPos;

    ObjectPooler pooler;

    Vector3 position;

    protected int damage = 1;

    protected float cooldown = 0.1f;
    protected float velocity = 20;
    float cooldownFinishTime = 0;

    protected bool enemyBullet = true;

    protected virtual void Start()
    {
        pooler = ObjectPooler.GetInstance();
    }

    protected void Fire(Bullet.Effect[] bulletEffects = null)
    {
        Fire(transform.rotation, bulletEffects);
    }

    protected void Fire(Quaternion orientation, Bullet.Effect[] bulletEffects = null)
    {

        bulletEffects = bulletEffects ?? new Bullet.Effect[0];

        if (Time.time > cooldownFinishTime) {
            cooldownFinishTime = Time.time + cooldown;

            if (!enemyBullet) {
                position = projectileStartingPos.position;
            }
            else
            {
                position = transform.position + orientation * Vector2.up * 8;
            }

            GameObject projectile = pooler.Spawn("bullet", position, orientation);

            Bullet bulletModule = projectile.GetComponent<Bullet>();

            if (bulletModule != null)
            {
                bulletModule.InitBullet(velocity, damage, enemyBullet);
                bulletModule.SetEffects(bulletEffects);
            }
            else
            {
                Debug.LogError("Bullet component is null!");
            }
        }
    }

    public bool CanShoot()
    {
        return Time.time > cooldownFinishTime;
    }
}
