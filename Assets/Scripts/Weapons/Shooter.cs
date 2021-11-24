using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{

    public Transform projectileStartingPos;

    ObjectPooler pooler;

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

        buffIndexes = buffIndexes ?? new int[0];

        if (Time.time > cooldownFinishTime) {
            cooldownFinishTime = Time.time + cooldown;

            Quaternion orientation = transform.rotation;
            Vector3 position = projectileStartingPos.position;

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
