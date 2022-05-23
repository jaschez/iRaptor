using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Numbrian : EnemyModule
{
    //IA

    State patrolIdle;
    State patrolWandering;

    State alertIdle;
    State alertWandering;
    State alertAttack;

    //Shooting parameters

    EnemyShooter shooter;

    Quaternion rotationToPlayer;
    Quaternion shootingDirection;

    float shootingAccuracy = .8f;
    float cooldown = .4f;
    float shootingDelta;

    bool hasShoot = false;

    protected override void InitEnemy()
    {
        base.InitEnemy();

        shooter = gameObject.AddComponent<EnemyShooter>();
        shooter.Init(cooldown, 200, 4, ProjectileType.Numbrian, false);
        shootingDelta = 180 * (1 - shootingAccuracy);

        SetupValues(EnemyType.Numbrian, 3, 100, 4);

        //MAQUINA DE ESTADOS

        //Creacion de los estados

        //Estados de patrulla
        patrolIdle = new State(Idle, StateMachine.States.Idle, .5f, 2f);
        patrolWandering = new State(Wandering, StateMachine.States.Wandering, .6f, 1.5f);

        //Estados de alerta
        alertIdle = new State(Idle, StateMachine.States.Idle, .3f, .5f);
        alertWandering = new State(Wandering, StateMachine.States.Wandering, .5f, .8f);
        alertAttack = new State(Attack, StateMachine.States.Attack, cooldown, cooldown * 1.2f);

        //Asignacion de las transiciones habituales de estados
        patrolIdle.AddNextState(patrolWandering);
        patrolWandering.AddNextState(patrolIdle);

        alertIdle.AddNextState(alertIdle);
        alertIdle.AddNextState(alertAttack);
        alertIdle.AddNextState(alertWandering);
        alertWandering.AddNextState(alertIdle);
        alertWandering.AddNextState(alertWandering);
        alertWandering.AddNextState(alertAttack);
        alertAttack.AddNextState(alertWandering);
        alertAttack.AddNextState(alertIdle);
        alertAttack.AddNextState(alertAttack);

        //Asignacion de las transiciones condicionales de estados
        patrolWandering.AddNextState(alertWandering, IsLookingAtPlayer);
        patrolIdle.AddNextState(alertWandering, 
            ()=> {return IsLookingAtPlayer(); });

        alertIdle.AddNextState(patrolIdle, () => !IsLookingAtPlayer());

        //Asignamos estados iniciales
        initialStates.Add(patrolIdle);
        initialStates.Add(patrolWandering);

        StateMachine.Init(initialStates);

        //Añadir sonidos de enemigo
        //AddTalkingSound(Sound.Numbrian_talk1);
        //AddTalkingSound(Sound.Numbrian_talk2);
    }

    protected override void OnStateStart(State startingState)
    {
        base.OnStateStart(startingState);

        switch (startingState.StateType)
        {
            case StateMachine.States.Attack:
                rotationToPlayer = RotationToPlayer();
                hasShoot = false;
                break;

            default:
                break;
        }
    }

    protected override void OnTakeDamage(int dmg)
    {
        base.OnTakeDamage(dmg);
        Talk(Sound.Numbrian_damage, true);
    }

    void Attack()
    {
        if (!hasShoot)
        {
            shootingDirection = Quaternion.Euler(0, 0, rotationToPlayer.eulerAngles.z + Random.Range(-shootingDelta, shootingDelta));

            shooter.FireShooter(shootingDirection);
            hasShoot = true;
        }
    }

    protected override void OnEnemyDead()
    {
        Talk(Sound.Numbrian_death);
    }
}
