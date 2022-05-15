using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : EnemyModule
{

    protected override void InitEnemy()
    {
        SetEnemyType(EnemyType.Slime);
        InitHealth(2);

        maxCarbonUnits = 2;
    }

    protected override void OnEnemyDead()
    {
        SoundManager.Play(Sound.ShipDeath, transform.position);
    }
}
