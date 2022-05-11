using System.Collections.Generic;

//Represents all stored progress that it is used to save and load games

[System.Serializable]
public class GameState
{
    public List<ItemID> UnlockedItems { get; private set; }
    public List<ItemID> StockItems { get; private set; }

    public int Orbs { get; private set; }

    public GameState(List<ItemID> unlocked = null, List<ItemID> stock = null)
    {
        UnlockedItems = unlocked != null? unlocked : new List<ItemID>();
        StockItems = stock != null? stock : new List<ItemID>();
        Orbs = 0;
    }

    public void SpendOrbs(int spent)
    {
        Orbs -= spent;
    }

    public void AddOrbs(int added)
    {
        Orbs += added;
    }

    public void AddUnlockedItem(ItemID id)
    {
        if (!UnlockedItems.Contains(id))
        {
            UnlockedItems.Add(id);
        }
    }

    public void AddStockItem(ItemID id)
    {
        if (!StockItems.Contains(id))
        {
            StockItems.Add(id);
        }
    }

    public void RemoveStockItem(ItemID id)
    {
        if (StockItems.Contains(id))
        {
            StockItems.Remove(id);
        }
    }
}
