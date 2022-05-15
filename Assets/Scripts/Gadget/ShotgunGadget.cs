using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunGadget : Gadget
{
    protected override void Start()
    {
        base.Start();

        SetUsesLeft(GetMaxUses());
    }

    protected override void Update()
    {

        canUse = Time.time > finishCooldownTime;

        base.Update();
    }

    protected override void Use()
    {
        finishCooldownTime = Time.time + cooldown;
        rechargeCooldownTime = finishCooldownTime + rechargeCooldown;

        SpendUse();

        SoundManager.Play(Sound.Break, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

        if (GetUsesLeft() == 0)
        {
            exhausted = true;
        }
    }
}
