using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : EnemyModule
{

    protected override void InitEnemy()
    {
        SetEnemyType(EnemyType.Egg);
        InitHealth(10);

        maxCarbonUnits = 8;
    }

    /*protected override void OnTakeDamage(int dmg)
    {
        //UIVisualizer.GetInstance().PopUp(PopUpType.Bad, "1", transform);
    }*/

    protected override void OnEnemyDead()
    {
        //CamManager.GetInstance().ShakeQuake(6, 2f, false);
        CamManager.GetInstance().ShockGame(.1f);
        //levelManager.AddBeatenEgg();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
