using System;
using System.Collections.Generic;

public class ShopRoomGenerator : RoomGeneration
{
    public List<Tuple<int, Coord>> Items;
    public Coord NPC;

    public ShopRoomGenerator(RoomNode room, int seed, int level) : base(room, seed, level)
    {
        Initialize(RoomType.Shop, random.Next(400, 500), 3, .8f);
    }

    protected override void GenerateMap()
    {
        DefaultMapGeneration();
    }

    protected override void AdditionalGeneration()
    {
        GenerateShop();
    }

    void GenerateShop()
    {
        Items = new List<Tuple<int, Coord>>();
        NPC = new Coord(Width / 2, Height / 2);
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Shop);
    }
}
