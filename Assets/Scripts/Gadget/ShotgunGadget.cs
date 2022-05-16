using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunGadget : Gadget
{
    Shotgun shotgun;

    protected override void Start()
    {
        base.Start();

        shotgun = gameObject.AddComponent<Shotgun>();
        shotgun.projectileStartingPos = AttackModule.GetInstance().projectileStartingPos;
        shotgun.muzzleGO = AttackModule.GetInstance().muzzleGO;
        shotgun.Initialize();
    }

    protected override void Use()
    {
        SpendUse();
        shotgun.Fire(transform.rotation.eulerAngles.z);
    }
}
