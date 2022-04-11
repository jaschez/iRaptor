using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject UIItemPrefab;

    Dictionary<ItemID, GameObject> UIItems;

    private void Awake()
    {
        UIItems = new Dictionary<ItemID, GameObject>();
    }

    public void LoadItems(Dictionary<ItemID, GameObject> UIItems)
    {
        this.UIItems = UIItems;
    }

    public void AddUIItem(ItemData item)
    {
        if (!UIItems.ContainsKey(item.ID))
        {
            GameObject uiItem = Instantiate(UIItemPrefab, transform);

            uiItem.GetComponent<Image>().sprite = item.Image;

            UIItems.Add(item.ID, uiItem);
        }
    }
}
