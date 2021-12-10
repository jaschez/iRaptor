using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warship : EnemyModule
{
    EnemyShooter shooter;
    Mover mover;

    PlayerModule player;

    //IA
    StateMachine stateMachine;
    List<State> initialStates;

    State patrolIdle;
    State patrolWandering;

    State alertIdle;
    State alertWandering;
    State alertAttack;

    Vector2 moveDirection;
    Quaternion rotationToPlayer;

    public LayerMask playerDetectionLayer;
    float playerDetectionRadius = 300;
    bool playerSeen = false;
    bool hasShoot = false;

    protected override void InitEnemy()
    {
        mover = gameObject.AddComponent<Mover>();
        shooter = gameObject.AddComponent<EnemyShooter>();
        stateMachine = gameObject.AddComponent<StateMachine>();

        player = (PlayerModule)PlayerModule.GetInstance();

        mover.InitMover(60);
        shooter.Init(.8f, 4, false);

        SetEnemyType(EnemyType.Warship);
        InitHealth(3);

        maxCarbonUnits = 4;

        //MAQUINA DE ESTADOS

        //Creacion de los estados

        //Estados de patrulla
        patrolIdle = new State(Idle, StateMachine.States.Idle, .5f, 3f);
        patrolWandering = new State(Wandering, StateMachine.States.Wandering, .5f, 1f);

        //Estados de alerta
        alertIdle = new State(Idle, StateMachine.States.Idle, .3f, .7f);
        alertWandering = new State(Wandering, StateMachine.States.Wandering, .3f, .7f);
        alertAttack = new State(Attack, StateMachine.States.Attack, 0f, 1f);

        //Asignacion de las transiciones habituales de estados
        patrolIdle.AddNextState(patrolWandering);
        patrolWandering.AddNextState(patrolIdle);

        alertIdle.AddNextState(alertAttack);
        alertIdle.AddNextState(alertWandering);
        alertWandering.AddNextState(alertIdle);
        alertWandering.AddNextState(alertAttack);
        alertAttack.AddNextState(alertWandering);
        alertAttack.AddNextState(alertIdle);

        //Asignamos estados iniciales
        initialStates = new List<State>();
        initialStates.Add(patrolIdle);
        initialStates.Add(patrolWandering);

        stateMachine.Init(initialStates, OnStateStart);
    }

    void Update()
    {

    }

    void OnStateStart()
    {
        moveDirection = Random.insideUnitCircle.normalized;
        rotationToPlayer = RotationToPlayer();
        hasShoot = false;
    }

    void Idle()
    {
        mover.Move(Vector2.zero);

        if (patrolIdle.TriggerState == null && alertIdle.TriggerState == null) {
            if (!playerSeen)
            {
                if (IsLookingAtPlayer())
                {
                    playerSeen = true;
                    stateMachine.ResetTimer(alertIdle.MinCompletionTime, alertIdle.MaxCompletionTime);
                    patrolIdle.SetTriggerState(alertWandering);
                }
            }
            else
            {
                if (!IsLookingAtPlayer())
                {
                    playerSeen = false;
                    stateMachine.ResetTimer(patrolIdle.MinCompletionTime, patrolIdle.MaxCompletionTime);
                    alertIdle.SetTriggerState(patrolIdle);
                }
            }
        }
    }

    void Wandering()
    {
        mover.Move(moveDirection);
    }

    void Attack()
    {
        if (!hasShoot)
        {
            shooter.FireShooter(rotationToPlayer);
            hasShoot = true;
        }
    }

    Vector2 DirectionToPlayer()
    {
        return (player.transform.position - transform.position).normalized;
    }

    Quaternion RotationToPlayer()
    {
        return Quaternion.LookRotation(DirectionToPlayer(), Vector2.up);
    }

    bool IsLookingAtPlayer()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, DirectionToPlayer(), playerDetectionRadius, playerDetectionLayer);
        if (ray.collider != null) {
            return ray.collider.tag.CompareTo("Player") == 0;
        }
        else
        {
            return false;
        }
    }

    protected override void OnEnemyDead()
    {
        SoundManager.Play(Sounds.ShipDeath, transform.position);
        CamManager.GetInstance().ShockGame(.05f);
        CamManager.GetInstance().ShakeQuake(5, 1f, false);
    }
}
