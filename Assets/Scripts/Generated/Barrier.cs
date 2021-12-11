using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : Entity
{
    protected override void InitEntity()
    {
        SetEntityType(EntityType.Barrier);
        InitHealth(1);

        units = 0;
    }

    protected override void Start()
    {
        base.Start();
        transform.Rotate(Vector3.forward, Random.Range(-10, 10));
    }

    protected override void Die()
    {
        CamManager.GetInstance().ShakeQuake(4, 2.5f, false);
        SoundManager.Play(Sounds.Break, transform.position);
        base.Die();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
