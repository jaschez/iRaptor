using System;
using System.Collections.Generic;

public class ShopRoom : RoomNode
{
    public List<Tuple<int, Coord>> Items;
    public Coord NPC;

    public ShopRoom(RoomNode room) : base(room)
    {

    }

    public override void Generate(RoomGeneration generator)
    {
        base.Generate(generator);
        NPC = ((ShopRoomGenerator)generator).NPC;
    }
}