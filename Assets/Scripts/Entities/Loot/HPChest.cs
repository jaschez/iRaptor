using UnityEngine;

public class HPChest : Chest
{
    protected override void InitEntity()
    {
        base.InitEntity();
        dropType = DropType.HP;
    }
}