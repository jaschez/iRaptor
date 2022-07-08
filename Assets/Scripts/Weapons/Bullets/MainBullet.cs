using UnityEngine;
using DG.Tweening;

public class MainBullet : Projectile
{
    Vector2 originalScale;

    GameObject attractedTarget;

    public override void InitBullet()
    {
        base.InitBullet();

        SetVelocity(600);
        SetDamage(1);
        SetProjectileType(ProjectileType.Main);
        SetEnemyBullet(false);

        attractedTarget = null;

        originalScale = transform.localScale;

        transform.localScale = new Vector2(originalScale.x * .2f, originalScale.y * 5f);
        transform.DOScale(originalScale, .2f).SetEase(Ease.OutCubic);
    }

    protected override void MovementUpdate()
    {
        if (attractedTarget == null)
        {
            Collider2D target = Physics2D.OverlapCircle(transform.position, 100, 1 << LayerMask.NameToLayer("Enemy"));

            if (target != null)
            {
                attractedTarget = target.gameObject;
            }
        }

        if (effects != null) {
            foreach (Effect effect in effects)
            {
                switch (effect)
                {
                    case Effect.Attraction:
                        if (attractedTarget != null) {
                            if (attractedTarget.activeSelf)
                            {
                                Vector2 targetDir = (attractedTarget.transform.position - transform.position).normalized;

                                Orientation = Vector2.Lerp(Orientation, targetDir, Time.deltaTime * 4);
                                transform.Rotate(0, 0, 1200 * Time.deltaTime);
                            }
                        }
                        break;

                    case Effect.Reflective:
                        MaxBounces = 1;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
