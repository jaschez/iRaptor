using UnityEngine;

public abstract class Projectile : MonoBehaviour, IPooledObject
{
    int hitNumber = 0;
    int maxHitNumber = 1;

    Vector3 lastStep;

    protected Rigidbody2D rb;

    public Vector2 Orientation { get; protected set; }
    public int Damage { get; private set; }
    public int MaxBounces { get; protected set; }
    public int CurrentBounces { get; protected set; }
    public float Velocity { get; protected set; }

    public ProjectileType BulletT { get; private set; }

    public bool EnemyBullet { get; private set; } = false;

    protected Effect[] effects;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //Método perteneciente a la interfaz del Object Pooling, se encarga de crear una nueva bala en la escena
    public void OnObjectSpawn()
    {
        Orientation = (transform.rotation * Vector2.up).normalized;
    }

    //Método que inicializa los valores del proyectil una vez creado en la escena
    public virtual void InitBullet()
    {
        hitNumber = 0;
        maxHitNumber = 1;
        CurrentBounces = 0;
        lastStep = transform.position;;
    }

    public void SetEffects(Effect[] bulletEffects)
    {
        effects = bulletEffects;
    }

    void FixedUpdate()
    {
        MovementUpdate();
        Move();
    }

    void Move()
    {

        Vector2 step = Time.fixedDeltaTime * Velocity * Orientation;
        CheckForCollision(step);
        lastStep = transform.position;
        transform.position = transform.position + (Vector3)step;
    }

    void CheckForCollision(Vector2 step)
    {
        int layerMask = ~((1 << gameObject.layer) | (EnemyBullet ? 0 : (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Entry"))));

        RaycastHit2D ray = Physics2D.Raycast(transform.position, Orientation, step.magnitude, layerMask);

        if (ray.collider != null)
        {
            OnImpactTrigger(ray);
        }
    }

    protected abstract void MovementUpdate();

    //Método que se encarga de detectar posibles impactos con otros cuerpos
    //  other: Cuerpo con el que ha impactado
    void OnImpactTrigger(RaycastHit2D ray)
    {
        //Evaluamos qué tipo de cuerpo es con el que ha chocado para hacer una cosa u otra
        Collider2D other = ray.collider;
        Entity otherEntity = other.GetComponent<Entity>();

        if (otherEntity != null)
        {
            if (!IsAttackingHimself(otherEntity))
            {
                OnEntityImpact(otherEntity);
            }
        }
        else
        {
            if (other.tag == "Scenario")
            {
                if (CurrentBounces >= MaxBounces) {
                    OnWallImpact();
                }
                else
                {
                    Flip(ray.normal);
                    transform.position = lastStep;

                    CurrentBounces++;
                }
            }
        }

    }

    protected void ImpactEntity(Entity entity)
    {
        if (entity.GetHP() > 0)
        {
            entity.TakeDamage(Damage);

            ApplyEffects(entity);

            hitNumber++;

            if (hitNumber >= maxHitNumber)
            {
                Impact();
            }
        }
    }

    protected void Flip(Transform flipper)
    {
        Vector2 flipDir = -flipper.up;
        Flip(flipDir);
    }

    protected void Flip(Vector2 normal)
    {
        Orientation = Vector2.Reflect(Orientation, normal);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Orientation);
    }

    protected virtual void OnEntityImpact(Entity entity) {
        ImpactEntity(entity);
    }

    protected virtual void OnWallImpact()
    {
        Impact();
    }

    //Método que define el comportamiento de lo que pasa al impactar con un cuerpo
    protected void Impact()
    {
        gameObject.SetActive(false);
    }

    //Apply corresponding effects set by the shooter
    void ApplyEffects(Entity entity)
    {
        if (effects != null) {
            foreach (Effect effect in effects)
            {
                switch (effect)
                {
                    case Effect.Burning:
                        if (entity.GetEntityType() == EntityType.Enemy)
                        {
                            EnemyModule enemy = (EnemyModule)entity;
                            enemy.Burn(1, 3);
                        }
                        break;

                    case Effect.Perforing:
                        maxHitNumber = 2;
                        break;

                    default:
                        break;
                }
            }
        }
    }

    bool IsAttackingHimself(Entity otherEntity)
    {
        return (otherEntity.GetEntityType() == EntityType.Player && !EnemyBullet) ||
            (otherEntity.GetEntityType() == EntityType.Enemy && EnemyBullet);
    }

    protected void SetVelocity(float velocity)
    {
        Velocity = velocity;
    }

    protected void SetDamage(int damage)
    {
        Damage = damage;
    }

    protected void SetEnemyBullet(bool enemyBullet)
    {
        EnemyBullet = enemyBullet;
    }

    protected void SetProjectileType(ProjectileType bulletType)
    {
        BulletT = bulletType;
    }

    protected void SetOrientation(Vector2 orientation)
    {
        Orientation = orientation;
    }

    public enum Effect
    {
        Burning,
        Perforing,
        Attraction,
        Reflective
    }
}

public enum ProjectileType
{
    Main,
    Shell,
    Numbrian
}