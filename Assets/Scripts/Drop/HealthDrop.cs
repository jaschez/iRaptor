using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrop : Drop
{

    private int hp;

    protected override void InitDrop()
    {
        dropType = DropType.HP;
        hp = 3;
    }

    protected override void ActivateDrop()
    {
        playerModule.Heal(hp);
    }
}
