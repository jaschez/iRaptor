using UnityEngine;

public class Bullet : MonoBehaviour, IPooledObject
{

    Vector2 orientation;

    int damage = 0;
    int hitNumber = 0;
    int maxHitNumber = 1;

    float velocity = 600f;

    bool enemyBullet = false;

    Effect[] effects;

    void Start()
    {

    }

    //Método perteneciente a la interfaz del Object Pooling, se encarga de crear una nueva bala en la escena
    public void OnObjectSpawn()
    {
        orientation = Quaternion.Euler(0, 0, transform.rotation.z) * Vector2.up;
    }

    //Método que inicializa los valores del proyectil una vez creado en la escena
    public void InitBullet(float velocity, int damage, bool enemyBullet)
    {
        this.velocity = velocity;
        this.damage = damage;
        this.enemyBullet = enemyBullet;

        hitNumber = 0;
        maxHitNumber = 1;
    }

    public void SetEffects(Effect[] bulletEffects)
    {
        effects = bulletEffects;
    }

    void FixedUpdate()
    {
        transform.Translate(orientation * velocity * Time.deltaTime);
    }

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
                otherEntity.TakeDamage(damage);

                ApplyEffects(otherEntity);

                hitNumber++;

                if (hitNumber >= maxHitNumber)
                {
                    Impact();
                }
            }
        }
        else
        {
            if (other.tag == "Scenario")
            {
                Impact();
                //Do nothing    
            }
        }

    }

    //Método que define el comportamiento de lo que pasa al impactar con un cuerpo
    void Impact()
    {
        gameObject.SetActive(false);
    }

    //Apply corresponding effects set by the shooter
    void ApplyEffects(Entity entity)
    {
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

    bool IsAttackingHimself(Entity otherEntity)
    {
        return (otherEntity.GetEntityType() == EntityType.Player && !enemyBullet) ||
            (otherEntity.GetEntityType() == EntityType.Enemy && enemyBullet);
    }

    //ENEMIES DEBUFFS

    void Burn()
    {

    }

    public enum Effect
    {
        Burning,
        Perforing
    }
}