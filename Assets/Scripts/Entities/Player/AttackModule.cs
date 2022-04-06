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

        Bullet.Effect[] bulletEffects = GetInventoryEffects(player.GetInventory());

        Fire(bulletEffects);

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

    //Extract effects based on the items from an specific inventory
    Bullet.Effect[] GetInventoryEffects(Inventory inventory)
    {
        List<Bullet.Effect> effects = new List<Bullet.Effect>();

        if (inventory.IsItemStored(ItemID.Drill))
        {
            if (Random.Range(0, 10) < 2) {
                effects.Add(Bullet.Effect.Perforing);
            }
        }

        if (inventory.IsItemStored(ItemID.Molotovic))
        {
            if (Random.Range(0, 10) < 2)
            {
                effects.Add(Bullet.Effect.Burning);
            }
        }

        return effects.ToArray();
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
