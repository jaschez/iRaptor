using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackModule : Shooter
{

    static AttackModule instance;

    public GameObject shootgo;
    GameObject muzzleInstance;

    PlayerModule player;

    CamManager camManager;
    Movement movementManager;

    bool locked = false;


    private void Awake()
    {
        instance = this;
    }

    protected override void Start()
    {
        base.Start();

        enemyBullet = false;
        velocity = 600;

        player = (PlayerModule)PlayerModule.GetInstance();
        camManager = CamManager.GetInstance();
        movementManager = Movement.GetInstance();

        if (muzzleInstance == null)
        {
            muzzleInstance = Instantiate(shootgo, null);
            muzzleInstance.transform.localPosition = Vector2.zero;
            muzzleInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (Controls.GetAttackKeyDown() && CanShoot() && !locked)
        {

            SoundManager.Play(Sounds.Shoot, camManager.transform.position, camManager.transform);

            PlayerFire();

        }
    }

    void PlayerFire()
    {
        StartCoroutine(MuzzleFlash(3));

        Fire(player.GetBuffIndexes());
        player.UseBuffs();

        movementManager.Recoil();

        camManager.ShakeSingle(5);
        camManager.Recoil();
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    public static AttackModule GetInstance()
    {
        return instance;
    }

    IEnumerator MuzzleFlash(int frames)
    {
        muzzleInstance.transform.position = projectileStartingPos.position + transform.forward * 6;

        muzzleInstance.SetActive(true);

        for (int i = 0; i < frames; i++) {
            yield return new WaitForEndOfFrame();
        }
        
        muzzleInstance.SetActive(false);
    }
}
