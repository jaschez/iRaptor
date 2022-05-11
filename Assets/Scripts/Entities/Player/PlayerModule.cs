using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModule : Entity
{

    private static Entity instance;

    private Gadget gadget;

    private Inventory inventory;
    private Dictionary<ItemData, int> items;

    private int _carbonUnits = 0;

    //Propiedad que gestiona las unidades de carbono disponibles del jugador
    private int CarbonUnits
    {
        get
        {
            return _carbonUnits;
        }

        set
        {
            if (value < 0)
            {
                _carbonUnits = 0;
            }
            else
            {
                _carbonUnits = value;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        instance = this;
    }

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Player);

        units = 0;

        InitHealth(20);

        inventory = new Inventory();

        items = new Dictionary<ItemData, int>();
    }

    //Método que se ejecuta al comenzar la escena
    protected override void Start()
    {
        base.Start();

        gadget = GetComponent<Gadget>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            SavingSystem.NewSave();
        }
    }

    //Método que devuelve la instancia actual del jugador
    public static Entity GetInstance()
    {
        return instance;
    }

    public void Lock()
    {
        Movement.GetInstance().Lock();
        AttackModule.GetInstance()?.Lock();
        gadget?.Lock();
    }

    public void Unlock()
    {
        Movement.GetInstance().Unlock();
        AttackModule.GetInstance()?.Unlock();
        gadget?.Unlock();
    }

    public PlayerState SavePlayerState()
    {
        PlayerState playerState = new PlayerState(GetHP(), CarbonUnits, gadget.GetUsesLeft(), items);

        return playerState;
    }

    public void LoadPlayerState(PlayerState playerState)
    {
        LoadHealth(playerState.hp);
        CarbonUnits = playerState.carbonUnits;
        gadget.SetUsesLeft(playerState.gadgetUses);

        items = playerState.items;

        foreach (ItemData item in items.Keys)
        {
            for (int replica = 0; replica < items[item]; replica++)
            {
                inventory.AddItem(item.ID);
            }
        }
    }

    public void AddItem(ItemData item)
    {
        inventory.AddItem(item.ID);
        SendEvent(PlayerEvent.ItemPicked, item);
    }

    public void AddCarbonUnits(int cu)
    {
        CarbonUnits += cu;

        SendEvent(PlayerEvent.AddedCU, cu);
    }

    public bool SpendCarbonUnits(int cu)
    {
        if (CarbonUnits >= cu) {
            CarbonUnits -= cu;
            SendEvent(PlayerEvent.SpentCU, cu);

            return true;
        }
        else
        {
            SendEvent(PlayerEvent.InsufficientCU, cu);
            return false;
        }
    }

    public void AddGadgetUnit()
    {
        gadget.AddGadgetUnit();

        SendEvent(PlayerEvent.AddedGadgetUse);
    }

    public void NotifyGadgetUse(int currentGadgetUses)
    {
        SendEvent(PlayerEvent.SpentGadgetUse, currentGadgetUses);
    }

    public void NotifyGadgetRecharge(int currentGadgetUses)
    {
        SendEvent(PlayerEvent.RechargedGadgetUse, currentGadgetUses);
    }

    public Inventory GetInventory()
    {
        return inventory;
    }

    public int GetCarbonUnits()
    {
        return CarbonUnits;
    }

    public int GetGadgetUnits()
    {
        return gadget.GetUsesLeft();
    }

    public int GetMaxGadgetUnits()
    {
        return gadget.GetMaxUses();
    }

    public float CheckGadgetCooldownPerc()
    {
        return gadget.GetCooldownPercLeft();
    }

    protected override void Die()
    {
        base.Die();

        Debug.Log("Player died");
    }

    public class PlayerEvent : EntityEvent
    {

        public static readonly PlayerEvent AddedCU = new PlayerEvent("AddedCU");
        public static readonly PlayerEvent SpentCU = new PlayerEvent("SpentCU");
        public static readonly PlayerEvent SpentOrbs = new PlayerEvent("SpentOrbs");
        public static readonly PlayerEvent InsufficientCU = new PlayerEvent("InsufficientCU");
        public static readonly PlayerEvent AddedGadgetUse = new PlayerEvent("AddedDash");
        public static readonly PlayerEvent RechargedGadgetUse = new PlayerEvent("RechargedDash");
        public static readonly PlayerEvent SpentGadgetUse = new PlayerEvent("SpentDash");
        public static readonly PlayerEvent ItemPicked = new PlayerEvent("ItemPicked");

        protected PlayerEvent(string name) : base(name){}
    }
}

public readonly struct PlayerState
{
    public readonly int hp, carbonUnits, gadgetUses;

    public readonly Dictionary<ItemData, int> items;

    public PlayerState(int _hp, int _carbonUnits, int _gadgetUses, Dictionary<ItemData, int> _items)
    {
        hp = _hp;
        carbonUnits = _carbonUnits;
        gadgetUses = _gadgetUses;
        items = _items;
    }
}