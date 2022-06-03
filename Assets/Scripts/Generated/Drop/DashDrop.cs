using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashDrop : Drop
{

    protected override void InitDrop()
    {
        dropType = DropType.ChargeUnit;
    }

    protected override void ActivateDrop()
    {
        playerModule.AddGadgetUnit();
    }
}
