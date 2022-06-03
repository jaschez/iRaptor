using UnityEngine;

public class ChargeChest : Chest
{
    protected override void InitEntity()
    {
        base.InitEntity();
        dropType = DropType.ChargeUnit;
    }
}