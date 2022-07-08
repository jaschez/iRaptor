﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    static LobbyManager instance;

    TransitionSystem transitionSystem;

    [SerializeField]
    ItemDataDictionary itemDictionary;

    WorkbenchParticles workbenchParticles;

    public delegate void UpdateOrbsCount(int currentOrbs);
    public static event UpdateOrbsCount UpdateOrbs;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (!SavingSystem.Loaded())
        {
            SavingSystem.Initialize();
        }

        if (SavingSystem.CurrentState == null)
        {
            SavingSystem.NewSave();
        }
    }

    public void Start()
    {
        transitionSystem = TransitionSystem.GetInstance();
        transitionSystem.SetTransitionColor(Color.black);
        transitionSystem.Apply(TransitionSystem.Transition.FadeIn, 1f);

        //Link each itemID to an item object
        List<ItemID> stockIDs = SavingSystem.CurrentState.StockItems;

        List<ItemData> unlockableItems = itemDictionary.GetItemsFromIDs(stockIDs);

        workbenchParticles = FindObjectOfType<WorkbenchParticles>();
        workbenchParticles.Initialize(unlockableItems);

        UIManager.GetInstance().Initialize();

        UpdateOrbsCounter();
    }

    void Update()
    {

    }

    public bool UnlockItem(ItemData item)
    {
        //Update unlocked list and remove item from stock list
        if (SavingSystem.CurrentState.Orbs >= item.Price) {
            if (!SavingSystem.CurrentState.UnlockedItems.Contains(item.ID))
            {
                SavingSystem.CurrentState.AddUnlockedItem(item.ID);
                SavingSystem.CurrentState.RemoveStockItem(item.ID);
                SavingSystem.CurrentState.SpendOrbs(item.Price);

                UpdateOrbsCounter();

                SavingSystem.Save();

                return true;
            }
            else
            {
                Debug.Log("Warning: object already bought");
                SavingSystem.CurrentState.RemoveStockItem(item.ID);
                SavingSystem.Save();
            }
        }

        return false;
    }

    void UpdateOrbsCounter()
    {
        UpdateOrbs?.Invoke(SavingSystem.CurrentState.Orbs);
    }

    public static LobbyManager GetInstance()
    {
        return instance;
    }
}
