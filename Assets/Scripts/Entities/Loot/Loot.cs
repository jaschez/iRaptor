using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : Entity
{

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Loot);
        InitHealth(1);

        dropIndex = (int)DropType.PowerUp;

        if (dropIndex == (int)DropType.CarbonUnit)
        {
            units = Random.Range(10, 14);
        }
    }

    protected override void Start()
    {
        base.Start();

        transform.Rotate(Vector3.forward, Random.Range(-180, 180));
    }

    protected override void Die()
    {

        //CamManager.GetInstance().ShakeQuake(4, 2.5f, false);
        SoundManager.Play(Sounds.Break, transform.position);
        base.Die();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
