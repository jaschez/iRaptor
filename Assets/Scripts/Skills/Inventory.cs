using System.Collections.Generic;

/*
    System for managing items obtained by player in game
 */
public class Inventory
{
    //Keeps track of every active item on the run
    public Dictionary<ItemID, int> StoredItems { get; private set; }

    public Inventory()
    {
        StoredItems = new Dictionary<ItemID, int>();
    }

    //Adds a new item to the manager
    public void AddItem(ItemID item)
    {
        if (!StoredItems.ContainsKey(item))
        {
            StoredItems.Add(item, 1);
        }
        else
        {
            StoredItems[item]++;
        }
    }

    //Check if one item is already stored
    public bool IsItemStored(ItemID item)
    {
        return StoredItems.ContainsKey(item);
    }

    //Returns how many copies of the same item are active
    public int GetItemReplicas(ItemID item)
    {
        if (IsItemStored(item))
        {
            return StoredItems[item];
        }
        else
        {
            return -1;
        }
    }
}
