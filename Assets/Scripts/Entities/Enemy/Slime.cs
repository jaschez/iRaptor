using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : EnemyModule
{

    float damageCooldown;
    float damageCooldownTime;

    State wandering;
    State following;

    Vector2 playerDirection;
    
    protected override void InitEnemy()
    {
        base.InitEnemy();
        SetupValues(EnemyType.Slime, 1, 50, 2);

        damageCooldown = .5f;

        wandering = new State(Wandering, StateMachine.States.Wandering, .6f, 1.5f);
        following = new State(Follow, StateMachine.States.Attack, .3f, .8f);

        wandering.AddNextState(wandering);
        following.AddNextState(following);

        wandering.AddNextState(following, IsLookingAtPlayer);
        following.AddNextState(wandering, () => !IsLookingAtPlayer());

        initialStates.Add(wandering);

        StateMachine.Init(initialStates);
    }

    protected override void OnStateStart(State startingState)
    {
        base.OnStateStart(startingState);

        if (startingState.StateType == StateMachine.States.Attack)
        {
            playerDirection = DirectionToPlayer();
        }
    }

    protected override void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 3f, (1 << LayerMask.NameToLayer("Player")));

        if (hit != null)
        {
            if (damageCooldownTime < Time.time)
            {
                Entity player = hit.gameObject.GetComponent<Entity>();

                player.TakeDamage(1);
                damageCooldownTime = Time.time + damageCooldown;
            }
        }
    }

    void Follow()
    {
        MoverComponent.Move(playerDirection);
        BounceOffWalls();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (damageCooldownTime < Time.time)
            {
                Entity player = collision.gameObject.GetComponent<Entity>();

                player.TakeDamage(1);
                damageCooldownTime = Time.time + damageCooldown;
            }
        }
    }

    protected override void OnEnemyDead()
    {
        SoundManager.Play(Sound.ShipDeath, transform.position);
    }
}
