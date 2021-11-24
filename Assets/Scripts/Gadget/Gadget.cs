using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gadget : MonoBehaviour
{

    protected PlayerModule playerModule;

    int maxGadgetUnits = 5;

    int spentPerUse = 1;

    private int _gadgetUnits = 0;

    protected float cooldown = .5f;
    protected float finishCooldownTime = 0;

    protected bool canUse = true;

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
    }

    protected virtual void Update()
    {
        if (Controls.GetDashKeyDown())
        {
            if (gadgetUnits >= spentPerUse && canUse) {
                Use();
            }
            else
            {
                if (gadgetUnits < spentPerUse && canUse) {
                    UIVisualizer.GetInstance().PopUp(PopUpType.Bad, "No energy", transform, .6f, 15);
                    SoundManager.Play(Sounds.NoEnergy, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);
                }

                CamManager.GetInstance().ShakeSingle(5f);
            }
        }    
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
