using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{

    public float yOrigin = 150;
    public float buffsOffset = 50;

    public GameObject UIInventoryPrefab;

    public Sprite[] buffImgs;

    Dictionary<ItemID, int> UISkills;

    private void Awake()
    {
        UISkills = new Dictionary<ItemID, int>();
    }

    public void LoadBuffs(Dictionary<ItemID, int> UISkills)
    {
        this.UISkills = UISkills;
    }

    public void AddUIItem(ItemID skill)
    {
        if (!UISkills.ContainsKey(skill))
        {
            
        }
        else
        {
            
        }
    }
}
