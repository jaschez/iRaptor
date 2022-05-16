using UnityEngine;

public abstract class Projectile : MonoBehaviour, IPooledObject
{
    int hitNumber = 0;
    int maxHitNumber = 1;

    public Vector2 Orientation { get; private set; }
    public int Damage { get; private set; }
    public int MaxBounces { get; protected set; }
    public int CurrentBounces { get; protected set; }
    public float Velocity { get; protected set; }

    public ProjectileType BulletT { get; private set; }

    public bool EnemyBullet { get; private set; } = false;

    Effect[] effects;

    //Método perteneciente a la interfaz del Object Pooling, se encarga de crear una nueva bala en la escena
    public void OnObjectSpawn()
    {
        Orientation = Quaternion.Euler(0, 0, transform.rotation.z) * Vector2.up;
    }

    //Método que inicializa los valores del proyectil una vez creado en la escena
    public virtual void InitBullet()
    {
        hitNumber = 0;
        maxHitNumber = 1;
        CurrentBounces = 0;
    }

    public void SetEffects(Effect[] bulletEffects)
    {
        effects = bulletEffects;
    }

    void FixedUpdate()
    {
        MovementUpdate();
    }

    protected abstract void MovementUpdate();

    //Método que se encarga de detectar posibles impactos con otros cuerpos
    //  other: Cuerpo con el que ha impactado
    void OnTriggerEnter2D(Collider2D other)
    {
        //Evaluamos qué tipo de cuerpo es con el que ha chocado para hacer una cosa u otra
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
                    Flip(other.transform);
                    CurrentBounces++;
                }
            }
        }

    }

    protected void ImpactEntity(Entity entity)
    {
        entity.TakeDamage(Damage);

        ApplyEffects(entity);

        hitNumber++;

        if (hitNumber >= maxHitNumber)
        {
            Impact();
        }
    }

    protected void Flip(Transform flipper)
    {
        Vector2 flipDir = -flipper.up;
        Quaternion flippedRotation = Quaternion.LookRotation(Vector3.forward, flipDir);

        Orientation = flippedRotation * Vector2.up;
        transform.rotation = flippedRotation;
    }

    protected virtual void OnEntityImpact(Entity entity) {
        ImpactEntity(entity);
    }

    protected virtual void OnWallImpact()
    {
        Impact();
    }

    //Método que define el comportamiento de lo que pasa al impactar con un cuerpo
    void Impact()
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
        Perforing
    }
}

public enum ProjectileType
{
    Main,
    Shell,
    Numbrian
}