using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Numbrian : EnemyModule
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
    Quaternion shootingDirection;

    public LayerMask playerDetectionLayer;

    float playerDetectionRadius = 300;
    float bumpRadius = 10;
    float shootingAccuracy = .8f;
    float cooldown = .4f;
    float shootingDelta;

    bool playerSeen = false;
    bool hasShoot = false;

    protected override void InitEnemy()
    {
        mover = gameObject.AddComponent<Mover>();
        shooter = gameObject.AddComponent<EnemyShooter>();
        stateMachine = gameObject.AddComponent<StateMachine>();

        player = (PlayerModule)PlayerModule.GetInstance();

        mover.InitMover(100);
        shooter.Init(cooldown, 200, 4, false);

        SetEnemyType(EnemyType.Numbrian);
        InitHealth(3);

        maxCarbonUnits = 4;
        shootingDelta = 180 * (1 - shootingAccuracy);

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

        BounceOffWalls();
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

    Vector2 DirectionToPlayer()
    {
        return (player.transform.position - transform.position).normalized;
    }

    Quaternion RotationToPlayer()
    {
        return Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, DirectionToPlayer()));
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

    void BounceOffWalls()
    {
        if (BumpTop() && BumpLeft())
        {
            moveDirection = Mathf.Abs(moveDirection.x) < Mathf.Abs(moveDirection.y) ? Vector2.down : Vector2.right;
        }
        else if (BumpTop() && BumpRight())
        {
            moveDirection = Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y) ? Vector2.down : Vector2.left;
        }
        else if (BumpBottom() && BumpLeft())
        {
            moveDirection = Mathf.Abs(moveDirection.y) < Mathf.Abs(moveDirection.x) ? Vector2.up : Vector2.right;
        }
        else if (BumpBottom() && BumpRight())
        {
            moveDirection = Mathf.Abs(moveDirection.y) < Mathf.Abs(moveDirection.x) ? Vector2.up : Vector2.left;
        }
        else if (BumpLeft() || BumpRight())
        {
            moveDirection.x = 0;
            moveDirection.Normalize();
        }
        else if (BumpTop() || BumpBottom())
        {
            moveDirection.y = 0;
            moveDirection.Normalize();
        }

        if (moveDirection == Vector2.zero)
        {
            moveDirection = Random.insideUnitCircle.normalized;
        }
    }

    bool BumpTop()
    {
        return Physics2D.Raycast(transform.position, Vector2.up, bumpRadius, LayerMask.GetMask("Scenario"));
    }

    bool BumpBottom()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, bumpRadius, LayerMask.GetMask("Scenario"));
    }

    bool BumpLeft()
    {
        return Physics2D.Raycast(transform.position, Vector2.left, bumpRadius, LayerMask.GetMask("Scenario"));
    }

    bool BumpRight()
    {
        return Physics2D.Raycast(transform.position, Vector2.right, bumpRadius, LayerMask.GetMask("Scenario"));
    }

    protected override void OnEnemyDead()
    {
        SoundManager.Play(Sound.ShipDeath, transform.position);
        CamManager.GetInstance().ShockGame(.05f);
        CamManager.GetInstance().ShakeQuake(5, 1f, false);
    }
}
