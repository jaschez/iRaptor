using System;
using System.Collections.Generic;

public class RewardRoomGenerator : RoomGeneration
{
    public Tuple<ItemData, Coord> Reward { get; private set; }

    static List<ItemData> Items;
    static int seed = 0;

    public RewardRoomGenerator(RoomNode room, int seed, int level) : base(room, seed, level)
    {
        Initialize(RoomType.Reward, random.Next(150, 200), 3, .7f);

        //Calculation in order to not run out of Items for the run
        if (gameManager.UniversalSeed != seed)
        {
            seed = gameManager.UniversalSeed;
            Items = null;
        }


        if (Items == null)
        {
            Items = new List<ItemData>(gameManager.AvailableItems);
        }
    }

    protected override void GenerateMap()
    {
        AddEntry(new Coord(Width / 2, Height / 2));
        DefaultMapGeneration();
    }

    protected override void AdditionalGeneration()
    {
        GenerateReward();
    }

    void GenerateReward()
    {
        ItemData selectedItem;
        int maximumIndex = -1;

        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].Level > Level)
            {
                maximumIndex = i;
            }
        }

        if (maximumIndex == -1)
        {
            maximumIndex = Items.Count;
        }

        selectedItem = Items[random.Next(0, maximumIndex)];

        Reward = new Tuple<ItemData, Coord>(selectedItem, new Coord(Width / 2, Height / 2));

        if (Items.Count > 1) {
            Items.Remove(selectedItem);
        }
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Floor_Reward);
    }
}

