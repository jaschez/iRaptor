using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyModule : Entity
{

    private EnemyType enemyType;

    WaveManager waveManager;

    protected int maxCarbonUnits = 5;

    protected bool damaged = false;
    protected bool burning = false;

    protected abstract void InitEnemy();

    protected abstract void OnEnemyDead();

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Enemy);
        InitEnemy();

        waveManager = WaveManager.GetInstance();

        dropType = DropType.CarbonUnit;

        units = Random.Range((int)(maxCarbonUnits*.7f), maxCarbonUnits);
    }

    public void Burn(int damage, float time)
    {
        if (!burning)
        {
            burning = true;
            StartCoroutine(BurnCoroutine(damage, time));
        }
    }

    protected override void Die()
    {
        CamManager.GetInstance().ShakeAnimation(10, .03f, 1);
        CamManager.GetInstance().ShockGame(.05f);

        OnEnemyDead();

        waveManager.AddBeatenEnemy();

        base.Die();
    }

    protected override void OnTakeDamage(int dmg)
    {
        SoundManager.Play(Sound.EnemyImpact, transform.position);
        UIVisualizer.GetInstance().PopUp(PopUpType.Bad, dmg.ToString(), transform);

        if (!damaged && GetHP() != 0)
        {
            damaged = true;
            UIVisualizer.GetInstance().CreateEnemyHealthUI(this);
        }
    }

    IEnumerator BurnCoroutine(int dmg, float time)
    {
        float endTime = Time.time + time;

        while (Time.time < endTime)
        {
            TakeDamage(dmg);
            yield return new WaitForSeconds(.2f);
        }

        burning = false;

        yield return null;
    }

    protected void SetEnemyType(EnemyType enemyType)
    {
        this.enemyType = enemyType;
    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }
}

public enum EnemyType
{
    Numbrian,
    Slime,
    Fairy
}