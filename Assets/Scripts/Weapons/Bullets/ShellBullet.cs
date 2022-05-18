using UnityEngine;
using DG.Tweening;

public class ShellBullet : Projectile
{
    public override void InitBullet()
    {
        base.InitBullet();

        SetVelocity(600);
        SetDamage(1);
        SetProjectileType(ProjectileType.Shell);
        SetEnemyBullet(false);
        MaxBounces = 1;

        DOTween.To(() => Velocity, x => Velocity = x, 0, Random.Range(.1f, .6f)).OnComplete(()=>
        {
            Impact();
        });
    }

    protected override void MovementUpdate()
    {
        //DOTween.To(() => Velocity, x => Velocity = x, 0, Random.Range(.1f, .6f));
    }

    protected override void OnWallImpact()
    {

        base.OnWallImpact();
    }
}
