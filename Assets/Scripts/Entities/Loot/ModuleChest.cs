using UnityEngine;

public class ModuleChest : Chest
{
    protected override void InitEntity()
    {
        base.InitEntity();

        dropType = DropType.Item;
        Price = 4;
    }

    public void SetItem(ItemData item)
    {
        drops = LevelManager.GetInstance().Drops;
        actualDrops = new Drop[1];
        actualDrops[0] = Instantiate(drops[dropType], transform.position, Quaternion.identity).GetComponent<Drop>();
        actualDrops[0].gameObject.SetActive(false);
        ((ItemDrop)actualDrops[0]).SetItem(item);
    }
}
