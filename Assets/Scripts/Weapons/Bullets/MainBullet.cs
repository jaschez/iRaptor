using UnityEngine;
using DG.Tweening;

public class MainBullet : Projectile
{
    Vector2 originalScale;

    public override void InitBullet()
    {
        base.InitBullet();

        SetVelocity(600);
        SetDamage(1);
        SetProjectileType(ProjectileType.Main);
        SetEnemyBullet(false);

        originalScale = transform.localScale;

        transform.localScale = new Vector2(originalScale.x * .2f, originalScale.y * 5f);
        transform.DOScale(originalScale, .2f).SetEase(Ease.OutCubic);
    }

    protected override void MovementUpdate()
    {
        
    }
}
