using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyModule : Entity
{

    private EnemyType enemyType;

    WaveManager waveManager;

    protected PlayerModule player;
    public Mover MoverComponent { get; protected set; }
    public StateMachine StateMachine { get; protected set; }
    protected List<State> initialStates;

    List<Sound> talkingSounds;

    protected Vector2 moveDirection;

    protected int maxCarbonUnits = 5;

    protected float playerDetectionRadius = 300;
    protected float bumpRadius = 10;

    float talkingTime;
    float talkingDelay = .2f;

    protected bool damaged = false;
    protected bool burning = false;
    protected bool CanTalk = true;
    public bool PlayerSeen { get; protected set; } = false;

    protected LayerMask playerDetectionLayer;

    protected abstract void OnEnemyDead();

    protected virtual void InitEnemy()
    {
        player = (PlayerModule)PlayerModule.GetInstance();
        StateMachine = gameObject.AddComponent<StateMachine>();
        MoverComponent = gameObject.AddComponent<Mover>();

        initialStates = new List<State>();
        playerDetectionLayer = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Scenario"));

        talkingSounds = new List<Sound>();
    }

    protected virtual void Update()
    {
        if (talkingTime < Time.time)
        {
            if (CanTalk && talkingSounds != null) {
                if (talkingSounds.Count > 0) {
                    Talk(talkingSounds[Random.Range(0, talkingSounds.Count)]);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (StateMachine != null)
        {
            StateMachine.OnStateStart += OnStateStart;
        }
    }

    private void OnDisable()
    {
        if (StateMachine != null) {
            StateMachine.OnStateStart -= OnStateStart;
        }
    }

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Enemy);
        InitEnemy();

        waveManager = WaveManager.GetInstance();

        dropType = DropType.CarbonUnit;
    }

    protected void SetupValues(EnemyType enemyType, int healthPoints, float velocity, int maxCarbonDrop)
    {
        SetEnemyType(enemyType);
        InitHealth(healthPoints);
        MoverComponent.InitMover(velocity);
        maxCarbonUnits = maxCarbonDrop;

        units = Random.Range((int)(maxCarbonUnits * .7f), maxCarbonUnits);
    }

    protected void AddTalkingSound(Sound sound)
    {
        talkingSounds.Add(sound);
    }

    protected void Talk(Sound sound, bool repetitive = false)
    {
        if (!repetitive) {
            SoundManager.Play(sound, transform.position);
        }
        else
        {
            if (talkingTime < Time.time)
            {
                SoundManager.Play(sound, transform.position);
                talkingTime = Time.time + talkingDelay;
            }
        }
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

    //Event driven methods

    protected virtual void OnStateStart(State startingState)
    {
        if (startingState.StateType == StateMachine.States.Wandering)
        {
            moveDirection = Random.insideUnitCircle.normalized;
        }
    }

    //Utility methods

    protected Vector2 DirectionToPlayer()
    {
        return (player.transform.position - transform.position).normalized;
    }

    protected Quaternion RotationToPlayer()
    {
        return Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, DirectionToPlayer()));
    }

    protected bool IsLookingAtPlayer()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, DirectionToPlayer(), playerDetectionRadius, playerDetectionLayer);
        
        if (ray.collider != null)
        {
            PlayerSeen = ray.collider.tag.CompareTo("Player") == 0;
        }
        else
        {
            PlayerSeen = false;
        }

        return PlayerSeen;
    }

    //Default behaviours
    protected virtual void Idle()
    {
        MoverComponent.Move(Vector2.zero);
    }

    protected virtual void Wandering()
    {
        MoverComponent.Move(moveDirection);

        BounceOffWalls();
    }

    protected void BounceOffWalls()
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

    //End of default behaviours

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