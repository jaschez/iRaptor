using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModule : Entity
{

    private static Entity instance;

    private Gadget gadget;

    Dictionary<int, int> buffUses;

    List<int> currentBuffs;

    private int _carbonUnits = 0;

    //Propiedad que gestiona las unidades de carbono disponibles del jugador
    private int carbonUnits
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

        buffUses = new Dictionary<int, int>();
        currentBuffs = new List<int>();
    }

    //Método que se ejecuta al comenzar la escena
    protected override void Start()
    {
        base.Start();

        gadget = GetComponent<Gadget>();
    }

    void Update()
    {

    }

    //Método que devuelve la instancia actual del jugador
    public static Entity GetInstance()
    {
        return instance;
    }

    public PlayerState SavePlayerState()
    {
        PlayerState playerState = new PlayerState(GetHP(), carbonUnits, gadget.GetUsesLeft(), buffUses);

        return playerState;
    }

    public void LoadPlayerState(PlayerState playerState)
    {
        LoadHealth(playerState.hp);
        carbonUnits = playerState.carbonUnits;
        gadget.SetUsesLeft(playerState.gadgetUses);
        buffUses.Clear();
        currentBuffs.Clear() ;
    }

    public void ActivateBuff(int buffIndex)
    {

        if (buffUses.ContainsKey(buffIndex))
        {
            buffUses[buffIndex] += 20;
            SendEvent(PlayerEvent.BuffExtension, buffIndex);
        }
        else
        {
            buffUses.Add(buffIndex, 20);
            currentBuffs.Add(buffIndex);

            SendEvent(PlayerEvent.BuffActivation, buffIndex);
        }
    }

    public void AddCarbonUnits(int cu)
    {
        carbonUnits += cu;

        SendEvent(PlayerEvent.AddedCU, cu);
    }

    public void SpendCarbonUnits(int cu)
    {
        carbonUnits -= cu;
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

    public int GetCarbonUnits()
    {
        return carbonUnits;
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

    public void UseBuffs()
    {

        SendEvent(PlayerEvent.BuffUse);

        for (int i = 0; i < currentBuffs.Count; i++)
        {
            int currBuff = currentBuffs[i];

            buffUses[currBuff]--;

            if (buffUses[currBuff] <= 0)
            {
                buffUses.Remove(currBuff);
                currentBuffs.RemoveAt(i);

                SendEvent(PlayerEvent.EndBuff, currBuff);

                i--;
            }
        }
    }

    public int[] GetBuffIndexes()
    {
        return currentBuffs.ToArray();
    }

    protected override void Die()
    {
        base.Die();

        Debug.Log("Player died");
    }

    public class PlayerEvent : EntityEvent
    {

        public static readonly PlayerEvent AddedCU = new PlayerEvent("AddedCU");
        public static readonly PlayerEvent AddedGadgetUse = new PlayerEvent("AddedDash");
        public static readonly PlayerEvent RechargedGadgetUse = new PlayerEvent("RechargedDash");
        public static readonly PlayerEvent SpentGadgetUse = new PlayerEvent("SpentDash");
        public static readonly PlayerEvent BuffActivation = new PlayerEvent("BuffActivation");
        public static readonly PlayerEvent BuffExtension = new PlayerEvent("BuffExtension");
        public static readonly PlayerEvent BuffUse = new PlayerEvent("BuffUse");
        public static readonly PlayerEvent EndBuff = new PlayerEvent("EndBuff");

        protected PlayerEvent(string name) : base(name){}
    }
}

public readonly struct PlayerState
{
    public readonly int hp, carbonUnits, gadgetUses;

    public readonly Dictionary<int, int> buffUses;

    public PlayerState(int _hp, int _carbonUnits, int _gadgetUses, Dictionary<int, int> _buffUses)
    {
        hp = _hp;
        carbonUnits = _carbonUnits;
        gadgetUses = _gadgetUses;
        buffUses = _buffUses;
    }
}