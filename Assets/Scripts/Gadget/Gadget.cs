using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : MonoBehaviour
{

    protected PlayerModule playerModule;

    int maxGadgetUnits = 5;

    int spentPerUse = 1;

    private int _gadgetUnits = 0;

    protected float cooldown = .3f;
    protected float rechargeCooldown = .2f;
    protected float finishCooldownTime = 0;
    protected float rechargeCooldownTime = 0;

    protected bool canUse = true;
    protected bool exhausted = false;
    private bool locked = false;

    //Propiedad que gestiona los usos disponibles del gadget
    protected int gadgetUnits
    {
        get
        {
            return _gadgetUnits;
        }

        set
        {
            if (value < 0)
            {
                _gadgetUnits = 0;
            }
            else if (value > maxGadgetUnits)
            {
                gadgetUnits = maxGadgetUnits;
            }
            else
            {
                _gadgetUnits = value;
            }
        }
    }

    protected virtual void Start()
    {
        playerModule = GetComponent<PlayerModule>();
        SetUsesLeft(GetMaxUses());
    }

    protected virtual void Update()
    {
        if (Controls.GetGadgetKeyDown() && !locked)
        {
            if (gadgetUnits >= spentPerUse && Time.time > finishCooldownTime && !exhausted) {
                
                finishCooldownTime = Time.time + cooldown;
                //rechargeCooldownTime = finishCooldownTime + rechargeCooldown;
                Use();
            }
            else if(gadgetUnits < spentPerUse)
            {
                UIVisualizer.GetInstance().PopUp(PopUpType.Bad, "NO AMMO", transform, .6f, 25);
                SoundManager.Play(Sound.NoEnergy, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            gadgetUnits = maxGadgetUnits;
            playerModule.NotifyGadgetRecharge(gadgetUnits);
        }

        /*if (gadgetUnits < maxGadgetUnits) {
            if (finishCooldownTime < Time.time)
            {
                if (rechargeCooldownTime < Time.time)
                {
                    rechargeCooldownTime = Time.time + rechargeCooldown;
                    
                    AddGadgetUnit();
                    playerModule.NotifyGadgetRecharge(gadgetUnits);

                    if (gadgetUnits == maxGadgetUnits && exhausted)
                    {
                        UIVisualizer.GetInstance().PopUp(PopUpType.Info, "DASH RELOADED!", transform, Color.yellow, 1);
                        SoundManager.Play(Sound.Pump, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);
                        exhausted = false;
                    }
                }
            }
        }*/
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    protected abstract void Use();

    protected void SpendUse()
    {
        gadgetUnits -= spentPerUse;
        playerModule.NotifyGadgetUse(gadgetUnits);
    }

    public void AddGadgetUnit()
    {
        gadgetUnits++;
    }

    public void AddGadgetUnits(int units)
    {
        gadgetUnits += units;
    }

    public int GetUsesLeft()
    {
        return gadgetUnits;
    }

    public void SetUsesLeft(int uses)
    {
        gadgetUnits = uses;
    }

    public int GetMaxUses()
    {
        return maxGadgetUnits;
    }

    public void SetMaxUses(int maxUse)
    {
        maxGadgetUnits = maxUse;
    }

    public void SetSpentPerUse(int spent)
    {
        spentPerUse = spent;
    }

    public int GetSpentPerUse()
    {
        return spentPerUse;
    }

    public float GetCooldownPercLeft()
    {
        float perc = 0;

        if (finishCooldownTime > Time.time)
        {
            perc = (finishCooldownTime - Time.time) / cooldown;
        }

        return perc;
    }
}
