using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Shooter
{
    public GameObject muzzleGO;
    private GameObject muzzleInstance;

    protected PlayerModule player;
    protected CamManager camManager;

    private bool locked = false;

    private Sound fireSound;

    public override void Initialize()
    {
        base.Initialize();

        enemyBullet = false;

        player = (PlayerModule)PlayerModule.GetInstance();
        camManager = CamManager.GetInstance();

        if (muzzleInstance == null)
        {
            muzzleInstance = Instantiate(muzzleGO, null);
            muzzleInstance.transform.localPosition = Vector2.zero;
            muzzleInstance.SetActive(false);
        }
    }

    public void Fire(float angle)
    {
        if (!locked && CanShoot()) {
            SoundManager.Play(fireSound, camManager.transform.position, camManager.transform);

            StartCoroutine(MuzzleFlash(2));
            FireAction(angle);
        }
    }

    protected abstract void FireAction(float angle);

    protected void SetFireSound(Sound sound)
    {
        fireSound = sound;
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    //Extract effects based on the items from an specific inventory
    protected Projectile.Effect[] TransferEffects()
    {
        List<Projectile.Effect> effects = new List<Projectile.Effect>();

        foreach (Projectile.Effect effect in ProjectileEffects)
        {
            effects.Add(effect);
        }

        return effects.ToArray();
    }

    IEnumerator MuzzleFlash(int frames)
    {
        muzzleInstance.transform.position = projectileStartingPos.position + transform.forward * 6;

        muzzleInstance.SetActive(true);

        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        muzzleInstance.SetActive(false);
    }
}

