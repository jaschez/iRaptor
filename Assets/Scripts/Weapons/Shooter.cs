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
    float cooldownFinishTime = 0;

    protected bool enemyBullet = true;

    protected virtual void Start()
    {
        pooler = ObjectPooler.GetInstance();
    }

    protected void Fire(int[] buffIndexes = null)
    {
        Fire(transform.rotation, buffIndexes);
    }

    protected void Fire(Quaternion orientation, int[] buffIndexes = null)
    {

        buffIndexes = buffIndexes ?? new int[0];

        if (Time.time > cooldownFinishTime) {
            cooldownFinishTime = Time.time + cooldown;

            if (!enemyBullet) {
                position = projectileStartingPos.position;
            }
            else
            {
                position = transform.position + orientation * Vector2.up * 50;
            }

            GameObject projectile = pooler.Spawn("bullet", position, orientation);

            Bullet bulletModule = projectile.GetComponent<Bullet>();

            if (bulletModule != null)
            {
                bulletModule.InitBullet(damage, enemyBullet);
                bulletModule.SetBuffState(buffIndexes);
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
