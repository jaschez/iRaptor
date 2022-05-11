using UnityEngine;

[CreateAssetMenu(fileName = "NewGameItem", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string Name = "Default Item";
    public string Description = "Default Description";
    public int Price = 3;
    public int Level = 0;

    public Sprite Image;

    public ItemID ID;
}

public enum ItemID
{
    Drill,
    Molotovic
}