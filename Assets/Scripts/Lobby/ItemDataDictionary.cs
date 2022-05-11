using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDictionary", menuName = "Items/ItemDictionary")]
public class ItemDataDictionary : ScriptableObject
{
    [SerializeField]
    List<ItemPair> ItemList;

    Dictionary<ItemID, ItemData> GeneratedDictionary;

    public ItemData GetItemFromID(ItemID id)
    {
        if (GeneratedDictionary == null)
        {
            GenerateDictionary();
        }

        if (!GeneratedDictionary.ContainsKey(id))
        {
            Debug.LogError("Error: no id " + id.ToString() + " was found in the dictionary.");
            return null;
        }

        return GeneratedDictionary[id];
    }

    public List<ItemData> GetItemsFromIDs(List<ItemID> ids)
    {
        List<ItemData> items = new List<ItemData>();

        foreach (ItemID id in ids)
        {
            items.Add(GetItemFromID(id));
        }

        return items;
    }

    void GenerateDictionary()
    {
        GeneratedDictionary = new Dictionary<ItemID, ItemData>();

        foreach (ItemPair pair in ItemList)
        {
            if (!GeneratedDictionary.ContainsKey(pair.ID))
            {
                GeneratedDictionary.Add(pair.ID, pair.Data);
            }
        }
    }

    [System.Serializable]
    public struct ItemPair
    {
        public ItemData Data;
        public ItemID ID;
    }
}