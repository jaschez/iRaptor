using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenDrop : Drop
{

    protected override void InitDrop()
    {
        dropType = DropType.CarbonUnit;
    }

    protected override void ActivateDrop()
    {
        playerModule.AddCarbonUnits(1);
    }
}
