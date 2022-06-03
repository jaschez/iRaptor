using System;
using System.Collections.Generic;

public class ShopRoom : RoomNode
{
    public List<Tuple<DropType, Coord>> Items;
    public Dictionary<DropType, int> Prices;
    public Coord NPC;

    public ShopRoom(RoomNode room) : base(room)
    {

    }

    public override void Generate(RoomGeneration generator)
    {
        base.Generate(generator);
        ShopRoomGenerator shopRoomGenerator = (ShopRoomGenerator)generator;

        NPC = shopRoomGenerator.NPC;
        Items = shopRoomGenerator.Items;
        Prices = shopRoomGenerator.Prices;
    }
}