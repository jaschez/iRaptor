using System;
using System.Collections.Generic;

public class ShopRoomGenerator : RoomGeneration
{
    public List<Tuple<DropType, Coord>> Items;
    public Dictionary<DropType, int> Prices;
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
        Items = new List<Tuple<DropType, Coord>>();
        NPC = new Coord(Width / 2, Height / 2);
        Prices = new Dictionary<DropType, int>();

        //Calculate prices for this level
        Prices.Add(DropType.HP, 20 + Level * 18);
        Prices.Add(DropType.ChargeUnit, 14 + Level * 11);
        Prices.Add(DropType.Item, 30 + Level * 20);

        DropType[] neededChests = new DropType[] { DropType.HP, DropType.ChargeUnit, DropType.HP};
        Coord placementStart = InterestingPoints[random.Next(0, InterestingPoints.Count)];
        ObjectPlacement chestPlacement = new ObjectPlacement(random.Next(), Map, FloorCoords, placementStart);

        for (int i = 0; i < neededChests.Length; i++)
        {
            Tuple<DropType, Coord> item = new Tuple<DropType, Coord>(neededChests[i], chestPlacement.GenerateNextPoint(3));
            Items.Add(item);
        }
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Shop);
    }
}
