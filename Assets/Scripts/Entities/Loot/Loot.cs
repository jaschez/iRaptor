using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : Entity
{
    protected override void InitEntity()
    {
        SetEntityType(EntityType.Loot);
        InitHealth(1);

        dropType = DropType.Item;
    }

    protected override void Start()
    {
        base.Start();

        transform.Rotate(Vector3.forward, Random.Range(-180, 180));
    }

    public void SetItem(ItemData item)
    {
        drops = LevelManager.GetInstance().Drops;
        actualDrops = new Drop[1];
        actualDrops[0] = Instantiate(drops[dropType], transform.position, Quaternion.identity).GetComponent<Drop>();
        actualDrops[0].gameObject.SetActive(false);
        ((ItemDrop)actualDrops[0]).SetItem(item);
    }

    protected override void Die()
    {
        SoundManager.Play(Sounds.Break, transform.position);
        base.Die();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
