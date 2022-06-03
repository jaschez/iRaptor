using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Entity : MonoBehaviour
{

    protected Dictionary<DropType, GameObject> drops;
    protected Drop[] actualDrops;

    Material spriteMat;
    Material whiteMat;

    SpriteRenderer sr;

    Color origColor;

    protected DropType dropType = DropType.CarbonUnit;

    protected LevelManager levelManager;

    private EntityType entityType;

    protected int units = 1;

    private int maxHealth = 8;

    private int _health;

    protected float takingDamageCooldown = -1;
    private float takingDamageCooldownTime = 0;

    public delegate void EntityEvents(Entity sender, EntityEvent eventType, object param);

    public event EntityEvents OnEntityEvent;

    //Propiedad que gestiona los puntos de salud disponibles del jugador
    private int health
    {
        get
        {
            return _health;
        }

        set
        {
            if (value < 0)
            {
                _health = 0;
            }
            else if (value > maxHealth)
            {
                _health = maxHealth;
            }
            else
            {
                _health = value;
            }
        }
    }

    protected abstract void InitEntity();

    //Evento que realiza cada entidad concreta
    protected virtual void OnTakeDamage(int dmg) { }
    protected virtual void OnHeal() {  }

    protected virtual void Die()
    {
        DropStuff();

        gameObject.SetActive(false);

        SendEvent(EntityEvent.Death);
    }

    //Se llama al método antes que Start y antes de que se inicialicen los objectos
    protected virtual void Awake()
    {

        InitEntity();

        SendEvent(EntityEvent.Creation);
    }

    protected virtual void Start()
    {
        if (!TryGetComponent(out sr))
        {
            sr = GetComponentInChildren<SpriteRenderer>(true);
        }

        spriteMat = sr.material;
        whiteMat = new Material(Shader.Find("GUI/Text Shader"));
        origColor = sr.color;

        levelManager = LevelManager.GetInstance();

        if (entityType != EntityType.Player) {
            if (actualDrops == null) {
                drops = levelManager.Drops;
                actualDrops = new Drop[units];
                GameObject actualDrop;

                for (int i = 0; i < units; i++)
                {
                    actualDrop = Instantiate(drops[dropType], transform.position, Quaternion.identity);
                    actualDrop.SetActive(false);

                    actualDrops[i] = actualDrop.GetComponent<Drop>();
                }
            }
        }
    }

    protected void SetEntityType(EntityType entityType)
    {
        this.entityType = entityType;
    }

    protected virtual void SendEvent(EntityEvent e, object param = null)
    {
        OnEntityEvent?.Invoke(this, e, param);
    }

    //Métodos que inicializan la salud de la entidad

    protected void InitHealth(int maxHp, int hp)
    {
        maxHealth = maxHp;
        health = hp;
    }

    protected void InitHealth(int maxHp)
    {
        maxHealth = maxHp;
        health = maxHealth;
    }

    public void LoadHealth(int hp)
    {
        health = hp;
    }

    //Método que resta vida al jugador
    public void TakeDamage(int damage)
    {
        if (takingDamageCooldownTime < Time.time) {
            if (health > 0) {
                health -= damage;

                Flash();

                SendEvent(EntityEvent.Damage, damage);

                OnTakeDamage(damage);

                if (health == 0)
                {
                    Invoke("Die", .07f);
                }
            }

            takingDamageCooldownTime = Time.time + takingDamageCooldown;
        }
    }

    public void Flash()
    {
        StartCoroutine(DamageFlash(.07f));
    }

    void Flinch()
    {
        Transform render = sr.transform;
        
        render.DOScaleX(.8f, .05f).OnComplete(() =>
        {
            render.DOScaleX(1f, .1f).SetEase(Ease.OutElastic);
        });
        render.DOScaleY(1.2f, .05f).OnComplete(() =>
        {
            render.DOScaleY(1f, .1f).SetEase(Ease.OutElastic);
        });
    }

    //Método que le añade puntos de salud a la entidad
    public void Heal(int hp)
    {
        health += hp;

        SendEvent(EntityEvent.Heal, hp);

        OnHeal();
    }

    //Método que devuelve la salud actual de la entidad
    public int GetHP()
    {
        return health;
    }

    public int GetMaxHP()
    {
        return maxHealth;
    }

    private void DropStuff()
    {
        if (actualDrops != null) {
            foreach (Drop drop in actualDrops)
            {
                GameObject dropGO = drop.gameObject;
                dropGO.transform.position = transform.position;
                dropGO.SetActive(true);
            }
        }
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return sr;
    }

    public EntityType GetEntityType()
    {
        return entityType;
    }

    IEnumerator DamageFlash(float flashTime)
    {
        while (sr == null && whiteMat == null) yield return null;

        Flinch();

        sr.material = whiteMat;
        sr.color = Color.white;

        yield return new WaitForSeconds(flashTime);

        sr.material = spriteMat;
        sr.color = origColor;

    }

    public class EntityEvent
    {
        public static readonly EntityEvent Creation = new EntityEvent("Creation");
        public static readonly EntityEvent Heal = new EntityEvent("Heal");
        public static readonly EntityEvent Damage = new EntityEvent("Damage");
        public static readonly EntityEvent Death = new EntityEvent("Death");

        static List<EntityEvent> entityList;// = new List<EntityEvent>();
        public static int Size { get; private set; } = 0;
        readonly int intValue;

        readonly string strValue;

        protected EntityEvent(string name)
        {
            if (entityList == null)
            {
                entityList = new List<EntityEvent>();
            }

            strValue = name;
            intValue = Size;

            entityList.Add(this);

            Size = entityList.Count;
        }

        public override string ToString()
        {
            return strValue;
        }

        public static implicit operator int(EntityEvent eEvent){
            return eEvent.intValue;
        }

        public static explicit operator EntityEvent(int index)
        {
            if (index < Size)
            {
                return entityList[index];
            }
            else
            {
                return null;
            }
        }
    }
}

public enum EntityType
{
    Player,
    Enemy,
    Chest,
    Barrier
}
