using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackModule : Gun
{
    static AttackModule instance;
    Movement movementManager;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        if (player != null) {
            player.OnEntityEvent += OnPlayerEvent;
        }
    }

    private void OnDisable()
    {
        if (player != null) {
            player.OnEntityEvent -= OnPlayerEvent;
        }
    }

    void Start()
    {
        Initialize();
        SetVelocity(600);
        SetFireSound(Sound.Shoot);

        player.OnEntityEvent += OnPlayerEvent;

        movementManager = Movement.GetInstance();
    }

    void Update()
    {
        if (Controls.GetAttackKeyDown())
        {
            Fire(transform.rotation.eulerAngles.z);
        }
    }

    protected override void FireAction(float angle)
    {
        FireBullet(angle);
        camManager.ShakeAnimation(1f, .05f);
        movementManager.Recoil();
    }

    protected override void ModifyProjectile(Projectile projectile)
    {
        projectile.SetEffects(TransferEffects());
    }

    void OnPlayerEvent(Entity sender, Entity.EntityEvent eventType, object param)
    {
        if (eventType == PlayerModule.PlayerEvent.ItemPicked)
        {
            ItemData item = (ItemData)param;
            AddEffectFromItem(item.ID);
        }
    }

    void AddEffectFromItem(ItemID itemID)
    {
        switch (itemID)
        {
            case ItemID.Drill:
                AddEffect(Projectile.Effect.Perforing);
                break;

            case ItemID.Molotovic:
                AddEffect(Projectile.Effect.Burning);
                break;

            default:
                break;
        }
    }

    public static AttackModule GetInstance()
    {
        return instance;
    }
}
