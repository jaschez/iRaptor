using UnityEngine;

public class Shotgun : Gun
{
    public override void Initialize()
    {
        base.Initialize();

        SetVelocity(400);
        SetCooldown(.1f);
        SetProjectileType(ProjectileType.Shell);
        SetFireSound(Sound.Break);
    }

    protected override void FireAction(float angle)
    {
        for (int i = 0; i < 6; i++) {
            FireBullet(angle + Random.Range(-30,30));
        }
        camManager.ShakeAnimation(2f, .2f, 30);
    }
}
